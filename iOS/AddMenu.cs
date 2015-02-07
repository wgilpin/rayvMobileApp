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
		RayvButton HereBtn;
		ActivityIndicator Spinner;
		bool FirstTime;
		StackLayout historyBox;
		bool DEBUG_ON_SIMULATOR = (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR);

		#endregion

		#region logic

		public Position searchPosition {
			get { return _searchPosition; }
			set {
				_dirty = true;
				_searchPosition = value;
			}
		}

		void DoSearch ()
		{
			if (!_dirty) {
				Console.WriteLine ("AddMenu.DoSearch - Not Dirty");
				return;
			}
			Console.WriteLine ("DoSearch: Activity");
			Spinner.IsRunning = true;
			_dirty = false;
			Dictionary<string, string> parameters = new Dictionary<string, string> ();
			parameters ["lat"] = searchPosition.Latitude.ToString ();
			parameters ["lng"] = searchPosition.Longitude.ToString ();
			if (SearchBox.Text != null) {
				parameters ["addr"] = SearchBox.Text;
			}
			if (SearchBox.Text != null) {
				parameters ["place_name"] = SearchBox.Text;
			}
			parameters ["near_me"] = "1";
			try {
				string result = restConnection.Instance.get ("/getAddresses_ajax", parameters).Content;
				JObject obj = JObject.Parse (result);
				//search results hsould show distance from me, not from the search location
//				Position search_center = new Position (
//					                         (double)obj.SelectToken ("search.lat"), 
//					                         (double)obj.SelectToken ("search.lng")
//				                         );
				List<Place> points = JsonConvert.DeserializeObject<List<Place>> (obj.SelectToken ("local.points").ToString ());
				foreach (Place point in points) {
					point.CalculateDistanceFromPlace ();
				}
				points.Sort ();
				Spinner.IsRunning = false;
				Console.WriteLine ("DoSearch: Activity Over - push AddResultsPage");
				this.Navigation.PushAsync (new AddResultsPage ());
				AddResultsPage.ItemsSource = points;
				Persist.Instance.AddSearchHistoryItem (SearchBox.Text);
			} catch (Exception e) {
				Spinner.IsRunning = false;
				restConnection.LogErrorToServer ("DoSearch: Exception {0}", e);
				DisplayAlert ("Oops", "Unable to search. Network problems?", "Close");
			}
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
					DoSearch ();
					SetupSearchHistory ();
				} else {
					await DisplayAlert ("Not Found", "Couldn't find that place", "OK");
				}
			} catch (Exception E) {
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
			Console.WriteLine ("SearchSomewhere: Got");
			if (positions.Count > 0) {
				searchPosition = positions.First ();
			} else if (DEBUG_ON_SIMULATOR) {
				searchPosition = new Position (53.1, -1.5);
				Console.WriteLine ("AddMenu.SearchSomewhere DEBUG_ON_SIMULATOR");
			}
			DoSearch ();
		}

		void ShowPlaceHistory (object sender, EventArgs e)
		{
			PlaceHistoryFrame.IsVisible = true;
		}

		void SearchNearMe (object sender, EventArgs e)
		{

			searchPosition = Persist.Instance.GpsPosition;
			DoSearch ();
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
			FirstTime = true;
			SearchBox = new Entry {
				Placeholder = "Name to find"
			};
			PlaceHistoryBtn = new RayvButton ();
			var HistoryList = Persist.Instance.SearchHistoryList;
			if (HistoryList.Count > 0 && HistoryList [0] != null && HistoryList [0].PlaceName.Length > 0) {
				PlaceHistoryBtn.Text = Persist.Instance.SearchHistoryList [0].PlaceName;
				PlaceHistoryBtn.Clicked -= ShowPlaceHistory;
				PlaceHistoryBtn.Clicked += SearchSomewhere;
			} else {
				PlaceHistoryBtn.Text = " Choose Where... ";
				PlaceHistoryBtn.Clicked -= SearchSomewhere;
				PlaceHistoryBtn.Clicked += ShowPlaceHistory;
			}
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
				PlaceHistoryFrame.IsVisible = true;
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
			PlaceHistoryBtn.Clicked += ShowPlaceHistory;

			NearMeBtn = new RayvButton {
				Text = SEARCH_NEAR,
			};
			NearMeBtn.Clicked += SearchNearMe;

			FromMapBtn = new RayvButton {
				Text = SEARCH_MAP,
			};
			FromMapBtn.Clicked += SearchMap;

			var PlaceHistoryCombo = new Grid {
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
				Children = {
					SearchBox,
					NearMeBtn,
					FromMapBtn,
					PlaceHistoryCombo,
					Spinner,
					PlaceHistoryFrame,
				}
			};
			StackLayout tools = new toolbar (this);
			Content = new StackLayout {
				Children = {
					menu,
					tools
				}
			};
			this.Appearing += (object sender, EventArgs e) => {
				PlaceHistoryBox.Text = "";
//				if (!FirstTime)
//					this.Navigation.PushAsync (new ListPage ());
//				else
//					FirstTime = false;
			};
		}

		#endregion


	}
}

