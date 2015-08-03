using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

		static PlacesListView listView;
		static FilterKind MainFilter = FilterKind.All;
		static String FilterCuisine;
		VoteFilterWho FilterVotesBy;
		VoteFilterWhat FilterVotesKind;
		StackLayout FilterCuisinePicker;
		StackLayout MainContent;
		Label SplashImage;
		Frame filters;
		Label FilterDescr;

		EntryWithButton FilterSearchBox;
		//		Entry AreaBox;
		ActivityIndicator Spinner;
		ToolbarItem FilterTool;
		string ALL_TYPES_OF_FOOD = "All Types of Food";

		Label NothingFound;
		bool IsFiltered;
		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();
		public bool NeedsReload = true;

		Position DisplayPosition;
		MealKind FilterPlaceKind;
		PlaceStyle FilterPlaceStyle;
		Position? FilterSearchCenter;

		public static IEnumerable ItemsSource {
			set {
				lock (Persist.Instance.Lock) {
					listView.ItemsSource = value;
				}
			}
		}

		#endregion

		#region Constructors



		public ListPage ()
		{
			Analytics.TrackPage ("ListPage");
			Console.WriteLine ("ListView()");
			this.Title = "Find Food";
			this.Icon = settings.DevicifyFilename ("bars-black.png");
			FilterPlaceKind = MealKind.None;
			FilterPlaceStyle = PlaceStyle.None;
			SplashImage = new Label { 
				Text = "Checking Location",
				BackgroundColor = settings.BaseColor,
				TextColor = Color.White,
				XAlign = TextAlignment.Center,
				YAlign = TextAlignment.Center,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsVisible = false,
			};
			IsFiltered = false;
			listView = new PlacesListView (showDistance: FilterSearchCenter.Equals (null));
			listView.ItemTapped += DoSelectListItem;
			listView.Refreshing += DoServerRefresh;
			listView.IsPullToRefreshEnabled = true;
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
					new RowDefinition { Height = new GridLength (35, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};

			FilterDescr = new Label { 
				Text = "All Places", 
				HorizontalOptions = LayoutOptions.CenterAndExpand, 
				BackgroundColor = settings.BaseColor, 
				TextColor = Color.White, 
				FontAttributes = FontAttributes.Bold 
			};
			var tapFilterDescr = new TapGestureRecognizer ();
			tapFilterDescr.Tapped += (sender, e) => {
				this.Navigation.PushModalAsync (new RayvNav (new FindChoicePage (this)), false);
			};
			FilterDescr.GestureRecognizers.Add (tapFilterDescr);
			filters = new Frame { 
				Padding = 4,
				HasShadow = false,
				OutlineColor = settings.BaseColor,
				BackgroundColor = settings.BaseColor,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Content = FilterDescr,
			};
			StackLayout inner = new StackLayout {
				Children = {
					filters,
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
			FilterSearchBox.TextEntry.TextChanged += (sender, e) => {
				DoTextSearch (sender, e);
				FilterSearchBox.TextEntry.Focus ();
			};
			FilterSearchBox.TextEntry.Completed += (sender, e) => {
				FilterSearchBox.TextEntry.Unfocus ();
			};


			grid.Children.Add (bg0, 0, 0);
			grid.Children.Add (FilterSearchBox, 0, 0);
			grid.Children.Add (Spinner, 0, 1);
			grid.Children.Add (inner, 0, 2);
			grid.Children.Add (SplashImage, 0, 1, 0, 3);
			filters.IsVisible = false;
			MainContent = new StackLayout {
				Children = {
					grid,
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

			FilterTool = new ToolbarItem {
				Text = "  Filter  ",
				//				Icon = "filter.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Console.WriteLine ("ListPage Toolbar Filter");
					this.Navigation.PushModalAsync (
						new RayvNav (new FindChoicePage (this)));
				})
			};
			ToolbarItems.Add (FilterTool);

			NeedsReload = true;
			DisplayPosition = Persist.Instance.GpsPosition;
			Console.WriteLine ("ListPage.FilterList Constructor set posn to {0},{1}", DisplayPosition.Latitude, DisplayPosition.Longitude);

			this.Disappearing += (sender, e) => {
				if (_timer != null)
					_timer.Close ();
			};
			this.Appearing += (sender, e) => {
				try {
					App.locationMgr.StartLocationUpdates ();
					DateTime? last_access = Persist.Instance.GetConfigDateTime (settings.LAST_SYNC);
					if (last_access != null && last_access + settings.LIST_PAGE_TIMEOUT < DateTime.UtcNow) {
						StartSplashTimer ();

					}
					Persist.Instance.SetConfig (settings.LAST_SYNC, DateTime.UtcNow);
					if (NeedsReload) {
						Refresh ();
						Analytics.TrackPage ("ListPage Refreshed");
						return;
					}
					Double deviation = Place.approx_distance (Persist.Instance.GpsPosition, DisplayPosition);
					if (deviation > 0.05) {
						Analytics.TrackPage ("ListPage Moved");
						DisplayPosition = Persist.Instance.GpsPosition;
						Refresh ();
					}
				} catch (Exception ex) {
					Insights.Report (ex);
				}
			};
			App.Resumed += delegate {
				DateTime? last_access = Persist.Instance.GetConfigDateTime (settings.LAST_SYNC);
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

		/**
		 * Constructor when a kind & style is supplied
		 */
		// Todo: this should be setting a property, not a ctor
		public ListPage (
			MealKind kind, 
			PlaceStyle style, 
			Position? location = null, 
			string cuisine = null, 
			VoteFilterWho byWho = VoteFilterWho.All,
			string filterByPlaceName = "",
			VoteFilterWhat voteKind = VoteFilterWhat.All
		) : this ()
		{
			FilterPlaceKind = kind;
			FilterPlaceStyle = style;
			FilterSearchCenter = location;
			if (!location.Equals (null))
				listView.IsShowingDistance = false;
			FilterCuisine = cuisine;
			FilterVotesKind = voteKind;
			FilterVotesBy = byWho;
			if (!string.IsNullOrEmpty (filterByPlaceName)) {
				FilterSearchBox.Text = filterByPlaceName;
			}
//			FilterList ();
		}

		#endregion

		#region Events

		void DoSelectListItem (object sender, ItemTappedEventArgs e)
		{
			Console.WriteLine ("Listpage.DoSelectListItem");
			var place = e.Item as Place;
			if (place.IsDraft) {
				NeedsReload = true;
				var editPage = new PlaceEditor (place, this);
				editPage.Edit ();
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
		}

		void Refresh ()
		{
			Console.WriteLine ("ListPage.Refresh");
			FilterList ();
			StartTimerIfNoGPS ();
		}

		public void DoServerRefresh (object s, EventArgs e)
		{
			try {
				Persist.Instance.GetUserData (
					onFail: () => {
						DisplayAlert ("Offline", "Unable to contact server - try later", "OK");
					},
					onSucceed: () => {
						Refresh ();
						listView.EndRefresh ();
					},
					onFailVersion: () => {
						Navigation.PushModalAsync (new LoginPage ());
					},
					since: DateTime.UtcNow, 
					incremental: true);
			} catch (ProtocolViolationException ex) {
				DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
			}
		}






		void ClearFilter (object s, EventArgs e)
		{ 
			Console.WriteLine ("Listpage.ClearFilter");
			FilterSearchBox.Text = "";
			FilterCuisine = "";
			FilterPlaceKind = MealKind.None;
			FilterPlaceStyle = PlaceStyle.None;
			FilterSearchCenter = null;
			MainFilter = FilterKind.All;
			DisplayPosition = Persist.Instance.GpsPosition;
			IsFiltered = false;
			FilterList ();
			filters.IsVisible = false;
		}


		void DoTextSearch (object sender, EventArgs e)
		{
			FilterSearchBox.TextEntry.Unfocus ();
			Console.WriteLine ("Listpage.DoTextSearch");
			FilterList ();
		}

		void UpdateCuisine (Object sender, ItemTappedEventArgs e)
		{
			Debug.WriteLine ("Listpage.UpdateCuisine");
			string cuisine = ((KeyValuePair<string,int>)e.Item).Key;
			FilterCuisine = cuisine;
			Spinner.IsVisible = true;
			Spinner.IsRunning = true;
			Content = MainContent;
			FilterList ();
			filters.IsVisible = false;
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

		void DebugList (string step, IEnumerable<Vote> filteredList, Dictionary<string,Vote> myVotes)
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

		async Task FilterList ()
		{
			Console.WriteLine ("Listpage.FilterList");
			List<string> styleDescriptionItems = new List<string> ();
			try {
				Persist.Instance.DisplayList = Persist.Instance.GetData ();
				IEnumerable<Vote> filteredList = Persist.Instance.Votes;
				// dict mapping vote place key to vote
				Dictionary<string,Vote> myVotes = Persist.Instance.Votes
					.Where (v => v.voter == Persist.Instance.MyId.ToString ())
					.ToDictionary (v => v.key, v => v);
				String text = FilterSearchBox.Text.ToLower ();

				// VOTE FILTERS
				// - Cuisine
				// - Mine
				if (FilterVotesBy == VoteFilterWho.Mine) {
					filteredList = filteredList.Where (v => v.voter == Persist.Instance.MyId.ToString ());
					IsFiltered = true;
					styleDescriptionItems.Add ("My votes only");
					Console.WriteLine ("ListPage filter text");
					DebugList ("Mine", filteredList, myVotes);

				} 
				// - chosen friends
				if (FilterVotesBy == VoteFilterWho.Chosen) {
					var chosenFriends = (from kvp in Persist.Instance.Friends
					                     where ((Friend)kvp.Value).InFilter
					                     select kvp.Key).ToList ();
					filteredList = filteredList.Where (v => chosenFriends.IndexOf (v.voter) > -1);
					// as I am not one of the chosen friends, clear my vote dict so my votes don't override theirs
					myVotes.Clear ();
					IsFiltered = true;
					styleDescriptionItems.Add ("From chosen friends");
					Console.WriteLine ("ListPage filter chosen friends (no override for my votes)");
					DebugList ("Friends", filteredList, myVotes);

				}
				if (!string.IsNullOrEmpty (FilterCuisine)) {
					filteredList = filteredList.Where (v => 
						myVotes.ContainsKey (v.key) ?
						myVotes [v.key].cuisineName == FilterCuisine :
						v.cuisineName == FilterCuisine);
					IsFiltered = true;
					styleDescriptionItems.Add ($"Cuisine is {FilterCuisine}");
					Console.WriteLine ("ListPage filter cuisine");
					DebugList ("Cuisine", filteredList, myVotes);
				}

				// - vote value
				switch (FilterVotesKind) {
					case VoteFilterWhat.Like:
						{
							filteredList = filteredList.Where (v =>
                                myVotes.ContainsKey (v.key) ?
								myVotes [v.key].vote == VoteValue.Liked :
								v.vote == VoteValue.Liked);
							IsFiltered = true;
							styleDescriptionItems.Add ("liked places");
							DebugList ("Vote Like", filteredList, myVotes);

							break;
						}
					case VoteFilterWhat.Try:
						{
							filteredList = filteredList.Where (v =>
                                myVotes.ContainsKey (v.key) ?
								myVotes [v.key].vote == VoteValue.Untried || myVotes [v.key].vote == VoteValue.Liked :
								v.vote == VoteValue.Untried || v.vote == VoteValue.Liked);
							filteredList = filteredList.Where (v => v.vote == VoteValue.Untried || v.vote == VoteValue.Liked);
							IsFiltered = true;
							styleDescriptionItems.Add ("places to try");
							DebugList ("Vote Try", filteredList, myVotes);
							break;
						}
					case VoteFilterWhat.Wish:
						{
							filteredList = filteredList.Where (v =>
                               myVotes.ContainsKey (v.key) ?
                               myVotes [v.key].vote == VoteValue.Untried :
                               v.vote == VoteValue.Untried);
							IsFiltered = true;
							styleDescriptionItems.Add ("wishlist places");
							DebugList ("Vote Wish", filteredList, myVotes);
							break;
						}
					default:
						break;
				}
				// - meal kind
				if (FilterPlaceKind != MealKind.None && (int)FilterPlaceKind != Vote.MAX_MEALKIND) {
					filteredList = filteredList.Where (v => 
					                                   myVotes.ContainsKey (v.key) ?
					                                   (myVotes [v.key].kind & FilterPlaceKind) != MealKind.None :
					                                   (v.kind & FilterPlaceKind) != MealKind.None);
					IsFiltered = true;
					styleDescriptionItems.Add ($"{FilterPlaceKind}");
					DebugList ("Kind", filteredList, myVotes);
					Console.WriteLine ("ListPage filter Kind");
				}
				// - place style
				if (FilterPlaceStyle != PlaceStyle.None) {
					DebugList ("pre", filteredList, myVotes);
					filteredList = filteredList.Where (v => 
					                                   (myVotes.ContainsKey (v.key) ?
					                                   myVotes [v.key].style == FilterPlaceStyle :
					                                   v.style == FilterPlaceStyle));
					DebugList ("post", filteredList, myVotes);
					IsFiltered = true;
					styleDescriptionItems.Add ($"{FilterPlaceStyle}");
					DebugList ("Style", filteredList, myVotes);
					Console.WriteLine ("ListPage filter style");
				}

				// turn vote list into place list
				IEnumerable<Place> placeList = filteredList.Select (v => Persist.Instance.GetPlace (v.key)).Distinct ();
//				List<string> placeKeyList = new List<string> ();
//				foreach (Vote v in filteredList) {
//					if (!placeKeyList.Contains (v.key)) {
//						placeKeyList.Add (v.key);
//					}
//				}
//				List<Place> placeList = new List<Place> ();
//				foreach (string key in placeKeyList) {
//					placeList.Add (Persist.Instance.GetPlace (key));
//				}

				// PLACE FILTERS
				if (!string.IsNullOrEmpty (text)) {
					placeList = placeList.Where (p => p.place_name.ToLower ().Contains (text));
					IsFiltered = true;
					styleDescriptionItems.Add ($"Name is '{text}'");
					Console.WriteLine ("ListPage filter text");
				}
				if (FilterSearchCenter != null) {
					var delta = settings.GEO_FILTER_BOX_SIZE_DEG;
					DisplayPosition = (Position)FilterSearchCenter;
					List<Place> distance_list = placeList.ToList ();
					foreach (var p in placeList) {
						p.distance_for_search = p.distance_from (DisplayPosition);
						if (p.distance_for_search < 0.1)
							Console.WriteLine ($"{p.place_name} is {p.distance_for_search}");
					}
					distance_list.Sort ((a, b) => a.distance_for_search.CompareTo (b.distance_for_search));
					Persist.Instance.DisplayList = distance_list;
					Console.WriteLine ("ListPage filter location");
					var savedLocation = Persist.Instance.GetConfig (settings.FILTER_WHERE_NAME);
					if (string.IsNullOrEmpty (savedLocation))
						styleDescriptionItems.Add ("Near location specified");
					else
						styleDescriptionItems.Add ($"Near {savedLocation}");
					IsFiltered = true;
				} else {
					Persist.Instance.DisplayList = placeList.ToList ();
					Persist.Instance.DisplayList.Sort ();
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DoSearch: Exception {0}", ex);
			}
			SetList (Persist.Instance.DisplayList);
			FilterSearchBox.Unfocus ();
			if (IsFiltered) {
				FilterTool.Text = "Filtered";
			} else {
				FilterTool.Text = "Filter";
			}
			FilterDescr.Text = string.Join (", ", styleDescriptionItems);
			filters.IsVisible = IsFiltered;
		}

	

		void InnerSetList (List<Place> list)
		{
			lock (Persist.Instance.Lock) {
				try {
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
					ItemsSource = null;
//					Console.WriteLine ("SetList SORT");
//					list.Sort ();
					ItemsSource = list;
					Spinner.IsVisible = false;
					Spinner.IsRunning = false;
				} catch (Exception ex) {
					Insights.Report (ex);
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
							Navigation.PushModalAsync (new LoginPage ());
						}, 
						incremental: false);
				} catch (ProtocolViolationException) {
					DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
				}
			} else
				InnerSetList (list);
		}

		#endregion

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
						SetList (Persist.Instance.Places);
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
	}

}


