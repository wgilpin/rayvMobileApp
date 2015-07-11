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
		LocationListWithHistory GeoLookupBox;
		PlacesListView PlacesLV;
		Label NothingFound;
		ActivityIndicator Spinner;
		public Position SearchPosition;
		EntryWithChangeButton PlaceNameBox;
		LabelWithChangeButton LocationSearchedBox;
		Button ResetLocationBtn;
		Button AddManualAddress;
		List<GeoLocation> LocationList;
		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();
		Place addingPlace;
		bool editAsDraft;
		Frame SearchHereBtn;

		#region Events

		void DoChangeLocation (object s, EventArgs e)
		{
			GeoLookupBox.IsVisible = true;
			LocationSearchedBox.IsVisible = false;
			ResetLocationBtn.IsVisible = false;
			PlaceNameBox.ButtonText = " ";
			SearchHereBtn.IsVisible = false;
		}

		void DoSearchForPlace (object s, EventArgs e) => DoSearch (PlaceNameBox.Text, "");






		void DoSelectPlace (object s, ItemTappedEventArgs e)
		{
			if (e == null || e.Item == null) {
				Insights.Track ("AddPage1.DoSelectPlace No Item");
				return;
			}
			addingPlace = (Place)e.Item;
			// get google db stuff
			Parameters parameters = new Parameters ();
			parameters ["place_id"] = addingPlace.place_id;
			try {
				string result = restConnection.Instance.get ("/api/place_details", parameters).Content;
				JObject obj = JObject.Parse (result);
				if (obj ["website"] != null)
					addingPlace.website = obj ["website"].ToString ();
				if (obj ["telephone"] != null)
					addingPlace.telephone = obj ["telephone"].ToString ();
				Debug.WriteLine ("AddPage1.DoSelectPlace Push EditPage");
				var editor = new PlaceEditor (addingPlace, this, false);
				editor.Edit ();
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
			GeoLookupBox.IsVisible = false;
			DoSearch (PlaceNameBox.Text, "");
			PlaceNameBox.ButtonText = "Search";
			SearchHereBtn.IsVisible = true;
		}

		void DoSelectLocation (object s, ItemTappedEventArgs e)
		{
			GeoLocation loc = (GeoLocation)e.Item;
			LocationSearchedBox.Text = loc.Name;
			LocationSearchedBox.IsVisible = true;
			ResetLocationBtn.IsVisible = true;
			SearchPosition = new Position (loc.Lat, loc.Lng);
			DoSearch (PlaceNameBox.Text, LocationSearchedBox.Text);
			PlacesLV.IsVisible = false;
			GeoLookupBox.IsVisible = false;
			PlaceNameBox.ButtonText = "Search";
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
		void DoSearch (String searchName, String searchLocation)
		{
			Console.WriteLine ("AddPage1.DoSearch: Activity");
			Spinner.IsRunning = true;
			PlaceNameBox.Entry.Unfocus ();
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
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("AddPage1.DoSearch: MainThread");
						SetupSearchHistory ();
						Spinner.IsRunning = false;
						Console.WriteLine ("AddPage1.DoSearch: Activity Over. push AddResultsPage");
						PlacesLV.ItemsSource = points;
						PlacesLV.IsVisible = points.Count > 0;
						NothingFound.IsVisible = points.Count == 0;
						AddManualAddress.IsVisible = true;
					});
				} catch (Exception e) {
					restConnection.LogErrorToServer ("AddPage1.DoSearch: Exception {0}", e);
					Device.BeginInvokeOnMainThread (async() => {
						Console.WriteLine ("AddPage1.DoSearch: MainThread Exception");
						Spinner.IsRunning = false;
						editAsDraft = await DisplayAlert ("No Network", "Unable to search. Network problems?", "Edit as draft", "Cancel");
						if (editAsDraft) {
							Persist.Instance.Online = false;
							var editor = new PlaceEditor (addingPlace, this, isDraft: true);
						}
					});
				}
			})).Start ();
		}

		#endregion

		#region Properties

		public  IEnumerable ItemsSource {
			set {
				PlacesLV.ItemsSource = value;
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

			PlacesLV = new PlacesListView (showVotes: false) {
//				ItemsSource = Persist.Instance.Places,
				IsVisible = false,
			};
			PlacesLV.ItemTapped += DoSelectPlace;

			GeoLookupBox = new LocationListWithHistory {
				IsVisible = false,
			};
			GeoLookupBox.OnItemTapped = DoSelectLocation;
			var searchBtn = new RayvButton ("Search Here") {
				BackgroundColor = ColorUtil.Darker (settings.BaseColor),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BorderRadius = 0
			};
			GeoLookupBox.OnCancel = (s, e) => {
				GeoLookupBox.IsVisible = false;
				LocationSearchedBox.IsVisible = true;
				DoResetLocation (this, null);
				ResetLocationBtn.IsVisible = true;
				PlaceNameBox.ButtonText = "Search";
				SearchHereBtn.IsVisible = true;
				ResetLocationBtn.IsVisible = false;
			};
			searchBtn.OnClick += (s, e) => {
				SearchHereBtn.IsVisible = false;
				DoSearchForPlace (s, e);
			};
			SearchHereBtn = new Frame {
				BackgroundColor = Color.White,
				HasShadow = false,
				OutlineColor = Color.White,
				Padding = 0,
				Content = searchBtn,
			};
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
					ResetLocationBtn,
					Spinner,
					SearchHereBtn,
					GeoLookupBox,
					NothingFound,
				}
			};
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					menu,
					PlacesLV,
					AddManualAddress,
					tools
				}
			};
		}
	}
}


