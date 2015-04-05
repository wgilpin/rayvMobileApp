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

namespace RayvMobileApp
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
		Entry AreaBox;
		LabelWithChangeButton LocationButton;
		LabelWithChangeButton CuisineButton;
		ActivityIndicator Spinner;
		ToolbarItem FilterTool;

		Label NothingFound;
		bool IsFiltered;
		bool DEBUG_ON_SIMULATOR = (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR);
		Grid filters;
		public bool NeedsReload = true;
		List<Place> DisplayList;
		Position DisplayPosition;

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
			Xamarin.FormsMaps.Init ();
			this.Title = "Find Food";
			this.Icon = "bars-black.png";
			IsFiltered = false;

			SetupFiltersBox ();




			listView = new PlacesListView {
				//ItemsSource = Persist.Instance.Places,
			};
			listView.ItemTapped += DoSelectListItem;
			StackLayout tools = new BottomToolbar (this, "list");
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			Grid grid = new Grid {
//				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				ColumnSpacing = 0,
				RowSpacing = 0,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (35, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
//					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) }
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
				BackgroundColor = settings.ColorDark,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};
			BoxView bg1 = new BoxView { 
				BackgroundColor = settings.ColorDark,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};
			BoxView bg2 = new BoxView { 
				BackgroundColor = settings.ColorDark,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};
			grid.Children.Add (bg0, 0, 0);
			grid.Children.Add (FilterSearchBox, 0, 0);
			grid.Children.Add (Spinner, 0, 1);
//			grid.Children.Add (new StackLayout {
//				BackgroundColor = settings.ColorDark,
//				HorizontalOptions = LayoutOptions.StartAndExpand,
//				Children = {
//					new Label { Text = "SL1" },
//					new Label { Text = "SL2" }
//					filters,
//					Spinner,
//				}
//			}, 0, 1);
			grid.Children.Add (inner, 0, 2);
			filters.IsVisible = false;
			FilterCuisinePicker.IsVisible = false;
			Content = new StackLayout {
				Children = {
					grid,
					tools
				},
				Padding = 0,
			};

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
			DisplayPosition = Persist.Instance.GpsPosition;
			Console.WriteLine ("ListPage.FilterList Constructor set posn to {0},{1}", DisplayPosition.Latitude, DisplayPosition.Longitude);

			this.Appearing += OnPageAppearing;
			this.Disappearing += (sender, e) => {
				if (_timer != null)
					_timer.Close ();
			};
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
			var place = e.Item as Place;
			if (place.IsDraft) {
				NeedsReload = true;
				Navigation.PushAsync (new EditPage (place));
			} else {
				Navigation.PushAsync (new DetailPage (place));
			}
		}

		async void OnPageAppearing (object sender, EventArgs e)
		{
			if (NeedsReload) {
				await FilterList ();
				StartTimerIfNoGPS ();
				NeedsReload = false;
			}
		}

		public void DoPickLocation (object s, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace (AreaBox.Text)) {
				AreaBox.Text = "";
				LocationButton.Text = "Near My Location";
				LocationButton.ButtonText = "Change";
				Console.WriteLine ("ListPage.FilterList pick location set posn to {0},{1}", DisplayPosition.Latitude, DisplayPosition.Longitude);

				DisplayPosition = Persist.Instance.GpsPosition;
				IsFiltered = false;
				Spinner.IsVisible = true;
				Spinner.IsRunning = true;
				Console.WriteLine ("Spin");
				new System.Threading.Thread (new System.Threading.ThreadStart (() => {

					FilterList ();

				})).Start ();
			} else {
				LocationButton.ButtonText = "Clear";
			}
		}

		async public void DoPlaceSearch (object s, EventArgs e)
		{
			Console.WriteLine ("DoPlaceSearch");
			Spinner.IsVisible = true;
			Spinner.IsRunning = true;
			var geoCodePositions = (await (new Geocoder ()).GetPositionsForAddressAsync (AreaBox.Text));
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				var positions = geoCodePositions.ToList ();
				if (DEBUG_ON_SIMULATOR || positions.Count > 0) {
					Console.WriteLine ("AddMenu.SearchHere: Got");
					Device.BeginInvokeOnMainThread (() => {
						LocationButton.Text = String.Format ("Near {0}", AreaBox.Text);
					});
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
			filters.IsVisible = DisplayList.Count () == 0;
		}

		public async void  DoFilterAll (object s, EventArgs e)
		{
			MainFilter = FilterKind.All;
			await FilterList ();
			filters.IsVisible = DisplayList.Count () == 0;
		}

		public async void  DoFilterWish (object s, EventArgs e)
		{
			MainFilter = FilterKind.Wishlist;
			await FilterList ();
			filters.IsVisible = DisplayList.Count () == 0;
		}

		void ClearFilter (object s, EventArgs e)
		{ 
			FilterSearchBox.Text = "";
			FilterAreaSearchBox.Text = "";
			filters.IsVisible = false;
			MainFilter = FilterKind.All;
			DisplayPosition = Persist.Instance.GpsPosition;
			Console.WriteLine ("ListPage.FilterList clearFilter set posn to {0},{1}", DisplayPosition.Latitude, DisplayPosition.Longitude);

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
			Spinner.IsVisible = true;
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
				DisplayList = DisplayList.Where (
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
			data.SortPlaces (updateDistancePosition: DisplayPosition);
			try {
				String text = FilterSearchBox.Text.ToLower ();
				if (text.Length > 0)
					IsFiltered = true;
				switch (MainFilter) {
				case FilterKind.Go:
					// places to go - from cuisine string constructorWill
					ResetCuisinePicker ();
					DisplayList = (
					    from p in data.Places
					    where
					        p.vote != "-1" &&
					        p.category == FilterCuisineKind
					    select p).ToList ();
					IsFiltered = true;
					break;
				case FilterKind.Mine:
					ResetCuisinePicker ();
					DisplayList = (
					    from p in data.Places
					    where p.iVoted == true && (
					            p.place_name.ToLower ().Contains (text) ||
					            p.CategoryLowerCase.Contains (text))
					    select p).ToList ();
					break;
				case FilterKind.All:
					ResetCuisinePicker ();
					DisplayList = (from p in data.Places
					               where
					                   p.place_name.ToLower ().Contains (text) ||
					                   p.CategoryLowerCase.Contains (text)
					               select p).ToList ();
					break;
				case FilterKind.Cuisine:
					if (FilterCuisineKind != null && FilterCuisineKind.Length > 0)
						DisplayList = (
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
					DisplayList = (
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
					data.SortPlaces (DisplayList);
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DoSearch: Exception {0}", ex);
			}
			Device.BeginInvokeOnMainThread (() => {
				DisplayList.Sort ();
				SetList (DisplayList);
				FilterSearchBox.Unfocus ();
				Spinner.IsVisible = false;
				Spinner.IsRunning = false;
				if (IsFiltered) {
					FilterTool.Text = "Filtered";
				} else {
					FilterTool.Text = "Filter";
				}
			});
		}

		void SetupFiltersBox ()
		{
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
				Source = "TB active search.png",
				OnClick = DoTextSearch,
				Text = "",
			};
			FilterSearchBox.TextEntry.BackgroundColor = settings.ColorLightGray;
			FilterAreaSearchBox = new EntryWithButton {
				Placeholder = "Search in an Area",
				Source = "icon-06-magnify@2x.png",
				OnClick = DoTextSearch,
				Text = "",
			};
			AreaBox = new Entry {
				Placeholder = "Enter Area to Search",
				Text = "",
			};
			AreaBox.BackgroundColor = settings.ColorLightGray;

			LocationButton = new LabelWithChangeButton {
				Text = "Near My Location",
				OnClick = DoPickLocation,
				ButtonText = "",
				Padding = new Thickness (5, 10, 5, 0),
			};

			CuisineButton = new LabelWithChangeButton {
				Text = "All Types of Food",
				ButtonText = "",
				OnClick = DoChangeCuisine,
			};
			filters = new Grid {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = settings.ColorDark,
//				ColumnSpacing = 5,
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },

				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (25, GridUnitType.Absolute) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (20, GridUnitType.Absolute) },
				}
			};
			filters.Children.Add (
				new Image{ Source = "Icon default directions.png", Aspect = Aspect.AspectFit, }, 0, 1, 0, 1);
			filters.Children.Add (LocationButton, 1, 2, 0, 1);
			filters.Children.Add (
				new ImageButton {
					Source = "Add Select right button.png", Aspect = Aspect.AspectFit, 
					OnClick = DoPickLocation,
				}, 2, 3, 0, 1);

			filters.Children.Add (
				new Image{ Source = "Icon default directions1.png", Aspect = Aspect.AspectFit, }, 0, 1, 1, 2);
			filters.Children.Add (AreaBox, 1, 2, 1, 2);
			filters.Children.Add (
				new ImageButton { 
					Source = "Add Select right button.png", Aspect = Aspect.AspectFit, 
					OnClick = DoTextSearch,
				}, 2, 3, 1, 2);

			filters.Children.Add (
				new Image{ Source = "Icon default website.png", Aspect = Aspect.AspectFit, }, 0, 1, 2, 3);
			filters.Children.Add (CuisineButton, 1, 2, 2, 3);
			filters.Children.Add (
				new ImageButton { 
					Source = "Add Select right button.png", Aspect = Aspect.AspectFit, 
					OnClick = DoChangeCuisine,
				}, 2, 3, 2, 3);
			
			filters.Children.Add (
				new Image{ Source = "TB default profile.png", Aspect = Aspect.AspectFit, }, 0, 1, 3, 4);
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
							cuisineType,
							cuisineCount,
						}
					},
				};
			});
			FilterCuisinePicker.ItemTapped += UpdateCuisine;
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
				Device.BeginInvokeOnMainThread (() => {
					Spinner.IsVisible = true;
					Spinner.IsRunning = true;
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
							Spinner.IsVisible = false;
							Spinner.IsRunning = false;
							
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("ListPage.SetList Exception {0}", ex);
						}
					}
				});
			}
		}

		#endregion

		#region timer

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
		}

		#endregion
	}

}


