﻿using System;
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

		static ListView listView;
		static FilterKind MainFilter = FilterKind.All;
		static String FilterCuisine;
		StackLayout FilterCuisinePicker;
		StackLayout MainContent;
		Label SplashImage;

		EntryWithButton FilterSearchBox;
		EntryWithButton FilterAreaSearchBox;
		//		Entry AreaBox;
		LabelWithChangeButton LocationButton;
		LabelWithChangeButton CuisineButton;
		ActivityIndicator Spinner;
		ToolbarItem FilterTool;
		string ALL_TYPES_OF_FOOD = "All Types of Food";

		Label NothingFound;
		bool IsFiltered;
		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();
		Grid filters;
		public bool NeedsReload = true;
		List<Place> DisplayList;
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
			SetupFiltersBox ();
			listView = new PlacesListView {
				//ItemsSource = Persist.Instance.Places,
			};
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
					filters.IsVisible = !filters.IsVisible;
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
		public ListPage (MealKind kind, PlaceStyle style, Position? location = null, string cuisine = null) : this ()
		{
			FilterPlaceKind = kind;
			FilterPlaceStyle = style;
			FilterSearchCenter = location;
			FilterCuisine = cuisine;
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
					() => {
						Navigation.PushModalAsync (new LoginPage ());
					},
					() => {
						Refresh ();
						listView.EndRefresh ();
					},
					since: DateTime.UtcNow, 
					incremental: true);
			} catch (ProtocolViolationException ex) {
				DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
			}
		}

		public void DoPickMyLocation (object s, EventArgs e)
		{
			Console.WriteLine ("Listpage.DoPickMyLocation");

			FilterAreaSearchBox.Text = "";
			LocationButton.Text = "Near My Location";
			Console.WriteLine ("ListPage.FilterList pick MY location set posn to {0},{1}", DisplayPosition.Latitude, DisplayPosition.Longitude);

			DisplayPosition = Persist.Instance.GpsPosition;
			IsFiltered = false;
			Spinner.IsVisible = true;
			Spinner.IsRunning = true;
			Console.WriteLine ("Spin");

//			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
			FilterList ();
//			})).Start ();
			Spinner.IsVisible = false;
			Spinner.IsRunning = false;
		}



		async public void DoPlaceSearch (object s, EventArgs e)
		{
			Console.WriteLine ("ListPage.DoPlaceSearch");
//			Spinner.IsVisible = true;
//			Spinner.IsRunning = true;
			var geoCodePositions = (await (new Geocoder ()).GetPositionsForAddressAsync (FilterAreaSearchBox.Text));
//			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
			var positions = geoCodePositions.ToList ();
			if (DEBUG_ON_SIMULATOR || positions.Count > 0) {
				Console.WriteLine ("AddMenu.SearchHere: Got");

				if (DEBUG_ON_SIMULATOR) {
					DisplayPosition = new Position (53.1, -1.5);
					Console.WriteLine ("AddMenu.SearchHere: DEBUG_ON_SIMULATOR");
				} else {
					Console.WriteLine ("ListPage.FilterList places search posn to {0},{1}", DisplayPosition.Latitude, DisplayPosition.Longitude);

					DisplayPosition = positions.First ();
				}
			}
			IsFiltered = true;
			FilterList ();
//			})).Start ();
		}

		async public void DoChangeCuisine (object s, EventArgs e)
		{
			Console.WriteLine ("Listpage.DoChangeCuisine");

			if (String.IsNullOrEmpty (FilterCuisine)) {
				CuisineButton.ButtonText = "Change";
				Content = FilterCuisinePicker;
				// Show it
			} else {
				FilterCuisine = "";
				CuisineButton.Text = "All Types of Food";
				IsFiltered = false;
				Content = MainContent;
				await FilterList ();
			} 
		}

		public async void  DoFilterMine (object s, EventArgs e)
		{
			Console.WriteLine ("Listpage.DoFilterMine");
			MainFilter = FilterKind.Mine;
			IsFiltered = true;
			await FilterList ();
			filters.IsVisible = DisplayList.Count () == 0;
		}

		public async void  DoFilterAll (object s, EventArgs e)
		{
			Console.WriteLine ("Listpage.DoFilterAll");
			MainFilter = FilterKind.All;
			await FilterList ();
			filters.IsVisible = DisplayList.Count () == 0;
		}

		public async void  DoFilterWish (object s, EventArgs e)
		{
			Console.WriteLine ("Listpage.DoChangeWish");
			MainFilter = FilterKind.Wishlist;
			IsFiltered = true;
			await FilterList ();
			filters.IsVisible = DisplayList.Count () == 0;
		}

		void ClearFilter (object s, EventArgs e)
		{ 
			Console.WriteLine ("Listpage.ClearFilter");
			FilterSearchBox.Text = "";
			FilterAreaSearchBox.Text = "";
			FilterCuisine = "";
			FilterPlaceKind = MealKind.None;
			FilterPlaceStyle = PlaceStyle.None;
			CuisineButton.Text = ALL_TYPES_OF_FOOD;
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
			CuisineButton.Text = cuisine;
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
			Navigation.PushAsync (map);
//			} else {
//				Navigation.PushAsync (new MapGooglePage ());
//			}
		}

		public async Task  NarrowGeoSearch ()
		{
			Debug.WriteLine ("Listpage.NarrowGeoSearch");
			try {
				FilterSearchCenter = new Position ();
				var positions = (await (new Geocoder ()).GetPositionsForAddressAsync (FilterAreaSearchBox.Text)).ToList ();
				Console.WriteLine ("ListPage.NarrowGeoSearch: Got");
				if (positions.Count > 0) {
					FilterSearchCenter = positions.First ();
				} else if (DEBUG_ON_SIMULATOR) {
					FilterSearchCenter = new Position (53.1, -1.5);
					Console.WriteLine ("ListPage.NarrowGeoSearch DEBUG_ON_SIMULATOR");
				}

			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		void ResetCuisinePicker ()
		{
//			FilterCuisinePicker.SelectedIndexChanged -= UpdateCuisine;
//			FilterCuisinePicker.SelectedIndex = -1;
//			FilterCuisinePicker.SelectedIndexChanged += UpdateCuisine;
			Debug.WriteLine ("Listpage.ResetCuisinePicker");
		}

		async Task FilterList ()
		{
			Console.WriteLine ("Listpage.FilterList");
			Persist data = Persist.Instance;
			try {
				DisplayList = data.Places;
				if (FilterAreaSearchBox.Text.Length > 0) {
					IsFiltered = true;
					await NarrowGeoSearch ();
				}
				IEnumerable<Place> filteredList = DisplayList;
				String text = FilterSearchBox.Text.ToLower ();
				if (!string.IsNullOrEmpty (FilterCuisine)) {
					filteredList = filteredList.Where (p => p.vote.cuisineName == FilterCuisine);
					IsFiltered = true;
					Console.WriteLine ("ListPage filter cuisine");
				}
				if (!string.IsNullOrEmpty (text)) {
					filteredList = filteredList.Where (p => p.place_name.ToLower ().Contains (text));
					IsFiltered = true;
					Console.WriteLine ("ListPage filter text");
				}
				if (FilterPlaceKind != MealKind.None) {
					filteredList = filteredList.Where (p => (p.vote.kind & FilterPlaceKind) != MealKind.None);
					IsFiltered = true;
					Console.WriteLine ("ListPage filter Kind");
				}
				if (FilterPlaceStyle != PlaceStyle.None) {
					filteredList = filteredList.Where (p => p.vote.style == FilterPlaceStyle);
					IsFiltered = true;
					Console.WriteLine ("ListPage filter style");
				}
				switch (MainFilter) {
					case FilterKind.Mine:
						filteredList = filteredList.Where (p => p.iVoted == true);
						Console.WriteLine ("ListPage filter mine");
						break;
					case FilterKind.Wishlist:
						filteredList = filteredList.Where (p => p.vote.vote == VoteValue.Untried);
						Console.WriteLine ("ListPage filter untried");
						break;
				}
				if (FilterSearchCenter != null) {
					var delta = settings.GEO_FILTER_BOX_SIZE_DEG;
					Position pos = (Position)FilterSearchCenter;
					filteredList = filteredList.Where (
						p => p.lat < pos.Latitude + delta &&
						p.lat > pos.Latitude - delta &&
						p.lng < pos.Longitude + delta &&
						p.lng > pos.Longitude - delta);
					Console.WriteLine ("ListPage filter location");
				}
				DisplayList = filteredList.ToList ();
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DoSearch: Exception {0}", ex);
			}
			SetList (DisplayList);
			FilterSearchBox.Unfocus ();
			if (IsFiltered) {
				FilterTool.Text = "Filtered";
			} else {
				FilterTool.Text = "Filter";
			}
//			});
		}

		void SetupFiltersBox ()
		{
			Console.WriteLine ("Listpage.SetupFiltersBox");
			// FILTER BOX
			var FiltersCloseBtn = new RayvButton ("Clear Filter") {
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			FiltersCloseBtn.Clicked += ClearFilter;
			var FilterMineBtn = new ButtonWide ("My Places"){ TextColor = settings.ColorLightGray, };
			FilterMineBtn.Clicked += DoFilterMine;
			var FilterAllBtn = new ButtonWide ("All Places"){ TextColor = settings.ColorLightGray, };
			FilterAllBtn.Clicked += DoFilterAll;
			var FilterNewBtn = new ButtonWide ("New Places"){ TextColor = settings.ColorLightGray, };
			FilterNewBtn.Clicked += (object sender, EventArgs e) => {
				DisplayAlert ("Not Implemented", "New Places is not done yet", "OK");
			};
			var FilterWishBtn = new ButtonWide ("Wishlist"){ TextColor = settings.ColorLightGray, };
			FilterWishBtn.Clicked += DoFilterWish;
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
			FilterAreaSearchBox = new EntryWithButton {
				Placeholder = "Search in an Area",
				Source = settings.DevicifyFilename ("TB active search.png"),
				OnClick = DoTextSearch,
				Text = "",
			};
			FilterAreaSearchBox.TextEntry.Completed += (sender, e) => {
				FilterAreaSearchBox.TextEntry.Unfocus ();
				DoTextSearch (sender, e);
			};
//			AreaBox = new Entry {
//				Placeholder = "Enter Area to Search",
//				Text = "",
//			};
//			AreaBox.BackgroundColor = settings.ColorLightGray;

			LocationButton = new LabelWithChangeButton {
				Text = "Near My Location",
				OnClick = DoPickMyLocation,
				ButtonText = "",
				Padding = new Thickness (5, 10, 5, 0),
			};

			CuisineButton = new LabelWithChangeButton {
				Text = ALL_TYPES_OF_FOOD,
				ButtonText = "",
				OnClick = DoChangeCuisine,
			};
			filters = new Grid {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = settings.BaseColor,
//				ColumnSpacing = 5,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (40) },
					new RowDefinition { Height = new GridLength (40) },
					new RowDefinition { Height = new GridLength (40) },
					new RowDefinition { Height = new GridLength (40) },
					new RowDefinition { Height = new GridLength (40) },

				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (25, GridUnitType.Absolute) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (2, GridUnitType.Absolute) },
				}
			};
			filters.Children.Add (
				new Image{ Source = settings.DevicifyFilename ("Icon default directions.png"), Aspect = Aspect.AspectFit, }, 0, 1, 0, 1);
			filters.Children.Add (LocationButton, 1, 2, 0, 1);
			filters.Children.Add (
				new ImageButton {
					Source = settings.DevicifyFilename ("arrow.png"), Aspect = Aspect.AspectFit, 
					OnClick = DoPickMyLocation,
				}, 2, 3, 0, 1);

			filters.Children.Add (
				new Image{ Source = settings.DevicifyFilename ("Icon default directions1.png"), Aspect = Aspect.AspectFit, }, 0, 1, 1, 2);
			filters.Children.Add (FilterAreaSearchBox, 1, 2, 1, 2);
			filters.Children.Add (
				new ImageButton { 
					Source = settings.DevicifyFilename ("arrow.png"), Aspect = Aspect.AspectFit, 
					OnClick = DoPlaceSearch,
				}, 2, 3, 1, 2);

			filters.Children.Add (
				new Image{ Source = settings.DevicifyFilename ("Icon default website.png"), Aspect = Aspect.AspectFit, }, 0, 1, 2, 3);
			filters.Children.Add (CuisineButton, 1, 2, 2, 3);
			filters.Children.Add (
				new ImageButton { 
					Source = settings.DevicifyFilename ("arrow.png"), Aspect = Aspect.AspectFit, 
					OnClick = DoChangeCuisine,
				}, 2, 3, 2, 3);
			
			filters.Children.Add (
				new Image{ Source = settings.DevicifyFilename ("TB default profile.png"), Aspect = Aspect.AspectFit, }, 0, 1, 3, 4);
			filters.Children.Add (new StackLayout {
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					FilterMineBtn,
					FilterWishBtn,
				},
			}, 1, 2, 3, 4);

			filters.Children.Add (FiltersCloseBtn, 0, 2, 4, 5);
			// CONTROLS
			ListView innerCuisinePickerLV = new ListView {
				ItemsSource = Persist.Instance.CategoryCounts,
				RowHeight = 30,
			};
			FilterCuisinePicker = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new RayvButton ("All kinds") {
						OnClick = (sender, e) => {
							FilterCuisine = "All";
							DoChangeCuisine (null, null);
						}
					},
					innerCuisinePickerLV,
				}
			};
//			filters.Children.Add (FilterCuisinePicker, 0, 2, 0, 5);
			innerCuisinePickerLV.ItemTemplate = new DataTemplate (() => {
				Label cuisineType = new Label ();
				cuisineType.SetBinding (Label.TextProperty, "Key");
				Label cuisineCount = new Label {
					FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					TranslationY = 4,
					TextColor = Color.Gray,
				};
				cuisineCount.SetBinding (Label.TextProperty, "Value");
				return new ViewCell {
					View = new StackLayout {
						Padding = new Thickness (5, 2, 0, 0),
						VerticalOptions = LayoutOptions.Center,
						Orientation = StackOrientation.Horizontal,
						Children = {
							cuisineType,
							cuisineCount,
						}
					},
				};
			});
			innerCuisinePickerLV.ItemTapped += UpdateCuisine;
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
					Console.WriteLine ("SetList SORT");
					list.Sort ();
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
							Navigation.PushModalAsync (new LoginPage ());
						}, 
						onSucceed: () => {
							InnerSetList (Persist.Instance.Places);
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


