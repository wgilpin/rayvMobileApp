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

	class GeoLocation
	{
		public string Name { get; set; }

		public Double Lat { get; set; }

		public Double Lng { get; set; }

	}

	public class AddPage1 : ContentPage
	{
		ListView LocationResultsView;
		ListView PlacesListView;
		Label NothingFound;
		ActivityIndicator Spinner;
		public Position SearchPosition;
		EntryWithChangeButton PlaceNameBox;
		EntryWithChangeButton LocationEditBox;
		LabelWithChangeButton LocationSearchedBox;
		Button ResetLocationBtn;
		Button AddManualAddress;
		List<GeoLocation> LocationList;
		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();

		#region Events

		void DoChangeLocation (object s, EventArgs e)
		{
			LocationSearchedBox.IsVisible = false;
			ResetLocationBtn.IsVisible = false;
			LocationEditBox.IsVisible = true;
			PlaceNameBox.ButtonText = " ";
		}

		void DoSearchForPlace (object s, EventArgs e)
		{
			DoSearch (PlaceNameBox.Text, "");
		}

		void DoFindLocation (object sender, EventArgs e)
		{
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Parameters parameters = new Parameters ();
				parameters ["address"] = LocationEditBox.Text;
				try {
					string result = restConnection.Instance.get ("/api/geocode", parameters).Content;
					JObject obj = JObject.Parse (result);
					//obj["results"][1]["formatted_address"].ToString()
					LocationList = new List<GeoLocation> ();
					int count = obj ["results"].Count ();
					if (count == 0) {
						NothingFound.IsVisible = true;
					} else {
						Double placeLat;
						Double placeLng;
						for (int idx = 0; idx < count; idx++) {
							Double.TryParse (
								obj ["results"] [idx] ["geometry"] ["location"] ["lat"].ToString (), out placeLat);
							Double.TryParse (
								obj ["results"] [idx] ["geometry"] ["location"] ["lng"].ToString (), out placeLng);
							LocationList.Add (
								new GeoLocation {
									Name = obj ["results"] [idx] ["formatted_address"].ToString (),
									Lat = placeLat,
									Lng = placeLng,
								});
						}
						Device.BeginInvokeOnMainThread (() => {
							Spinner.IsRunning = false;
							LocationResultsView.ItemsSource = LocationList;
							LocationResultsView.IsVisible = true;
							ResetLocationBtn.IsVisible = true;
							LocationSearchedBox.IsVisible = false;
							LocationEditBox.IsVisible = true;
							NothingFound.IsVisible = false;
							PlacesListView.IsVisible = false;
						});
					}
				} catch (Exception ex) {
					Insights.Report (ex);
				}

			})).Start ();
		}

		void DoSelectPlace (object s, ItemTappedEventArgs e)
		{
			Place p = (Place)e.Item;
			// get google db stuff
			Parameters parameters = new Parameters ();
			parameters ["place_id"] = p.place_id;
			try {
				string result = restConnection.Instance.get ("/api/place_details", parameters).Content;
				JObject obj = JObject.Parse (result);
				if (obj ["website"] != null)
					p.website = obj ["website"].ToString ();
				if (obj ["telephone"] != null)
					p.telephone = obj ["telephone"].ToString ();
				Debug.WriteLine ("AddPage1.DoSelectPlace Push EditPage");
				var editPage = new EditPage (p, addingNewPlace: true);
				editPage.Saved += (sender, ev) => {
					this.Navigation.PushModalAsync (new NavigationPage (new DetailPage (ev.EditedPlace, true)));
				};
				this.Navigation.PushAsync (editPage);
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
			LocationEditBox.IsVisible = false;
			LocationResultsView.IsVisible = false;
			DoSearch (PlaceNameBox.Text, "");
			PlaceNameBox.ButtonText = "Search";
		}

		void DoSelectLocation (object s, ItemTappedEventArgs e)
		{
			GeoLocation loc = (GeoLocation)e.Item;
			LocationSearchedBox.Text = loc.Name;
			LocationSearchedBox.IsVisible = true;
			ResetLocationBtn.IsVisible = true;
			LocationEditBox.IsVisible = false;
			SearchPosition = new Position (loc.Lat, loc.Lng);
			DoSearch (PlaceNameBox.Text, LocationSearchedBox.Text);
			PlacesListView.IsVisible = false;
			LocationResultsView.IsVisible = false;
			PlaceNameBox.ButtonText = "Search";
		}

		void DoSuccess (object o, EventArgs e)
		{
			Navigation.PopAsync ();
		}

		void DoFail (object o, EventArgs e)
		{
			;
		}

		#endregion

		#region Methods

		void SetupSearchHistory ()
		{

		}

		void DoSearch (String searchName, String searchLocation)
		{

			Console.WriteLine ("AddPage1.DoSearch: Activity");
			Spinner.IsRunning = true;
			PlaceNameBox.Entry.Unfocus ();
			LocationEditBox.Entry.Unfocus ();
			if (PlaceNameBox.ButtonText == "Search" && LocationSearchedBox.ButtonText == "Search") {
				PlaceNameBox.ButtonText = " ";
			}
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Console.WriteLine ("AddPage1.DoSearch: Thread");
				Dictionary<string, string> parameters = new Dictionary<string, string> ();
				parameters ["lat"] = SearchPosition.Latitude.ToString ();
				parameters ["lng"] = SearchPosition.Longitude.ToString ();
				if (searchLocation != null) {
					parameters ["addr"] = searchLocation;
				}
				if (PlaceNameBox.Text != null) {
					parameters ["place_name"] = searchName;
				}
				parameters ["near_me"] = "1";
				try {
					string result = restConnection.Instance.get ("/getAddresses_ajax", parameters).Content;
					JObject obj = JObject.Parse (result);
					List<Place> points = JsonConvert.DeserializeObject<List<Place>> (obj.SelectToken ("local.points").ToString ());
					foreach (Place point in points) {
						if (point == null)
							continue;
						Console.WriteLine (point.place_name);
						point.CalculateDistanceFromPlace ();
					}
					Console.WriteLine ("AddPage1.DoSearch SORT");

					points.Sort ();
					if (!String.IsNullOrEmpty (searchLocation)) {
						Persist.Instance.SearchHistory.Add (searchLocation, unique: true);
						Console.WriteLine ("DoSearch: SearchHistory += {0}", searchLocation);
					}
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("AddPage1.DoSearch: MainThread");
						SetupSearchHistory ();
						Spinner.IsRunning = false;
						Console.WriteLine ("AddPage1.DoSearch: Activity Over. push AddResultsPage");
						PlacesListView.ItemsSource = points;
						PlacesListView.IsVisible = points.Count > 0;
						NothingFound.IsVisible = points.Count == 0;
						AddManualAddress.IsVisible = true;
					});
				} catch (Exception e) {
					restConnection.LogErrorToServer ("AddPage1.DoSearch: Exception {0}", e);
					Device.BeginInvokeOnMainThread (async() => {
						Console.WriteLine ("AddPage1.DoSearch: MainThread Exception");
						Spinner.IsRunning = false;
						var editAsDraft = await DisplayAlert ("No Network", "Unable to search. Network problems?", "Edit as draft", "Cancel");
						if (editAsDraft) {
							Persist.Instance.Online = false;
							var editPage = new EditPage (editAsDraft: true, addingNewPlace: true);
							editPage.Saved += (o, ev) => {
								this.Navigation.PushModalAsync (new NavigationPage (new ListPage ()));
							};
							await this.Navigation.PushAsync (editPage);
						}
					});
				}
			})).Start ();
		}

		#endregion

		#region Properties

		public  IEnumerable ItemsSource {
			set {
				PlacesListView.ItemsSource = value;
				NothingFound.IsVisible = (value as List<Place>).Count == 0;
			}
		}

		#endregion

		public AddPage1 ()
		{
			Analytics.TrackPage ("AddPage1");
			Title = "Add a Place";
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				IsVisible = false,
			};

			Spinner = new ActivityIndicator {
				IsRunning = false,
				Color = Color.Red,
			};

			ResetLocationBtn = new Button {
				Text = "  return to current location",
				HorizontalOptions = LayoutOptions.Start,
				IsVisible = false,
				TextColor = Color.White,
			};
			ResetLocationBtn.Clicked += DoResetLocation;

			PlacesListView = new PlacesListView (showVotes: false) {
//				ItemsSource = Persist.Instance.Places,
				IsVisible = false,
			};
			PlacesListView.ItemTapped += DoSelectPlace;

			LocationResultsView = new ListView {
				IsVisible = false,
			};
			LocationResultsView.ItemTapped += DoSelectLocation;
			LocationResultsView.ItemTemplate = new DataTemplate (typeof(TextCell));
			LocationResultsView.ItemTemplate.SetBinding (TextCell.TextProperty, "Name");

			PlaceNameBox = new EntryWithChangeButton {
				PlaceHolder = "Search for a place",
				OnClick = DoSearchForPlace,
				ButtonText = "Search",
			};
			PlaceNameBox.Entry.Completed += (sender, e) => {
				PlaceNameBox.Entry.Unfocus ();
				DoSearchForPlace (sender, e);
			};
			LocationSearchedBox = new LabelWithChangeButton {
				Text = "Searching current location",
				OnClick = DoChangeLocation,
			};
			LocationEditBox = new EntryWithChangeButton {
				PlaceHolder = "Where? e.g. Town or Address",
				OnClick = DoFindLocation,
				ButtonText = "Search",
				IsVisible = false,
			};
			LocationEditBox.Entry.Completed += (sender, e) => {
				LocationEditBox.Entry.Unfocus ();
				DoFindLocation (sender, e);
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
			StackLayout menu = new StackLayout { 
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Spacing = 10,
				Padding = 4,
				BackgroundColor = settings.BaseColor,
				Children = {
					PlaceNameBox,
					LocationSearchedBox,
					LocationEditBox,
					ResetLocationBtn,
					Spinner,
					LocationResultsView,
					NothingFound,
				}
			};
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					menu,
					PlacesListView,
					AddManualAddress,
					tools
				}
			};
		}
	}
}


