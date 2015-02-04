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
					point.distance_from_place ();
				}
				points.Sort ();
				Spinner.IsRunning = false;
				Console.WriteLine ("DoSearch: Activity Over - push AddResultsPage");
				this.Navigation.PushAsync (new AddResultsPage ());
				AddResultsPage.ItemsSource = points;
			} catch (Exception e) {
				Console.WriteLine ("DoSearch: Excaption {0}", e);
				DisplayAlert ("Oops", "Unable to search as an error occurred", "Close");
			}
		}


		#endregion

		#region Events

		async void SearchHere (object sender, EventArgs e)
		{
			// geocode
			Xamarin.FormsMaps.Init ();
			var positions = (await (new Geocoder ()).GetPositionsForAddressAsync (PlaceHistoryBox.Text)).ToList ();
			Console.WriteLine ("SearchHere: Got");
			Persist.Instance.AddSearchHistoryItem (PlaceHistoryBox.Text);
			searchPosition = positions.First ();
			DoSearch ();
		}

		async void SearchSomewhere (object sender, EventArgs e)
		{
			// geocode
			Xamarin.FormsMaps.Init ();
			var positions = (await (new Geocoder ()).GetPositionsForAddressAsync ((sender as Button).Text)).ToList ();
			Console.WriteLine ("SearchSomewhere: Got");
			searchPosition = positions.First ();
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
			var HistoryList = Persist.Instance.SearchHistory;
			if (HistoryList.Count > 0 && HistoryList [0] != null && HistoryList [0].PlaceName.Length > 0) {
				PlaceHistoryBtn.Text = Persist.Instance.SearchHistory [0].PlaceName;
				PlaceHistoryBtn.Clicked -= ShowPlaceHistory;
				PlaceHistoryBtn.Clicked += SearchSomewhere;
			} else {
				PlaceHistoryBtn.Text = " Choose Where... ";
				PlaceHistoryBtn.Clicked -= SearchSomewhere;
				PlaceHistoryBtn.Clicked += ShowPlaceHistory;
			}
			PlaceHistoryBox = new Entry {
				Placeholder = "Where to search near...",
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

			StackLayout history = new StackLayout ();
			history.Children.Add (PlaceHistoryBox);
			if (Persist.Instance.SearchHistory.Count == 0) {
				history.Children.Add (new LabelWide {
					Text = NO_SEARCH_HISTORY,
				});
			} else {
				foreach (SearchHistory item in Persist.Instance.SearchHistory) {
					if (item.PlaceName != null && item.PlaceName.Length > 0) {
						Button clickItem = new Button {
							Text = item.PlaceName,
							HorizontalOptions = LayoutOptions.Center,

						};
						clickItem.Clicked += SearchSomewhere;
						history.Children.Add (clickItem);
					}
				}
			}
			history.Children.Add (HereBtn);
			PlaceHistoryFrame = new Frame {
				OutlineColor = Color.Silver,
				IsVisible = false,
				Content = history,
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
				if (!FirstTime)
					this.Navigation.PushAsync (new ListPage ());
				else
					FirstTime = false;
			};
		}

		#endregion


	}
}

