using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using RestSharp;
using System.Linq;
using System.Diagnostics;
using Xamarin;
using Xamarin.Forms.Maps;
using System.Threading;
using System.Text;

namespace RayvMobileApp
{

	enum FilterKind: short
	{
		Mine,
		All,
		Wishlist,
		New,
	}

	public class ListPage : ContentPage
	{
		#region Fields

		static PlacesTableView listView;
		//		StackLayout FilterCuisinePicker;
		StackLayout MainContent;
		Label SplashImage;

		EntryWithButton FilterSearchBox;
		//		Entry AreaBox;
		ActivityIndicator Spinner;
		//		string ALL_TYPES_OF_FOOD = "All Types of Food";

		Label NothingFound;
		bool IsFiltered;
		//		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();
		public bool NeedsReload = true;
		string _FilterSearchText;
		RayvButton addNewButton;


		public static List<Place> ItemsSource {
			set {
				lock (Persist.Instance.Lock) {
					var nearby = value.Where (p => p.distance_for_search < settings.MAX_LIST_DISTANCE).ToList ();
					listView.SetMainList (nearby);
				}
			}
		}

		#endregion

		#region Properties

		public MealKind FilterPlaceKind { get; set; }

		public PlaceStyle FilterPlaceStyle { get; set; }

		Position? _filterSearchCenter;

		public Position? FilterSearchCenter { 
			get { return _filterSearchCenter; } 
			set {
				_filterSearchCenter = value;
				listView.SearchCentre = value;
			}
		}

		Position _displayPosition;

		public Position DisplayPosition { 
			get { return _displayPosition; } 
			set{ _displayPosition = value; } 
		}

		public string FilterCuisine { get; set; }

		public string FilterShowWho { get; set; }

		public string FilterByPlaceName { 
			get {
				return _FilterSearchText;
			} 
			set {
				_FilterSearchText = value;
			} 
		}

		public VoteFilterKind FilterVoteKind { get; set; }

		#endregion


		void LoadFilterValues ()
		{
			if (!FilterSearchCenter.Equals (null))
				listView.IsShowingDistance = false;
			if (!string.IsNullOrEmpty (FilterByPlaceName)) {
				FilterSearchBox.Text = FilterByPlaceName;
			}
		}

		#region timer

		// splash creen timer
		private System.Timers.Timer _splashTimer;

		public void  StopSplashTimer (object sender, EventArgs e)
		{
			Console.WriteLine ("Disabling ListPage Splash Timer due to location update");
			_splashTimer.Close ();
			SplashImage.IsVisible = false;
			App.locationMgr.RemoveLocationUpdateHandler (StopSplashTimer);
		}

		void StartSplashTimer ()
		{
			Console.WriteLine ("StartSplashTimer START");
			if (_splashTimer != null)
				_splashTimer.Close ();
			_splashTimer = new System.Timers.Timer ();
			//Trigger event every 5 second
			_splashTimer.Interval = 5000;
			_splashTimer.Elapsed += OnSplashTimerTrigger;
			_splashTimer.Enabled = true;
			App.locationMgr.AddLocationUpdateHandler (StopSplashTimer);
			Thread.Sleep (500);
			if (_splashTimer.Enabled)
				SplashImage.IsVisible = true;
		}

		private void OnSplashTimerTrigger (object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine ("OnSplashTimerTrigger");
			Device.BeginInvokeOnMainThread (() => {
				_splashTimer.Close ();
				try {
					SplashImage.IsVisible = false;
					Refresh ();
				} catch (Exception ex) {
					Console.WriteLine ("OnSplashTimerTrigger Exception {0}", ex);
				} 
			});
		}

		// gps timer
		private System.Timers.Timer _timer;

		void StartTimerIfNoGPS ()
		{
			if (Persist.Instance.Online && Persist.Instance.GpsPosition.Latitude != 0.0)
				return;
			Console.WriteLine ("StartTimerIfNoGPS START");
			if (_timer != null)
				_timer.Close ();
			_timer = new System.Timers.Timer ();
			//Trigger event every 5 second
			_timer.Interval = 5000;
			_timer.Elapsed += OnTimerTrigger;
			_timer.Enabled = true;
		}

		private void OnTimerTrigger (object sender, System.Timers.ElapsedEventArgs e)
		{
			try {
				if (!Persist.Instance.Online) {
					// not ready yet
					//Debug.WriteLine ("OnTimerTrigger - not live");
					return;
				}
				Console.WriteLine ("StartTimerIfNoGPS OnTimerTrigger ONLINE");
				lock (Persist.Instance.Lock) {
					try {
						Device.BeginInvokeOnMainThread (() => SetList (Persist.Instance.Places));
					} catch (Exception ex) {
						Insights.Report (ex);
						restConnection.LogErrorToServer ("ListPage.OnTimerTrigger Exception {0}", ex);
					}
				}
				_timer.Close ();
			} catch (UnauthorizedAccessException) {
				// login failed - stop
				_timer.Close ();
			}
		}

		#endregion



		#region Events

		void DoSelectListItem (object sender, EventArgs e)
		{
			try {
				Console.WriteLine ("Listpage.DoSelectListItem");
				var place = (sender as PlaceCell).Place;
				if (place.IsDraft) {
					NeedsReload = true;
					var editPage = new PlaceEditor (place);
					editPage.Cancelled += (s, ev) => Navigation.PopAsync ();
					editPage.Saved += (s, ev) => {
						Navigation.PopAsync ();
						Refresh ();
					};
					Navigation.PushAsync (editPage);
				} else {
					var detailPage = new DetailPage (place);
					detailPage.Closed += (s, ev) => {
						if ((s as DetailPage).Dirty) {
							Refresh ();
						}
					};
					NeedsReload = false;
					Navigation.PushAsync (detailPage);
				}
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		void Refresh ()
		{
			Console.WriteLine ("ListPage.Refresh");
			DoFilterList ();
			StartTimerIfNoGPS ();
		}

		public void DoServerRefresh (object s, EventArgs e)
		{
//			try {
//				Persist.Instance.GetUserData (
//					onFail: () => {
//						Spinner.IsVisible = false;
//						DisplayAlert ("Offline", "Unable to contact server - try later", "OK");
//					},
//					onSucceed: () => {
//						Refresh ();
//						listView.DisplayedList.EndRefresh ();
//					},
//					onFailVersion: () => {
//						var login = new LoginPage ();
//						Navigation.PushModalAsync (login);
//					},
//					since: DateTime.UtcNow, 
//					incremental: true);
//			} catch (ProtocolViolationException) {
//				DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
//			}
		}






		void ClearFilter (object s, EventArgs e)
		{ 
			Console.WriteLine ("Listpage.ClearFilter");
			FilterSearchBox.Text = "";
			FilterCuisine = "";
			FilterPlaceKind = MealKind.None;
			FilterPlaceStyle = PlaceStyle.None;
			FilterSearchCenter = null;
			DisplayPosition = Persist.Instance.GpsPosition;
			IsFiltered = false;
			DoFilterList ();
		}


		void DoTextSearch (object sender, EventArgs e)
		{
			FilterSearchBox.TextEntry.Unfocus ();
			Console.WriteLine ("Listpage.DoTextSearch");
			DoFilterList ();
		}

		void UpdateCuisine (Object sender, ItemTappedEventArgs e)
		{
			Debug.WriteLine ("Listpage.UpdateCuisine");
			string cuisine = ((KeyValuePair<string,int>)e.Item).Key;
			FilterCuisine = cuisine;
			Spinner.IsVisible = true;
			Spinner.IsRunning = true;
			Content = MainContent;
			DoFilterList ();
		}

		#endregion

		#region Methods

		void ShowMap ()
		{
			Debug.WriteLine ("ListPage ShowMap: Push GOOGLE MapPage");
			// keep last posn so we can tell if it changed
//			if (settings.USE_XAMARIN_MAPS) {
			Persist.Instance.DisplayPosition = DisplayPosition;
			MapPage map = new MapPage ();
			map.Changed += (sender, e) => {
				NeedsReload = true;
				FilterSearchCenter = Persist.Instance.DisplayPosition;
				DisplayPosition = Persist.Instance.DisplayPosition;
			};
			Navigation.PushAsync (map);
		}


		void ResetCuisinePicker ()
		{
			Debug.WriteLine ("Listpage.ResetCuisinePicker");
		}

		static void DebugList (string step, IEnumerable<Vote> filteredList, Dictionary<string,Vote> myVotes)
		{
			//debug
			if (false)
				foreach (var v in filteredList.ToList ()) {
					bool res = (myVotes.ContainsKey (v.key));
//						?
//					            myVotes [v.key].style == FilterPlaceStyle :
//					            v.style == FilterPlaceStyle);
					Console.WriteLine ($"[{step}]{v.key} {v.place_name} by {v.VoterName} style {res}");
				}
		}

		public static List<Place> FilterPlaceList (
			List<Place> list, 
			out string description,
			out bool isFiltered,
			FilterParameters filter,
			ref Position displayPosition
		)
		{
			isFiltered = false;
			listView.Filter = filter;
			description = "";
			Console.WriteLine ("Listpage.FilterPlaceList");
			List<Place> results = new List<Place> ();
			List<string> styleDescriptionItems = new List<string> ();
			var filteredList = from v in Persist.Instance.Votes
			                   join p in list on v.key equals p.key
			                   select v;
//			IEnumerable<Vote> filteredList = list.Select (p=
			// dict mapping vote place key to vote
			Dictionary<string,Vote> myVotes = Persist.Instance.Votes
					.Where (v => v.voter == Persist.Instance.MyId.ToString ())
					.ToDictionary (v => v.key, v => v);
			String text = filter.Text?.ToLower ();

			// VOTE FILTERS
			// - Cuisine
			// - Mine
			var myKey = Persist.Instance.MyId.ToString ();
			Persist.Instance.FilterWhoKey = "";
			if (filter.Who == myKey) {
				filteredList = filteredList.Where (v => v.voter == myKey);
				isFiltered = true;
				styleDescriptionItems.Add ("My votes only");
				Console.WriteLine ("FilterPlaceList filter text");

			} else {
				// - chosen friends
				if (!string.IsNullOrEmpty (filter.Who)) {
					filteredList = filteredList.Where (v => v.voter == filter.Who && v.vote > 0);
					// as I am not the chosen friend, clear my vote dict so my votes don't override theirs
					myVotes.Clear ();
					isFiltered = true;
					Friend voter;
					if (Persist.Instance.Friends.TryGetValue (filter.Who, out voter)) {
						styleDescriptionItems.Add ($"{voter.Name}'s Places");
						Persist.Instance.FilterWhoKey = filter.Who;
						Console.WriteLine ("ListPage filter chosen friends (no override for my votes)");
					} else
						Console.WriteLine ("List Page - Friend not found (Bad Key)");
				}
			}
			if (!string.IsNullOrEmpty (filter.Cuisine)) {
				filteredList = filteredList.Where (v => 
					                                   myVotes.ContainsKey (v.key) ?
					                                   myVotes [v.key].cuisineName == filter.Cuisine :
					                                   v.cuisineName == filter.Cuisine);
				isFiltered = true;
				styleDescriptionItems.Add ($"Cuisine is {filter.Cuisine}");
				Console.WriteLine ("ListPage filter cuisine");
			}

			// - vote value
			switch (filter.Kind) {
				case VoteFilterKind.Stars:
					{
						filteredList = filteredList.Where (v =>
							                                   myVotes.ContainsKey (v.key) ?
							                                   myVotes [v.key].vote >= FindChoicePage.FilterMimimunStarValue :
							                                   v.vote >= FindChoicePage.FilterMimimunStarValue);
						isFiltered = true;
						styleDescriptionItems.Add ("liked places");
						DebugList ("Vote Like", filteredList, myVotes);

						break;
					}
				case VoteFilterKind.Try:
					{
						filteredList = filteredList.Where (v =>
							                                   myVotes.ContainsKey (v.key) ?
							                                   myVotes [v.key].untried || myVotes [v.key].vote > 3 :
							                                   v.untried || v.vote > 3);
						//							filteredList = filteredList.Where (v => v.untried || v.vote >3);
						isFiltered = true;
						styleDescriptionItems.Add ("places to try");
						DebugList ("Vote Try", filteredList, myVotes);
						break;
					}
				case VoteFilterKind.Wish:
					{
						filteredList = filteredList.Where (v =>
							                                   myVotes.ContainsKey (v.key) ?
							                                   myVotes [v.key].untried :
							                                   v.untried);
						isFiltered = true;
						styleDescriptionItems.Add ("wishlist places");
						DebugList ("Vote Wish", filteredList, myVotes);
						break;
					}
				default:
					break;
			}
			// - meal kind
			if (filter.MealKind != MealKind.None && (int)filter.MealKind != Vote.MAX_MEALKIND) {
				filteredList = filteredList.Where (v => 
					                                   myVotes.ContainsKey (v.key) ?
					                                   (myVotes [v.key].kind & filter.MealKind) != MealKind.None :
					                                   (v.kind & filter.MealKind) != MealKind.None);
				isFiltered = true;
				styleDescriptionItems.Add ($"{filter.MealKind}");
				DebugList ("Kind", filteredList, myVotes);
				Console.WriteLine ("ListPage filter Kind");
			}
			// - place style
			if (filter.Style != PlaceStyle.None) {
				DebugList ("pre", filteredList, myVotes);
				filteredList = filteredList.Where (v => 
					                                   (myVotes.ContainsKey (v.key) ?
					                                    myVotes [v.key].style == filter.Style :
					                                    v.style == filter.Style));
				DebugList ("post", filteredList, myVotes);
				isFiltered = true;
				styleDescriptionItems.Add ($"{filter.Style}");
				DebugList ("Style", filteredList, myVotes);
				Console.WriteLine ("ListPage filter style");
			}

			// turn vote list into place list
			IEnumerable<Place> placeList = filteredList.Select (v => Persist.Instance.GetPlace (v.key)).Distinct ();

			// PLACE FILTERS
			if (!string.IsNullOrEmpty (text)) {
				Console.WriteLine ($"ListPage filter text BEFORE {placeList.ToList ().Count}");
				placeList = placeList.Where (p => p.place_name.ToLower ().Contains (text));
				isFiltered = true;
				styleDescriptionItems.Add ($"Name is '{text}'");
				Console.WriteLine ($"ListPage filter text {placeList.ToList ().Count}");
			}

			description = string.Join (", ", styleDescriptionItems);
			if (filter.Centre != null) {
				//					var delta = settings.GEO_FILTER_BOX_SIZE_DEG;
				displayPosition = (Position)filter.Centre;
				List<Place> distance_list = placeList.Where (p => p != null).ToList ();
				foreach (var p in distance_list) {
					if (p == null)
						Console.WriteLine ("p = null");

					p.distance_for_search = p.distance_from (displayPosition);
					if (p.distance_for_search < 0.1)
						Console.WriteLine ($"{p.place_name} is {p.distance_for_search}");
				}
				distance_list.Sort ((a, b) => a.distance_for_search.CompareTo (b.distance_for_search));
				Console.WriteLine ("ListPage filter location");
				var savedLocation = Persist.Instance.GetConfig (settings.FILTER_WHERE_NAME);
				if (string.IsNullOrEmpty (savedLocation))
					styleDescriptionItems.Add ("Near location specified");
				else
					styleDescriptionItems.Add ($"Near {savedLocation}");
				isFiltered = true;
				return distance_list;
			} else {
				//reset the list distance in case it was modified by a previous, cleared, geo search #757
				foreach (var p in placeList) {
					p.CalculateDistanceFromPlace (displayPosition);
					p.distance_for_search = 0;
				}
				List<Place> result_list = placeList.ToList ();
				result_list.Sort ();
				return result_list;
			}
		}

		void DoFilterList ()
		{
			Console.WriteLine ("Listpage.DoFilterList");
			LoadFilterValues ();
			string styleDescription = "";
			try {
				Persist.Instance.DisplayList = Persist.Instance.GetData ();
				IEnumerable<Vote> filteredList = Persist.Instance.Votes;
				// dict mapping vote place key to vote
				Dictionary<string,Vote> myVotes = Persist.Instance.Votes
					.Where (v => v.voter == Persist.Instance.MyId.ToString ())
					.ToDictionary (v => v.key, v => v);
				String text = _FilterSearchText?.ToLower ();

				var filterParams = new FilterParameters () {
					Text = _FilterSearchText,
					Who = FilterShowWho,
					Cuisine = FilterCuisine,
					Kind = FilterVoteKind,
					MealKind = FilterPlaceKind,
					Style = FilterPlaceStyle,
					Centre = FilterSearchCenter
				};

				Persist.Instance.DisplayList = FilterPlaceList (
					Persist.Instance.DisplayList, 
					out styleDescription,
					out IsFiltered,
					filterParams,
					ref _displayPosition
				);
				addNewButton.IsVisible = string.IsNullOrEmpty (text) ? false : text.Length > 0;


			} catch (Exception ex) {
				Insights.Report (ex);
				Console.WriteLine ($"DoFilterList ERROR {ex}");
				restConnection.LogErrorToServer ("DoFilterList: Exception {0}", ex);
			}
			SetList (Persist.Instance.DisplayList);
			FilterSearchBox.Unfocus ();
			listView.SummaryText = styleDescription;
		}

	

		void InnerSetList (List<Place> list)
		{
			lock (Persist.Instance.Lock) {
				try {
					List<Place> nearestList = new List<Place> ();
					Console.WriteLine ("SetList {0}", list.Count);
					if (list.Count == 0) {
						listView.IsVisible = false;
						NothingFound.IsVisible = true;
						Spinner.IsVisible = false;
						Spinner.IsRunning = false;
						return;
					}
					NothingFound.WidthRequest = this.Width;
					NothingFound.IsVisible = false;
					listView.IsVisible = true;
					ItemsSource = list;
					Spinner.IsVisible = false;
					Spinner.IsRunning = false;
				} catch (Exception ex) {
					Insights.Report (ex);
					Console.WriteLine ($"InnerSetList ERROR {ex}");
					restConnection.LogErrorToServer ("ListPage.SetList Exception {0}", ex);
				}
				//				});
			}
		}



		public void SetList (List<Place> list)
		{
			Debug.WriteLine ("Listpage.SetList");
			if (Persist.Instance.Places.Count () == 0) {
				try {
					Persist.Instance.GetUserData (
						onFail: () => {
							DisplayAlert ("Offline", "Unable to contact server - try later", "OK");
						}, 
						onSucceed: () => {
							InnerSetList (Persist.Instance.Places);
						},
						onFailVersion: () => {
							var login = new LoginPage ();
							Navigation.PushModalAsync (login);
						}, 
						incremental: false);
				} catch (ProtocolViolationException) {
					DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
				}
			} else
				InnerSetList (list);
		}

		#endregion

		#region Constructors

		public ListPage ()
		{
			Analytics.TrackPage ("ListPage");
			Console.WriteLine ("ListView()");
			this.Icon = settings.DevicifyFilename ("bars-black.png");
			FilterPlaceKind = MealKind.None;
			FilterPlaceStyle = PlaceStyle.None;
			FilterVoteKind = VoteFilterKind.All;
			FilterCuisine = "";
			FilterShowWho = "";
			SplashImage = new Label { 
				Text = "Checking Location",
				BackgroundColor = settings.BaseColor,
				TextColor = Color.White,
				XAlign = TextAlignment.Center,
				YAlign = TextAlignment.Center,
				FontSize = settings.FontSizeLabelLarge,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsVisible = false,
			};
			IsFiltered = false;

			listView = new PlacesTableView (showDistance: FilterSearchCenter.Equals (null));
			listView.OnPlaceTapped = DoSelectListItem;
			listView.OnErrorMessage = (s, e) => DisplayAlert ("Places", e.Message, "OK");
//			listView.DisplayedList.Refreshing += DoServerRefresh;
//			listView.DisplayedList.IsPullToRefreshEnabled = true;
			listView.SearchCentre = FilterSearchCenter;
			StackLayout tools = new BottomToolbar (this, "list");
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			Grid grid = new Grid {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.StartAndExpand,
				ColumnSpacing = 0,
				RowSpacing = 0,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (35, Device.OnPlatform (GridUnitType.Absolute, GridUnitType.Auto, GridUnitType.Auto)) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};

			StackLayout inner = new StackLayout {
				Children = {
					listView,
					NothingFound,
				}
			};
			Spinner = new ActivityIndicator {
				IsVisible = true,
				IsRunning = true,
				Color = Color.Red,
			};
			BoxView bg0 = new BoxView { 
				BackgroundColor = settings.BaseColor,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};
			FilterSearchBox = new EntryWithButton {
				Placeholder = "Search for place",
				Source = settings.DevicifyFilename ("TB active search.png"),
				OnClick = DoTextSearch,
				Text = "",
			};
			FilterSearchBox.TextEntry.BackgroundColor = settings.ColorLightGray;

			FilterSearchBox.TextEntry.Completed += (sender, e) => {
				FilterSearchBox.TextEntry.Unfocus ();
			};
			FilterSearchBox.OnClick = (s, e) => {
				_FilterSearchText = FilterSearchBox.Text;
				DoTextSearch (s, e);
			};


			grid.Children.Add (bg0, 0, 0);
			grid.Children.Add (FilterSearchBox, 0, 0);
			grid.Children.Add (Spinner, 0, 1);
			grid.Children.Add (inner, 0, 2);
			grid.Children.Add (SplashImage, 0, 1, 0, 3);
			addNewButton = new RayvButton ("Add New Place");
			addNewButton.OnClick = (sender, e) => {
				Navigation.PushAsync (new AddPage1 (false){ SearchText = FilterSearchBox.Text });
			};

			MainContent = new StackLayout {
				Children = {
					grid,
					addNewButton,
					tools
				},
				Padding = 0,
			};
			Content = MainContent;

			ToolbarItems.Add (new ToolbarItem {
				Text = "  Map  ",
				//				Icon = "icon-map.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (ShowMap),
			});

			NeedsReload = true;
			DisplayPosition = Persist.Instance.GpsPosition;
			Console.WriteLine ($"ListPage ctor. set posn to {DisplayPosition.Latitude},{DisplayPosition.Longitude}");

			this.Disappearing += (sender, e) => {
				if (_timer != null)
					_timer.Close ();
			};
			this.Appearing += (sender, e) => {
				try {
					FilterSearchBox.TextEntry.TextChanged += (s, ev) => {
						_FilterSearchText = FilterSearchBox.Text;
						DoTextSearch (sender, e);
						FilterSearchBox.TextEntry.Focus ();
						Console.WriteLine ("FilterSearchBox TextChanged");
					};
					Console.WriteLine ("FilterSearchBox Unfocus");
					FilterSearchBox.TextEntry.Unfocus ();
					App.locationMgr.StartLocationUpdates ();
					DateTime? last_access = Persist.Instance.GetConfigDateTime (settings.LAST_SYNC);
					if (last_access != null && last_access + settings.LIST_PAGE_TIMEOUT < DateTime.UtcNow) {
						StartSplashTimer ();

					}
					Persist.Instance.SetConfig (settings.LAST_OPENED, DateTime.UtcNow);
					if (NeedsReload) {
						Refresh ();
						Analytics.TrackPage ("ListPage Refreshed");
						return;
					}
					Double deviation = Place.approx_distance (Persist.Instance.GpsPosition, DisplayPosition);
					if (deviation > 0.05) {
						Analytics.TrackPage ("ListPage Moved");
						Console.WriteLine ($"ListPage Moved {deviation}");
						DisplayPosition = Persist.Instance.GpsPosition;
						Refresh ();
					}
				} catch (Exception ex) {
					Insights.Report (ex);
				}
			};
			App.Resumed += delegate {
				DateTime? last_access = Persist.Instance.GetConfigDateTime (settings.LAST_OPENED);
				if (last_access != null && last_access + settings.LIST_PAGE_TIMEOUT < DateTime.UtcNow) {
					StartSplashTimer ();

				}
			};
		}

		/**
		 * Constructor when a cuisine is supplied
		 */
		// Todo: this should be setting a property, not a ctor
		public ListPage (string cuisine) : this ()
		{
			FilterCuisine = cuisine;
		}

		#endregion
	}

}


