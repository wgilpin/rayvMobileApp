using System;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Linq;
using Xamarin.Forms.Maps;
using System.Diagnostics;
using Xamarin;

namespace RayvMobileApp.iOS
{
	public class AddMenu : ContentPage
	{
		const String SEARCH_NEAR = "Add Near Me";
		const String NO_SEARCH_HISTORY = "No History";
		const String SEARCH_MAP = "Add From Map";

		#region Fields

		Entry SearchBox;
		RayvButton NearMeBtn;
		RayvButton FromMapBtn;
		RayvButton PlaceHistoryBtn;
		Entry PlaceHistoryBox;
		Boolean _dirty = false;
		Position _searchPosition;
		Frame PlaceHistoryFrame;
		Grid PlaceHistoryCombo;
		RayvButton HereBtn;
		ActivityIndicator Spinner;
		bool FirstTime;
		StackLayout historyBox;
		bool DEBUG_ON_SIMULATOR = (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR);

		#endregion

		#region Methods

		void SetHistoryButton ()
		{
			var HistoryList = Persist.Instance.SearchHistoryList;
			while (HistoryList.Count > 0 && HistoryList [0] != null && HistoryList [0].PlaceName == null)
				HistoryList.RemoveAt (0);

			if (HistoryList.Count > 0 &&
			    HistoryList [0] != null &&
			    HistoryList [0].PlaceName != null &&
			    HistoryList [0].PlaceName.Length > 0) {

				PlaceHistoryBtn.Text = Persist.Instance.SearchHistoryList [0].PlaceName;
				PlaceHistoryBtn.Clicked -= ShowPlaceHistory;
				PlaceHistoryBtn.Clicked -= SearchSomewhere;
				PlaceHistoryBtn.Clicked += SearchSomewhere;
			} else {
				PlaceHistoryBtn.Text = " Choose Where... ";
				PlaceHistoryBtn.Clicked -= SearchSomewhere;
				PlaceHistoryBtn.Clicked -= ShowPlaceHistory;
				PlaceHistoryBtn.Clicked += ShowPlaceHistory;
			}
		}

		public Position searchPosition {
			get { return _searchPosition; }
			set {
				_dirty = true;
				_searchPosition = value;
			}
		}

		void DoSearch (String searchPlace, bool addToHistory = false)
		{
			if (!_dirty) {
				Console.WriteLine ("AddMenu.DoSearch - Not Dirty");
				return;
			}
			Console.WriteLine ("AddMenu.DoSearch: Activity");
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				_dirty = false;
				Console.WriteLine ("AddMenu.DoSearch: Thread");
				_dirty = false;
				Dictionary<string, string> parameters = new Dictionary<string, string> ();
				parameters ["lat"] = searchPosition.Latitude.ToString ();
				parameters ["lng"] = searchPosition.Longitude.ToString ();
				if (SearchBox.Text != null) {
					parameters ["addr"] = searchPlace;
				}
				if (SearchBox.Text != null) {
					parameters ["place_name"] = searchPlace;
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
					if (addToHistory)
						Persist.Instance.AddSearchHistoryItem (searchPlace);
					Device.BeginInvokeOnMainThread (() => {
						Spinner.IsRunning = false;
						Console.WriteLine ("AddMenu.DoSearch: Activity Over. push AddResultsPage");
						this.Navigation.PushAsync (new AddResultsPage (points));
					});
				} catch (Exception e) {
					Insights.Report (e);
					restConnection.LogErrorToServer ("AddMenu.DoSearch: Exception {0}", e);
					Device.BeginInvokeOnMainThread (() => {
						Spinner.IsRunning = false;
						DisplayAlert ("Oops", "Unable to search. Network problems?", "Close");
					});
				}
			})).Start ();
		}

		void SetupSearchHistory ()
		{
			historyBox.Children.Clear ();
			Persist.Instance.LoadSearchHistoryFromDb ();
			if (Persist.Instance.SearchHistoryList.Count == 0) {
				historyBox.Children.Add (new LabelWide {
					Text = NO_SEARCH_HISTORY,
				});
			} else {
				foreach (SearchHistory item in Persist.Instance.SearchHistoryList) {
					if (item.PlaceName != null && item.PlaceName.Length > 0) {
						Button clickItem = new Button {
							Text = item.PlaceName,
							HorizontalOptions = LayoutOptions.Center,
						};
						clickItem.Clicked += SearchSomewhere;
						historyBox.Children.Add (clickItem);
					}
				}
			}
			historyBox.Children.Add (PlaceHistoryBox);
			historyBox.Children.Add (HereBtn);
		}


		#endregion

		#region Events

		async void SearchHere (object sender, EventArgs e)
		{
			// geocode
			try {
				Xamarin.FormsMaps.Init ();
				var geoCodePositions = (await (new Geocoder ()).GetPositionsForAddressAsync (PlaceHistoryBox.Text));
				var positions = geoCodePositions.ToList ();
				if (DEBUG_ON_SIMULATOR || positions.Count > 0) {
					Console.WriteLine ("AddMenu.SearchHere: Got");
					Persist.Instance.AddSearchHistoryItem (PlaceHistoryBox.Text);
					//TODO: remove DEBUG_LOGIC
					if (DEBUG_ON_SIMULATOR) {
						searchPosition = new Position (53.1, -1.5);
						Console.WriteLine ("AddMenu.SearchHere: DEBUG_ON_SIMULATOR");
					} else {
						searchPosition = positions.First ();
					}
					DoSearch (SearchBox.Text, true);
					SetupSearchHistory ();
				} else {
					await DisplayAlert ("Not Found", "Couldn't find that place", "OK");
				}
			} catch (Exception E) {
				Insights.Report (E);
				restConnection.LogErrorToServer ("AddMenu.SearchHere: Exception {0}", E.Message);
				await DisplayAlert ("Error", "Couldn't find that place", "OK");
			}
		}

		async void SearchSomewhere (object sender, EventArgs e)
		{
			// A History button click event
			// geocode
			Xamarin.FormsMaps.Init ();
			var positions = (await (new Geocoder ()).GetPositionsForAddressAsync ((sender as Button).Text)).ToList ();
			Console.WriteLine ("AddMenu.SearchSomewhere: Got");
			if (positions.Count > 0) {
				searchPosition = positions.First ();
			} else if (DEBUG_ON_SIMULATOR) {
				searchPosition = new Position (53.1, -1.5);
				Console.WriteLine ("AddMenu.SearchSomewhere DEBUG_ON_SIMULATOR");
			}
			DoSearch ((sender as Button).Text, true);
			SetHistoryButton ();
		}

		void ShowPlaceHistory (object sender, EventArgs e)
		{
			PlaceHistoryFrame.IsVisible = true;
			PlaceHistoryCombo.IsVisible = false;
		}

		void SearchNearMe (object sender, EventArgs e)
		{

			searchPosition = Persist.Instance.GpsPosition;
			DoSearch (SearchBox.Text, false);
		}

		async void SearchMap (object sender, EventArgs e)
		{
			Debug.WriteLine ("AddMenu.SearchMap: Push AddMapPage");
			await Navigation.PushAsync (new AddMapPage ());
		}

		void SearchLocationEdited (object sender, EventArgs e)
		{
			HereBtn.IsVisible = PlaceHistoryBox.Text.Length > 0;
		}


		#endregion

		#region Constructors



		public AddMenu ()
		{
			Console.WriteLine ("AddMenu()");
			Persist.Instance.HaveAdded = false;
			FirstTime = true;
			SearchBox = new Entry {
				Placeholder = "Name to find"
			};
			PlaceHistoryBtn = new RayvButton ();
			SetHistoryButton ();
			PlaceHistoryBox = new Entry {
				Placeholder = "Enter a Search Location...",
			};
			PlaceHistoryBox.TextChanged += SearchLocationEdited;
			var PlaceHistoryOpenBtn = new Image {
				Source = ImageSource.FromFile ("216-compose.png"),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			var clickOpen = new TapGestureRecognizer ();
			clickOpen.Tapped += (s, e) => {
				ShowPlaceHistory (null, null);
			};
			PlaceHistoryOpenBtn.GestureRecognizers.Add (clickOpen);

			HereBtn = new RayvButton {
				Text = " Search Here ",
				IsVisible = false,
			};
			HereBtn.Clicked += SearchHere;

			historyBox = new StackLayout ();
			SetupSearchHistory ();

			PlaceHistoryFrame = new Frame {
				OutlineColor = Color.Silver,
				IsVisible = false,
				Content = historyBox,
			};
//			PlaceHistoryBtn.Clicked += ShowPlaceHistory;
			SetHistoryButton ();

			NearMeBtn = new RayvButton {
				Text = SEARCH_NEAR,
			};
			NearMeBtn.Clicked += SearchNearMe;

			FromMapBtn = new RayvButton {
				Text = SEARCH_MAP,
			};
			FromMapBtn.Clicked += SearchMap;

			PlaceHistoryCombo = new Grid {
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			PlaceHistoryCombo.Children.Add (PlaceHistoryBtn, 0, 4, 0, 1);
			PlaceHistoryCombo.Children.Add (PlaceHistoryOpenBtn, 4, 0);

			Spinner = new ActivityIndicator {
				IsRunning = false,
				Color = Color.Blue,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			this.Title = "Add a place";
			var menu = new StackLayout {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Spacing = 10,
				Padding = 4,
				Children = {
					SearchBox,
					NearMeBtn,
					FromMapBtn,
					PlaceHistoryCombo,
					Spinner,
					PlaceHistoryFrame,
				}
			};
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					menu,
					tools
				}
			};
			this.Appearing += (object sender, EventArgs e) => {
				PlaceHistoryBox.Text = "";
				// if we have come from saving a place then we go back to the list
				if (Persist.Instance.HaveAdded)
					this.Navigation.PushAsync (new ListPage ());
				Persist.Instance.HaveAdded = false;
			};
		}

		#endregion


	}
}

