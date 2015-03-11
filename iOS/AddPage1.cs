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

namespace RayvMobileApp.iOS
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
		List<GeoLocation> LocationList;
		bool DEBUG_ON_SIMULATOR = (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR);

		#region Events

		void DoChangeLocation (object s, EventArgs e)
		{
			LocationSearchedBox.IsVisible = false;
			ResetLocationBtn.IsVisible = false;
			LocationEditBox.IsVisible = true;
		}

		void DoSearchForPlace (object s, EventArgs e)
		{
			DoSearch (PlaceNameBox.Text, "");
		}

		void DoFindLocation (object sender, EventArgs e)
		{
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
					LocationResultsView.ItemsSource = LocationList;
					LocationResultsView.IsVisible = true;
					ResetLocationBtn.IsVisible = true;
					LocationSearchedBox.IsVisible = false;
					LocationEditBox.IsVisible = true;
					NothingFound.IsVisible = false;
					PlacesListView.IsVisible = false;
				}
			} catch (Exception ex) {
				Insights.Report (ex);
			}

//			Xamarin.FormsMaps.Init ();
//			var positions = (await (new Geocoder ()).GetPositionsForAddressAsync ((sender as Button).Text)).ToList ();
//			Console.WriteLine ("AddMenu.SearchSomewhere: Got");
//			if (positions.Count > 0) {
//				SearchPosition = positions.First ();
//			} else if (DEBUG_ON_SIMULATOR) {
//				SearchPosition = new Position (53.1, -1.5);
//				Console.WriteLine ("AddMenu.SearchSomewhere DEBUG_ON_SIMULATOR");
//			}
//			DoSearch (PlaceNameBox.Text, (sender as Button).Text);
			//SetHistoryButton ();
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
				this.Navigation.PushAsync (new EditPage (p, addingNewPlace: true));
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		void DoResetLocation (object s, EventArgs e)
		{
			SearchPosition = Persist.Instance.GpsPosition;
			ResetLocationBtn.IsVisible = false;
			LocationSearchedBox.IsVisible = false;
			LocationEditBox.IsVisible = true;
			LocationResultsView.IsVisible = false;
			DoSearch (PlaceNameBox.Text, "");
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
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Console.WriteLine ("AddPage1.DoSearch: Thread");
				Dictionary<string, string> parameters = new Dictionary<string, string> ();
				parameters ["lat"] = SearchPosition.Latitude.ToString ();
				parameters ["lng"] = SearchPosition.Longitude.ToString ();
				if (PlaceNameBox.Text != null) {
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
						point.CalculateDistanceFromPlace ();
					}
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
					});
				} catch (Exception e) {
					Insights.Report (e);
					restConnection.LogErrorToServer ("AddPage1.DoSearch: Exception {0}", e);
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("AddMenu.DoSearch: MainThread Exception");
						Spinner.IsRunning = false;
						DisplayAlert ("Oops", "Unable to search. Network problems?", "Close");
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
			Title = "Add a Place";
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				IsVisible = false,
			};

			Spinner = new ActivityIndicator {
				IsRunning = false,
			};

			ResetLocationBtn = new Button {
				Text = "  return to current location",
				HorizontalOptions = LayoutOptions.Start,
				IsVisible = false,
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
			LocationSearchedBox = new LabelWithChangeButton {
				Text = "searching Current Location",
				OnClick = DoChangeLocation,
			};
			LocationEditBox = new EntryWithChangeButton {
				PlaceHolder = "Where? e.g. Town or Address",
				OnClick = DoFindLocation,
				ButtonText = "Search",
				IsVisible = false,
			};
			SearchPosition = Persist.Instance.GpsPosition;
			StackLayout menu = new StackLayout { 
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Spacing = 10,
				Padding = 4,
				Children = {
					PlaceNameBox,
					LocationSearchedBox,
					LocationEditBox,
					ResetLocationBtn,
					Spinner,
					LocationResultsView,
					PlacesListView,
					NothingFound,
				}
			};
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					menu,
					tools
				}
			};
		}
	}
}

