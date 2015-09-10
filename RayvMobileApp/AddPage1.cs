using System;

using Xamarin.Forms;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms.Maps;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xamarin;
using System.Linq;
using System.Diagnostics;

namespace RayvMobileApp
{

	using Parameters = Dictionary<string, string>;

	public class GeoLocation
	{
		public string Name { get; set; }

		public Double Lat { get; set; }

		public Double Lng { get; set; }

	}

	public class AddPage1 : ContentPage
	{
		public event EventHandler Cancelled;

		LocationListWithHistory LocationsListBox;
		PlacesListView PlacesLV;
		Label NothingFound;
		ActivityIndicator Spinner;
		public Position SearchPosition;
		EntryWithChangeButton PlaceNameBox;
		LabelWithChangeButton LocationSearchedBox;
		Button ResetLocationBtn;
		Button AddManualAddress;
		//		List<GeoLocation> LocationList;
		//		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();
		Place addingPlace;
		//		bool editAsDraft;
		StackLayout menu;
		BottomToolbar tools;

		#region Events

		void DoChangeLocation (object s, EventArgs e)
		{
			LocationsListBox.IsVisible = true;
			LocationSearchedBox.IsVisible = false;
			ResetLocationBtn.IsVisible = false;
			PlaceNameBox.ButtonText = " ";
			PlacesLV.IsVisible = false;
		}

		void DoSearchForPlace (object s, EventArgs e) => DoSearch ( "");

		void DoSelectPlace (object s, ItemTappedEventArgs e)
		{
			// a place has been selected from the results. Make a skeleton Place and edit it
			if (e == null || e.Item == null) {
				Insights.Track ("AddPage1.DoSelectPlace No Item");
				return;
			}
			addingPlace = (Place)e.Item;
			// get google db stuff
			Parameters parameters = new Parameters ();
			parameters ["place_id"] = addingPlace.place_id;
			try {
				var restResult = restConnection.Instance.get ("/api/place_details", parameters);
				if (restResult == null) {
					if (DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ()) {
						// make a simulated result
						addingPlace.website = "www.madeup_place.com";
						addingPlace.telephone = "";
					} else
						throw new ApplicationException ("DoSelectPlace: Null result from /api/place_details");
				} else {
					// we have a result from the server
					string result = restResult.Content;
					JObject obj = JObject.Parse (result);
					addingPlace.website = (obj ["website"] ?? "").ToString ();
					addingPlace.telephone = (obj ["telephone"] ?? "").ToString ();
				}
				Debug.WriteLine ("AddPage1.DoSelectPlace Push EditPage");
				var editor = new PlaceEditor (addingPlace, false);
				editor.Saved += (sender, ev) => {
					Console.WriteLine ("Editor Returned - Saving");
					string errorMessage = "";
					addingPlace.Save (out errorMessage);
					Device.BeginInvokeOnMainThread (() => {
						if (string.IsNullOrEmpty (errorMessage)) {
							Console.WriteLine ($"Editor Returned - Loading DetailPage for {addingPlace.place_name}");
							Navigation.PushModalAsync (new RayvNav (new DetailPage (addingPlace, showToolbar: true)));
						} else {
							DisplayAlert ("Error",$"Couldn't save {addingPlace.place_name}","OK"); 
						}
					});
				};
				editor.Cancelled += (sender, ev) => Navigation.PopModalAsync ();
				Navigation.PushModalAsync (editor);
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		void DoResetLocation (object s, EventArgs e)
		{
			SearchPosition = Persist.Instance.GpsPosition;
			ResetLocationBtn.IsVisible = false;
			LocationSearchedBox.IsVisible = true;
			LocationSearchedBox.Text = "Searching current location";
			LocationsListBox.IsVisible = false;
			DoSearch ("");
			PlaceNameBox.ButtonText = " Search ";
		}

		void DoSelectLocation (object s, ItemTappedEventArgs e)
		{
			GeoLocation loc = (GeoLocation)e.Item;
			LocationSearchedBox.Text = loc.Name;
			LocationSearchedBox.IsVisible = true;
			ResetLocationBtn.IsVisible = true;
			SearchPosition = new Position (loc.Lat, loc.Lng);
			DoSearch (LocationSearchedBox.Text);
			PlacesLV.IsVisible = true;
			LocationsListBox.IsVisible = false;
			PlaceNameBox.ButtonText = " Search ";
		}

		void DoSuccess (object o, EventArgs e) => Navigation.PopAsync ();

		void DoFail (object o, EventArgs e)
		{
			
		}

		#endregion

		#region Methods

		void SetupSearchHistory ()
		{

		}

		// Search for a place at a location
		void DoSearch (String searchLocation)
		{
			Spinner.IsRunning = true;
			Spinner.IsVisible = true;
			PlaceNameBox.Entry.Unfocus ();
			if (PlaceNameBox.ButtonText == " Search " && LocationSearchedBox.ButtonText == " Search ") {
				PlaceNameBox.ButtonText = " ";
			}

			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				try {
					Console.WriteLine ("AddPage1.DoSearch: Activity");
					Console.WriteLine ("AddPage1.DoSearch: Thread");
					Dictionary<string, string> parameters = new Dictionary<string, string> ();
					parameters ["lat"] = SearchPosition.Latitude.ToString ();
					parameters ["lng"] = SearchPosition.Longitude.ToString ();
					if (searchLocation != null) {
						parameters ["addr"] = searchLocation;
					}
					if (!string.IsNullOrEmpty (PlaceNameBox.Text)) {
						parameters ["place_name"] = PlaceNameBox.Text;
					}
					parameters ["near_me"] = "1";
					var restResult = restConnection.Instance.get ("/getAddresses_ajax", parameters, timeout: 30000);
					if (restResult == null) {
						throw new SystemException ("DoSearch: No Server Response");
					} else {
						string result = restResult.Content;
						JObject obj = JObject.Parse (result);
						List<Place> points = new List<Place> ();
						List<Place> pointsIn = JsonConvert.DeserializeObject<List<Place>> (obj.SelectToken ("local.points").ToString ());
						foreach (Place point in pointsIn) {
							if (point == null)
								continue;
							point.CalculateDistanceFromPlace (SearchPosition);
							points.Add (point);
						}
						pointsIn = null;
						points.Sort ();
						foreach (Place p in points.Take (30))
							if (p != null)
								Console.WriteLine ($"{p.place_name}, {p.distance}");

						Console.WriteLine ($"Search Position {SearchPosition.Latitude} {SearchPosition.Longitude}");
						Console.WriteLine ($"AddPage1.DoSearch N={points.Count}");
						Console.WriteLine ("AddPage1.DoSearch: MainThread");
						Device.BeginInvokeOnMainThread (() => {
			
							SetupSearchHistory ();
							Spinner.IsRunning = false;
							Console.WriteLine ("AddPage1.DoSearch: Activity Over. source set");
							PlacesLV.ItemsSource = null;
							PlacesLV.ItemsSource = points.Take (30).ToList ();

							NothingFound.IsVisible = points.Count == 0;
							PlacesLV.IsVisible = !NothingFound.IsVisible;
							Console.WriteLine ($"AddPage1.DoSearch Visible {PlacesLV.IsVisible}");
							AddManualAddress.IsVisible = true;
							Spinner.IsRunning = false;
							Spinner.IsVisible = false;
						});
					}
				} catch (Exception e) {
					Insights.Report (e, "SearchLocation", searchLocation);
					Console.WriteLine ($"AddPage1.DoSearch: MainThread {e}");
					Device.BeginInvokeOnMainThread (() => {
						Spinner.IsRunning = false;
						DisplayAlert ("No Response", "Unable to search. Network problems?", "OK");
					});
				}
			})).Start ();
		}

		#endregion

		#region Properties


		#endregion

		void DoCancel ()
		{
			Cancelled?.Invoke (null, null);
		}

		public AddPage1 (bool hasBackButton = false)
		{
			Analytics.TrackPage ("AddPage1");
			Title = "Add a Place";
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				IsVisible = false,
			};

			Spinner = new ActivityIndicator {
				IsRunning = false,
				Color = Color.White,
			};

			ResetLocationBtn = new Button {
				Text = "  return to current location",
				HorizontalOptions = LayoutOptions.Start,
				IsVisible = false,
				TextColor = Color.White,
			};
			ResetLocationBtn.Clicked += DoResetLocation;

			PlacesLV = new PlacesListView (showVotes: false) {
//				ItemsSource = Persist.Instance.Places,
				IsVisible = false,
			};
			PlacesLV.ItemTapped += DoSelectPlace;

			LocationsListBox = new LocationListWithHistory {
				IsVisible = false,
			};
			LocationsListBox.OnItemTapped = DoSelectLocation;

			LocationsListBox.OnCancel = (s, e) => {
				LocationsListBox.IsVisible = false;
				LocationSearchedBox.IsVisible = true;
				DoResetLocation (this, null);
				ResetLocationBtn.IsVisible = true;
				PlaceNameBox.ButtonText = " Search ";
				ResetLocationBtn.IsVisible = false;
			};


			PlaceNameBox = new EntryWithChangeButton {
				PlaceHolder = "Search for a place",
				OnClick = DoSearchForPlace,
				ButtonText = " Search ",
			};
			PlaceNameBox.Entry.Completed += (sender, e) => {
				PlaceNameBox.Entry.Unfocus ();
				DoSearchForPlace (sender, e);
			};
			LocationSearchedBox = new LabelWithChangeButton {
				Text = "Searching current location",
				OnClick = DoChangeLocation,
			};
			AddManualAddress = new RayvButton {
				HeightRequest = 30,
				Text = "Add unlisted place",
				OnClick = (s, e) => {
					AddPage4_Map addMapPage = new AddPage4_Map (SearchPosition);
					addMapPage.Succeeded += DoSuccess;
					addMapPage.Failed += DoFail;
					this.Navigation.PushAsync (addMapPage);
				},
				IsVisible = false,
			};
			SearchPosition = Persist.Instance.GpsPosition;

			menu = new StackLayout { 
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Spacing = 10,
				Padding = 4,
				BackgroundColor = settings.BaseColor,
				Children = {
					PlaceNameBox,
					LocationSearchedBox,
					ResetLocationBtn,
					Spinner,
					LocationsListBox,
					NothingFound,
				}
			};
			tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					menu,
					PlacesLV,
					AddManualAddress,
//					tools
				}
			};
			if (hasBackButton) {
				ToolbarItems.Add (new ToolbarItem {
					Text = "Cancel",
					//				Icon = "187-pencil@2x.png",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						DoCancel ();
					})
				});
			}
			this.Appearing += DoSearchForPlace;
		}
	}
}


