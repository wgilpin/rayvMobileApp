﻿using System;

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

		StackLayout SearchContent;
		StackLayout MapContent;
		bool MapMode = false;
		Place SelectedPlace;

		//Search Content
		LocationListWithHistory LocationsListBox;
		PlacesListView PlacesLV;
		Label NothingFound;
		ActivityIndicator Spinner;
		public Position SearchPosition;
		EntryWithChangeButton PlaceNameBox;
		LabelWithChangeButton LocationSearchedBox;
		Button ResetLocationBtn;
		Button AddManualAddress;
		StackLayout menu;
		List<Place> points;
		Button CancelBtn;

		//MapContent
		PlacesListView MapLV;
		Map map;
		LabelWide placeLabel;
		LabelWide addressLabel;

		#region Events

		void DoTapItem (object sender, ItemTappedEventArgs e)
		{
			Console.WriteLine ($"tapped {(e.Item as Place).place_name}");
			SetDisplayMode (IsMapMode: true);
			map.Pins.Clear ();
			SelectedPlace = (e.Item as Place);
			var tappedPlacePosition = SelectedPlace.GetPosition ();
			placeLabel.Text = SelectedPlace.place_name;
			addressLabel.Text = SelectedPlace.address;
			map.Pins.Add (new Pin{ Position = tappedPlacePosition, Type = PinType.Place, Label = SelectedPlace.place_name });
			map.MoveToRegion (MapSpan.FromCenterAndRadius (
				tappedPlacePosition, Distance.FromMiles (0.1)));
			var mapList = new List<Place> ();
			foreach (var pl in points) {
				if (string.IsNullOrEmpty (SelectedPlace.key)) {
					if (SelectedPlace.place_id != pl.place_id)
						mapList.Add (pl);
				} else if (pl.key != SelectedPlace.key) {
					mapList.Add (pl);
				}
			}
			MapLV.DisplayedList.ItemsSource = mapList;
		}

		void DoChangeLocation (object s, EventArgs e)
		{
			LocationsListBox.IsVisible = true;
			LocationSearchedBox.IsVisible = false;
			ResetLocationBtn.IsVisible = false;
			PlaceNameBox.ButtonText = " ";
			PlacesLV.IsVisible = false;
		}

		void DoSearchForPlace (object s, EventArgs e) => DoSearch ( "");

		void DoSelectPlace (object s, EventArgs e)
		{
			// a place has been selected from the results. Make a skeleton Place and edit it
			if (SelectedPlace == null) {
				Insights.Track ("AddPage1.DoSelectPlace No Item");
				return;
			}

			// get google db stuff
			Parameters parameters = new Parameters ();
			parameters ["place_id"] = SelectedPlace.place_id;
			try {
				var restResult = Persist.Instance.GetWebConnection ().get ("/api/place_details", parameters);
				if (restResult == null) {
					if (DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ()) {
						// make a simulated result
						SelectedPlace.website = "www.madeup_place.com";
						SelectedPlace.telephone = "";
					} else
						throw new ApplicationException ("DoSelectPlace: Null result from /api/place_details");
				} else {
					// we have a result from the server
					string result = restResult.Content;
					JObject obj = JObject.Parse (result);
					SelectedPlace.website = (obj ["website"] ?? "").ToString ();
					SelectedPlace.telephone = (obj ["telephone"] ?? "").ToString ();
				}
				Debug.WriteLine ("AddPage1.DoSelectPlace Push EditPage");
				var editor = new PlaceEditor (SelectedPlace, false);
				editor.Saved += (sender, ev) => {
					Console.WriteLine ("Editor Returned - Saving");
					string errorMessage = "";
					SelectedPlace.Save (out errorMessage);
					Device.BeginInvokeOnMainThread (() => {
						if (string.IsNullOrEmpty (errorMessage)) {
							Console.WriteLine ($"Editor Returned - Loading DetailPage for {SelectedPlace.place_name}");
							Navigation.PushModalAsync (
								new RayvNav (new DetailPage (
									place: SelectedPlace, 
									showSave: true, 
									showToolbar: true)));
						} else {
							DisplayAlert ("Error",$"Couldn't save {SelectedPlace.place_name}","OK"); 
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

		void SetDisplayMode (bool IsMapMode)
		{
			if (IsMapMode) {
				//asking for map mode
				if (MapMode)
					// already showing map
					return;
				else {
					Content = GetMapContent ();
					MapMode = true;
				}
			} else {
				// stop map mode
				MapMode = false;
				Content = SearchContent;
			}
		}

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
					if (SearchPosition.Latitude == 0 && SearchPosition.Longitude == 0)
						SearchPosition = Persist.Instance.GpsPosition;
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
					var restResult = Persist.Instance.GetWebConnection ().get ("/getAddresses_ajax", parameters, timeout: 30000);
					if (restResult == null) {
						throw new SystemException ("DoSearch: No Server Response");
					} else {
						string result = restResult.Content;
						JObject obj = JObject.Parse (result);
						points = new List<Place> ();
						List<Place> pointsIn = JsonConvert.DeserializeObject<List<Place>> (obj.SelectToken ("local.points").ToString ());
						foreach (Place point in pointsIn) {
							if (point == null)
								continue;
							point.CalculateDistanceFromPlace (SearchPosition);
							points.Add (point);
						}
						pointsIn = null;
						points.Sort ();
						points = points.Take (20).ToList ();

						Console.WriteLine ($"Search Position {SearchPosition.Latitude} {SearchPosition.Longitude}");
						Console.WriteLine ($"AddPage1.DoSearch N={points.Count}");
						Console.WriteLine ("AddPage1.DoSearch: MainThread");
						Device.BeginInvokeOnMainThread (() => {
			
							SetupSearchHistory ();
							Spinner.IsRunning = false;
							Console.WriteLine ("AddPage1.DoSearch: Activity Over. source set");
							PlacesLV.DisplayedList.ItemsSource = null;
							PlacesLV.DisplayedList.ItemsSource = points.Take (30).ToList ();

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

		void DoCancel (object s, EventArgs e)
		{
			if (MapMode)
				SetDisplayMode (IsMapMode: false);
			else
				Cancelled?.Invoke (null, null);
		}

		StackLayout GetSearchContent ()
		{
			BackgroundColor = settings.BaseColor;
			CancelBtn = new Button { 
				Text = "< Back", 
				TextColor = Color.White, 
				HorizontalOptions = LayoutOptions.Start,
				HeightRequest = 22,
			};
			CancelBtn.Clicked += DoCancel;
			var AddBtn = new Label {
				Text = "Add", 
				TextColor = Color.White, 
				FontSize = settings.FontSizeLabelLarge,
				HorizontalOptions = LayoutOptions.Center, 
			};
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
				BackgroundColor = Color.White
			};
			PlacesLV.OnItemTapped = DoTapItem;
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
					CancelBtn,
					AddBtn,
					PlaceNameBox,
					LocationSearchedBox,
					ResetLocationBtn,
					Spinner,
					LocationsListBox,
					NothingFound,
				}
			};
			return new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					menu,
					PlacesLV,
					AddManualAddress,
				}
			};
		}

		StackLayout GetMapContent ()
		{
			if (MapContent != null)
				return MapContent;
			placeLabel = new LabelWide (){ FontSize = settings.FontSizeLabelMedium };
			addressLabel = new LabelWide (){ FontSize = settings.FontSizeLabelSmall };
			StackLayout detailView = new StackLayout {
				Padding = 5,
				BackgroundColor = Color.White, 
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = { placeLabel, addressLabel }
			};
			map = new Map (
				MapSpan.FromCenterAndRadius (
					SearchPosition, Distance.FromMiles (0.1))) {
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};
			var saveButton = new ButtonWithImage () { 
				Text = "Next",
				BackgroundColor = ColorUtil.Darker (settings.BaseColor),
				FontSize = settings.FontSizeButtonLarge, 
				ImageSource = "forward_1.png" 
			};
			saveButton.OnClick = DoSelectPlace;

			MapLV = new PlacesListView (showVotes: false, showDistance: true) { 
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.White
			};
			MapLV.OnItemTapped = DoTapItem;
			MapContent = new StackLayout {
				Spacing = 1,
				Children = {
					CancelBtn,
					detailView,
					map,
					new Label { Text = "Nearby Results", FontSize = settings.FontSizeLabelMedium },
					saveButton,
					MapLV
				}
			};
			return MapContent;
		}

		public AddPage1 (bool hasBackButton = false)
		{
			Analytics.TrackPage ("AddPage1");
			Title = "";
			Padding = new Thickness (2, Device.OnPlatform (20, 0, 0), 0, 0);

			SearchContent = GetSearchContent ();
			Content = SearchContent;
			this.Appearing += DoSearchForPlace;
		}
	}
}


