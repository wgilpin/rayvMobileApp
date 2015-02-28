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
		List<Place> currentPlaces;
		EntryWithButton FilterSearchBox;
		EntryWithButton FilterAreaSearchBox;
		EntryWithButton AreaBox;
		LabelWithChangeButton LocationButton;
		LabelWithChangeButton CuisineButton;
		Position SearchPosition;
		ActivityIndicator Spinner;

		Label NothingFound;
		Page Caller;
		bool DEBUG_ON_SIMULATOR = (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR);
		Grid filters;

		public static IEnumerable ItemsSource {
			set {
				listView.ItemsSource = value;
			}
		}

		#endregion

		#region Constructors


		public ListPage ()
		{
			Console.WriteLine ("ListView()");
			Xamarin.FormsMaps.Init ();
			this.Title = "List";
			this.Icon = "bars-black.png";

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
				Placeholder = "Search for text",
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
					new RowDefinition { Height = GridLength.Auto },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			filters.Children.Add (FilterSearchBox, 0, 2, 0, 1);
			filters.Children.Add (FilterMineBtn, 0, 1, 1, 2);
			filters.Children.Add (FilterAllBtn, 1, 2, 1, 2);
			filters.Children.Add (FilterWishBtn, 0, 1, 2, 3);
			filters.Children.Add (FilterNewBtn, 1, 2, 2, 3);
			//			filters.Children.Add (FilterCuisinePicker, 0, 2, 3, 4);
			//			filters.Children.Add (FilterAreaSearchBox, 0, 2, 4, 5);
			filters.Children.Add (FiltersCloseBtn, 0, 2, 5, 6);


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
				HeightRequest = 30,
			};

			LocationButton = new LabelWithChangeButton {
				Text = "Near My Location",
				OnClick = DoPickLocation,
				Padding = new Thickness (5, 15, 5, 0),
			};

			CuisineButton = new LabelWithChangeButton {
				Text = "All Types of Food",
				OnClick = DoChangeCuisine,
			};


			listView = new PlacesListView {
				//ItemsSource = Persist.Instance.Places,
			};
			listView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
				Debug.WriteLine ("Listpage.ItemTapped: Push DetailPage");
				this.Navigation.PushAsync (new DetailPage (e.Item as Place));
			};
			StackLayout tools = new BottomToolbar (this, "list");
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			Grid grid = new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (30, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (30, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (35, GridUnitType.Auto) }
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Auto },
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
				IsRunning = false,
			};
			grid.Children.Add (new StackLayout {
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Children = {
					LocationButton,
					AreaBox,
					Spinner,
				}
			}, 0, 0);
			grid.Children.Add (new StackLayout {
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Children = {
					CuisineButton,
					FilterCuisinePicker,
				}
			}, 0, 1);
			grid.Children.Add (inner, 0, 2);
			grid.Children.Add (tools, 0, 3);
			filters.IsVisible = false;
			FilterCuisinePicker.IsVisible = false;
			AreaBox.IsVisible = false;
			this.Content = grid;

			ToolbarItems.Add (new ToolbarItem {
				Text = "Map",
//				Icon = "icon-map.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("ListPage Toolbar Map: Push GOOGLE MapPage");
					if (settings.USE_XAMARIN_MAPS) {
						Navigation.PushAsync (new MapPage ());
					} else {
						Navigation.PushAsync (new MapGooglePage ());
					}
				})
			});

//			ToolbarItems.Add (new ToolbarItem {
//				Text = "Filter",
////				Icon = "filter.png",
//				Order = ToolbarItemOrder.Primary,
//				Command = new Command (() => {
//					Debug.WriteLine ("ListPage Toolbar Filter");
//					filters.IsVisible = !filters.IsVisible;
//				})
//			});

			this.Appearing += OnPageAppearing;

		}



		/**
		 * Constructor when a list of Places is supplied
		 */
		public ListPage (List<Place> source) : this ()
		{
			listView.ItemsSource = source;
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

		async void OnPageAppearing (object sender, EventArgs e)
		{
			SearchPosition = Persist.Instance.GpsPosition;
			await FilterList ();
			StartTimerIfNoGPS ();
		}

		async public void DoPickLocation (object s, EventArgs e)
		{
			if (AreaBox.IsVisible) {
				AreaBox.IsVisible = false;
				LocationButton.ButtonText = "Change";
				SearchPosition = Persist.Instance.GpsPosition;
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
						SearchPosition = new Position (53.1, -1.5);
						Console.WriteLine ("AddMenu.SearchHere: DEBUG_ON_SIMULATOR");
					} else {
						SearchPosition = positions.First ();
					}
				}
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
				FilterCuisineKind = "";
				CuisineButton.Text = "All Types of Food";
				await FilterList ();
			} 
		}

		public async void  DoFilterMine (object s, EventArgs e)
		{
			MainFilter = FilterKind.Mine;
			await FilterList ();
			filters.IsVisible = currentPlaces.Count () == 0;
		}

		public async void  DoFilterAll (object s, EventArgs e)
		{
			MainFilter = FilterKind.All;
			await FilterList ();
			filters.IsVisible = currentPlaces.Count () == 0;
		}

		public async void  DoFilterWish (object s, EventArgs e)
		{
			MainFilter = FilterKind.Wishlist;
			await FilterList ();
			filters.IsVisible = currentPlaces.Count () == 0;
		}

		void ClearFilter (object s, EventArgs e)
		{ 
			FilterSearchBox.Text = "";
			FilterAreaSearchBox.Text = "";
			filters.IsVisible = false;
			MainFilter = FilterKind.All;
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
				currentPlaces = currentPlaces.Where (
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
			data.updatePlaces (SearchPosition);
			try {
				String text = FilterSearchBox.Text.ToLower ();
				switch (MainFilter) {
				case FilterKind.Go:
					// places to go - from cuisine string constructorWill
					ResetCuisinePicker ();
					currentPlaces = (
					    from p in data.Places
					    where
					        p.vote != "-1" &&
					        p.category == FilterCuisineKind
					    select p).ToList ();
					break;
				case FilterKind.Mine:
					ResetCuisinePicker ();
					currentPlaces = (
					    from p in data.Places
					    where p.iVoted == true && (
					            p.place_name.ToLower ().Contains (text) ||
					            p.CategoryLowerCase.Contains (text))
					    select p).ToList ();
					break;
				case FilterKind.All:
					ResetCuisinePicker ();
					currentPlaces = (from p in data.Places
					                 where
					                     p.place_name.ToLower ().Contains (text) ||
					                     p.CategoryLowerCase.Contains (text)
					                 select p).ToList ();
					break;
				case FilterKind.Cuisine:
					if (FilterCuisineKind != null && FilterCuisineKind.Length > 0)
						currentPlaces = (
						    from p in data.Places
						    where p.category == FilterCuisineKind && (
						            p.place_name.ToLower ().Contains (text) ||
						            p.CategoryLowerCase.Contains (text))
						    select p).ToList ();
					else
						goto case FilterKind.All;
					break;
				case FilterKind.Wishlist:
					currentPlaces = (
					    from p in data.Places
					    where p.untried == true && (
					            p.place_name.ToLower ().Contains (text) ||
					            p.CategoryLowerCase.Contains (text))
					    select p).ToList ();
					break;
				}
				if (FilterAreaSearchBox.Text.Length > 0)
					await NarrowGeoSearch ();
				lock (Persist.Instance.Lock) {
					data.SortPlaces (currentPlaces);
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DoSearch: Exception {0}", ex);
			}
			Device.BeginInvokeOnMainThread (() => {
				SetList (currentPlaces);
				FilterSearchBox.Unfocus ();
				Spinner.IsRunning = false;
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


