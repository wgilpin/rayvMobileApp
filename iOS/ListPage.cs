using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using CoreLocation;
using RestSharp;
using System.Linq;
using System.Diagnostics;
using Xamarin;
using Xamarin.Forms.Maps;

namespace RayvMobileApp.iOS
{

	enum FilterKind: short
	{
		Mine,
		All,
		Cuisine,
		Wishlist,
		New,
		Go,
	}

	public class ListPage : ContentPage
	{
		#region Fields

		static ListView listView;
		static FilterKind MainFilter = FilterKind.All;
		static String FilterCuisineKind;
		ListView FilterCuisinePicker;

		EntryWithButton FilterSearchBox;
		EntryWithButton FilterAreaSearchBox;
		EntryWithButton AreaBox;
		LabelWithChangeButton LocationButton;
		LabelWithChangeButton CuisineButton;
		ActivityIndicator Spinner;
		ToolbarItem FilterTool;

		private Position? lastPositionOnListPage;
		Label NothingFound;
		bool IsFiltered;
		bool DEBUG_ON_SIMULATOR = (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR);
		Grid filters;
		public bool NeedsReload = true;

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
			Console.WriteLine ("ListView()");
			Xamarin.FormsMaps.Init ();
			this.Title = "Find Food";
			this.Icon = "bars-black.png";
			IsFiltered = false;

			// FILTER BOX

			var FiltersCloseBtn = new RayvButton ("Clear Filter") {
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			FiltersCloseBtn.Clicked += ClearFilter;

			var FilterMineBtn = new ButtonWide ("My Places");
			FilterMineBtn.Clicked += DoFilterMine;

			var FilterAllBtn = new ButtonWide ("All Places");
			FilterAllBtn.Clicked += DoFilterAll;

			var FilterNewBtn = new ButtonWide ("New Places");
			FilterNewBtn.Clicked += (object sender, EventArgs e) => {
				DisplayAlert ("Not Implemented", "New Places is not done yet", "OK");
			};

			var FilterWishBtn = new ButtonWide ("Wishlist");
			FilterWishBtn.Clicked += DoFilterWish;

			FilterSearchBox = new EntryWithButton {
				Placeholder = "Search for place",
				Source = "icon-06-magnify@2x.png",
				OnClick = DoTextSearch,
				Text = "",
			};

			FilterAreaSearchBox = new EntryWithButton {
				Placeholder = "Search in an Area",
				Source = "icon-06-magnify@2x.png",
				OnClick = DoTextSearch,
				Text = "",
			};

			filters = new Grid {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			filters.Children.Add (FilterMineBtn, 0, 1, 0, 1);
			filters.Children.Add (FilterAllBtn, 1, 2, 0, 1);
			filters.Children.Add (FilterWishBtn, 0, 1, 1, 2);
			filters.Children.Add (FilterNewBtn, 1, 2, 1, 2);
			//			filters.Children.Add (FilterCuisinePicker, 0, 2, 3, 4);
			//			filters.Children.Add (FilterAreaSearchBox, 0, 2, 4, 5);
			filters.Children.Add (FiltersCloseBtn, 0, 2, 4, 5);


			// CONTROLS

			FilterCuisinePicker = new ListView {
				ItemsSource = Persist.Instance.CategoryCounts,
				RowHeight = 30,
			};
			FilterCuisinePicker.ItemTemplate = new DataTemplate (() => {

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
							cuisineType, cuisineCount,
						}
					},
				};
			});


			FilterCuisinePicker.ItemTapped += UpdateCuisine;
				
			AreaBox = new EntryWithButton {
				Placeholder = "Enter Area to Search",
				Source = "icon-06-magnify@2x.png",
				OnClick = DoPlaceSearch,
				Text = "",
//				HeightRequest = 30,
			};

			LocationButton = new LabelWithChangeButton {
				Text = "Near My Location",
				OnClick = DoPickLocation,
				Padding = new Thickness (5, 5, 5, 0),
			};

			CuisineButton = new LabelWithChangeButton {
				Text = "All Types of Food",
				OnClick = DoChangeCuisine,
			};


			listView = new PlacesListView {
				//ItemsSource = Persist.Instance.Places,
			};
			listView.ItemTapped += DoSelectListItem;
			StackLayout tools = new BottomToolbar (this, "list");
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			Grid grid = new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (30, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (5, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (35, GridUnitType.Absolute) }
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
				IsRunning = true,
				Color = Color.Red,
			};
			grid.Children.Add (FilterSearchBox, 0, 0);
			grid.Children.Add (new StackLayout {
				HorizontalOptions = LayoutOptions.StartAndExpand,
				VerticalOptions = LayoutOptions.Start,
				Children = {
					LocationButton,
					AreaBox,
					Spinner,
				}
			}, 0, 1);
			grid.Children.Add (new StackLayout {
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Children = {
					CuisineButton,
					FilterCuisinePicker,
				}
			}, 0, 2);
			grid.Children.Add (inner, 0, 3);
			grid.Children.Add (tools, 0, 4);
			filters.IsVisible = false;
			FilterCuisinePicker.IsVisible = false;
			AreaBox.IsVisible = false;
			this.Content = grid;

			ToolbarItems.Add (new ToolbarItem {
				Text = "Map",
//				Icon = "icon-map.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (ShowMap),
			});

			FilterTool = new ToolbarItem {
				Text = "Filter",
				//				Icon = "filter.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("ListPage Toolbar Filter");
					filters.IsVisible = !filters.IsVisible;
				})
			};
			ToolbarItems.Add (FilterTool);

			NeedsReload = true;
			Persist.Instance.DisplayPosition = Persist.Instance.GpsPosition;
			Console.WriteLine ("ListPage.FilterList Constructor set posn to {0},{1}", Persist.Instance.DisplayPosition.Latitude, Persist.Instance.DisplayPosition.Longitude);

			this.Appearing += OnPageAppearing;
		}




		/**
		 * Constructor when a cuisine is supplied
		 */
		public ListPage (string cuisine) : this ()
		{
			MainFilter = FilterKind.Go;
			FilterCuisineKind = cuisine;
			FilterList ();
		}

		private static NavigationPage _instance;

		public static NavigationPage Instance {
			get {
				if (_instance == null) {
					Console.WriteLine ("ListPage: Instance create");
					_instance = new NavigationPage (new ListPage ());
				}
				Console.WriteLine ("ListPage: Instance exists");
				return _instance;
			}
		}

		#endregion

		#region Events

		void DoSelectListItem (object sender, ItemTappedEventArgs e)
		{
			Debug.WriteLine ("Listpage.ItemTapped: Push DetailPage");
			this.Navigation.PushAsync (new DetailPage (e.Item as Place));
		}

		async void OnPageAppearing (object sender, EventArgs e)
		{
			if (NeedsReload || (lastPositionOnListPage != Persist.Instance.DisplayPosition)) {
				Console.WriteLine (
					"ListPage appearing reload {0},{1}", 
					Persist.Instance.DisplayPosition.Latitude, 
					Persist.Instance.DisplayPosition.Longitude);
				if (lastPositionOnListPage == null)
					lastPositionOnListPage = Persist.Instance.DisplayPosition;
				if (Persist.Instance.DisplayList == null)
					Persist.Instance.DisplayList = Persist.Instance.Places;
				await FilterList ();
				StartTimerIfNoGPS ();
				NeedsReload = false;
			}
		}

		public void DoPickLocation (object s, EventArgs e)
		{
			if (AreaBox.IsVisible) {
				AreaBox.IsVisible = false;
				LocationButton.ButtonText = "Change";
				Console.WriteLine ("ListPage.FilterList pick location set posn to {0},{1}", Persist.Instance.DisplayPosition.Latitude, Persist.Instance.DisplayPosition.Longitude);

				Persist.Instance.DisplayPosition = Persist.Instance.GpsPosition;
				IsFiltered = false;
				Spinner.IsRunning = true;
				Console.WriteLine ("Spin");
				new System.Threading.Thread (new System.Threading.ThreadStart (() => {

					FilterList ();

				})).Start ();
			} else {
				AreaBox.IsVisible = true;
				LocationButton.ButtonText = "Clear";
			}
		}

		async public void DoPlaceSearch (object s, EventArgs e)
		{
			Spinner.IsRunning = true;
			var geoCodePositions = (await (new Geocoder ()).GetPositionsForAddressAsync (AreaBox.Text));
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				var positions = geoCodePositions.ToList ();
				if (DEBUG_ON_SIMULATOR || positions.Count > 0) {
					Console.WriteLine ("AddMenu.SearchHere: Got");
					if (DEBUG_ON_SIMULATOR) {
						Persist.Instance.DisplayPosition = new Position (53.1, -1.5);
						Console.WriteLine ("AddMenu.SearchHere: DEBUG_ON_SIMULATOR");
					} else {
						Console.WriteLine ("ListPage.FilterList places search posn to {0},{1}", Persist.Instance.DisplayPosition.Latitude, Persist.Instance.DisplayPosition.Longitude);

						Persist.Instance.DisplayPosition = positions.First ();
					}
				}
				IsFiltered = true;
				FilterList ();
			})).Start ();
		}

		async public void DoChangeCuisine (object s, EventArgs e)
		{
			if (String.IsNullOrEmpty (FilterCuisineKind)) {
				CuisineButton.ButtonText = "Clear";
				if (FilterCuisinePicker.IsVisible) {
					CuisineButton.ButtonText = "Change";
				} 
				FilterCuisinePicker.IsVisible = !FilterCuisinePicker.IsVisible;
				// Show it
			} else {
				CuisineButton.ButtonText = "Change";
				FilterCuisineKind = "";
				CuisineButton.Text = "All Types of Food";
				IsFiltered = false;
				await FilterList ();
			} 
		}

		public async void  DoFilterMine (object s, EventArgs e)
		{
			MainFilter = FilterKind.Mine;
			await FilterList ();
			filters.IsVisible = Persist.Instance.DisplayList.Count () == 0;
		}

		public async void  DoFilterAll (object s, EventArgs e)
		{
			MainFilter = FilterKind.All;
			await FilterList ();
			filters.IsVisible = Persist.Instance.DisplayList.Count () == 0;
		}

		public async void  DoFilterWish (object s, EventArgs e)
		{
			MainFilter = FilterKind.Wishlist;
			await FilterList ();
			filters.IsVisible = Persist.Instance.DisplayList.Count () == 0;
		}

		void ClearFilter (object s, EventArgs e)
		{ 
			FilterSearchBox.Text = "";
			FilterAreaSearchBox.Text = "";
			filters.IsVisible = false;
			MainFilter = FilterKind.All;
			Persist.Instance.DisplayPosition = Persist.Instance.GpsPosition;
			Console.WriteLine ("ListPage.FilterList clearFilter set posn to {0},{1}", Persist.Instance.DisplayPosition.Latitude, Persist.Instance.DisplayPosition.Longitude);

			IsFiltered = false;
			FilterList ();
		}


		void DoTextSearch (object sender, EventArgs e)
		{
			FilterList ();
		}

		void UpdateCuisine (Object sender, ItemTappedEventArgs e)
		{
			MainFilter = FilterKind.Cuisine;
			string cuisine = ((KeyValuePair<string,int>)e.Item).Key;
			FilterCuisineKind = cuisine;
			CuisineButton.Text = cuisine;
			FilterCuisinePicker.IsVisible = false;
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {

				FilterList ();

			})).Start ();
			filters.IsVisible = false;
		}

		#endregion

		#region Methods

		void ShowMap ()
		{
			Debug.WriteLine ("ListPage ShowMap: Push GOOGLE MapPage");
			// keep last posn so we can tell if it changed
			lastPositionOnListPage = Persist.Instance.DisplayPosition;
			if (settings.USE_XAMARIN_MAPS) {
				MapPage map = new MapPage ();
				Navigation.PushAsync (map);
			} else {
				Navigation.PushAsync (new MapGooglePage ());
			}
		}

		public async Task  NarrowGeoSearch ()
		{
			try {
				Position centre = new Position ();
				var positions = (await (new Geocoder ()).GetPositionsForAddressAsync (FilterAreaSearchBox.Text)).ToList ();
				Console.WriteLine ("ListPage.NarrowGeoSearch: Got");
				if (positions.Count > 0) {
					centre = positions.First ();
				} else if (DEBUG_ON_SIMULATOR) {
					centre = new Position (53.1, -1.5);
					Console.WriteLine ("ListPage.NarrowGeoSearch DEBUG_ON_SIMULATOR");
				}
				var delta = settings.GEO_FILTER_BOX_SIZE_DEG;
				Persist.Instance.DisplayList = Persist.Instance.DisplayList.Where (
					p => p.lat < centre.Latitude + delta &&
					p.lat > centre.Latitude - delta &&
					p.lng < centre.Longitude + delta &&
					p.lng > centre.Longitude - delta).ToList ();
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}



		void ResetCuisinePicker ()
		{
//			FilterCuisinePicker.SelectedIndexChanged -= UpdateCuisine;
//			FilterCuisinePicker.SelectedIndex = -1;
//			FilterCuisinePicker.SelectedIndexChanged += UpdateCuisine;
		}



		async Task FilterList ()
		{
			Persist data = Persist.Instance;
			if (Persist.Instance.DisplayPosition != lastPositionOnListPage) {
				Console.WriteLine ("ListPage.FilterList set posn to {0},{1}", Persist.Instance.DisplayPosition.Latitude, Persist.Instance.DisplayPosition.Longitude);
				lastPositionOnListPage = Persist.Instance.DisplayPosition;
				foreach (var p in Persist.Instance.DisplayList)
					p.CalculateDistanceFromPlace (Persist.Instance.DisplayPosition);
			}
			try {
				String text = FilterSearchBox.Text.ToLower ();
				if (text.Length > 0)
					IsFiltered = true;
				switch (MainFilter) {
				case FilterKind.Go:
					// places to go - from cuisine string constructorWill
					ResetCuisinePicker ();
					data.DisplayList = (
					    from p in data.Places
					    where
					        p.vote != "-1" &&
					        p.category == FilterCuisineKind
					    select p).ToList ();
					IsFiltered = true;
					break;
				case FilterKind.Mine:
					ResetCuisinePicker ();
					data.DisplayList = (
					    from p in data.Places
					    where p.iVoted == true && (
					            p.place_name.ToLower ().Contains (text) ||
					            p.CategoryLowerCase.Contains (text))
					    select p).ToList ();
					break;
				case FilterKind.All:
					ResetCuisinePicker ();
					data.DisplayList = (from p in data.Places
					                    where
					                        p.place_name.ToLower ().Contains (text) ||
					                        p.CategoryLowerCase.Contains (text)
					                    select p).ToList ();
					break;
				case FilterKind.Cuisine:
					if (FilterCuisineKind != null && FilterCuisineKind.Length > 0)
						data.DisplayList = (
						    from p in data.Places
						    where p.category == FilterCuisineKind && (
						            p.place_name.ToLower ().Contains (text) ||
						            p.CategoryLowerCase.Contains (text))
						    select p).ToList ();
					else
						goto case FilterKind.All;
					IsFiltered = true;
					break;
				case FilterKind.Wishlist:
					data.DisplayList = (
					    from p in data.Places
					    where p.untried == true && (
					            p.place_name.ToLower ().Contains (text) ||
					            p.CategoryLowerCase.Contains (text))
					    select p).ToList ();
					IsFiltered = true;
					break;
				}
				if (FilterAreaSearchBox.Text.Length > 0) {
					IsFiltered = true;
					await NarrowGeoSearch ();
				}
				lock (Persist.Instance.Lock) {
					data.SortPlaces (data.DisplayList);
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DoSearch: Exception {0}", ex);
			}
			Device.BeginInvokeOnMainThread (() => {
				SetList (data.DisplayList);
				FilterSearchBox.Unfocus ();
				Spinner.IsRunning = false;
				if (IsFiltered) {
					FilterTool.Text = "Filtered";
				} else {
					FilterTool.Text = "Filter";
				}
			});
		}


		public static void Setup (Page caller)
		{
			Console.WriteLine ("ListPage.Setup");
			// fire off a thread to get the data
			System.Threading.ThreadPool.QueueUserWorkItem (delegate {
				Persist.Instance.GetUserData (caller, incremental: true);
			}, null);

			System.Diagnostics.Debug.WriteLine ("ListPage.Setup out");
		}

		public void SetList (List<Place> list)
		{
			if (Persist.Instance.Places.Count () == 0)
				Setup (this);
			else {
				lock (Persist.Instance.Lock) {
					try {
						Console.WriteLine ("SetList {0}", list.Count);
						if (list.Count == 0) {
							listView.IsVisible = false;
							NothingFound.IsVisible = true;
							return;
						}
						NothingFound.WidthRequest = this.Width;
						NothingFound.IsVisible = false;
						listView.IsVisible = true;
						ItemsSource = null;
						list.Sort ();
						ItemsSource = list;
						Spinner.IsRunning = false;

					} catch (Exception ex) {
						Insights.Report (ex);
						restConnection.LogErrorToServer ("ListPage.SetList Exception {0}", ex);
					}
				}
			}
		}

		#endregion

		#region timer

		private System.Timers.Timer _timer;

		void StartTimerIfNoGPS ()
		{
			if (Persist.Instance.DataIsLive)
				return;
			_timer = new System.Timers.Timer ();
			//Trigger event every second
			_timer.Interval = 2000;
			_timer.Elapsed += OnTimerTrigger;
			_timer.Enabled = true;
		}

		private void OnTimerTrigger (object sender, System.Timers.ElapsedEventArgs e)
		{
			if (!Persist.Instance.DataIsLive) {
				// not ready yet
				//Debug.WriteLine ("OnTimerTrigger - not live");
				return;
			}
			Debug.WriteLine ("OnTimerTrigger - Live");
			lock (Persist.Instance.Lock) {
				try {
					SetList (Persist.Instance.Places);
				} catch (Exception ex) {
					Insights.Report (ex);
					restConnection.LogErrorToServer ("ListPage.OnTimerTrigger Exception {0}", ex);
				}
			}
			_timer.Close ();
		}

		#endregion
	}

}


