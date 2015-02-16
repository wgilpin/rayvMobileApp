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
	}

	public class ListPage : ContentPage
	{
		#region Fields

		static ListView listView;
		static FilterKind MainFilter = FilterKind.All;
		static String FilterCuisineKind;
		Picker FilterCuisinePicker;
		List<Place> currentPlaces;
		EntryWithButton FilterSearchBox;
		EntryWithButton FilterAreaSearchBox;
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

			var FilterMineBtn = new ButtonWide ("My Places");
			FilterMineBtn.Clicked += DoFilterMine;

			var FilterAllBtn = new ButtonWide ("All Places");
			FilterAllBtn.Clicked += DoFilterAll;

			FilterCuisinePicker = new Picker {
				Title = "Filter by Cuisine",
			};
			foreach (string cat in Persist.Instance.Categories) {
				FilterCuisinePicker.Items.Add (cat);
			}
			FilterCuisinePicker.SelectedIndex = FilterCuisinePicker.Items.IndexOf (FilterCuisineKind);
			FilterCuisinePicker.SelectedIndexChanged += UpdateCuisine;
				

			var FiltersCloseBtn = new RayvButton ("Clear Filter") {
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			FiltersCloseBtn.Clicked += ClearFilter;

			var FilterNewBtn = new ButtonWide ("New Places");
			FilterNewBtn.Clicked += (object sender, EventArgs e) => {
				DisplayAlert ("Not Implemented", "New Places is not done yet", "OK");
			};

			var FilterWishBtn = new ButtonWide ("Wishlist");
			FilterWishBtn.Clicked += DoFilterWish;

			FilterSearchBox = new EntryWithButton {
				Placeholder = "Search for text",
				Source = "06-magnify@2x.png",
				OnClick = DoTextSearch,
				Text = "",
			};

			FilterAreaSearchBox = new EntryWithButton {
				Placeholder = "Search in an Area",
				Source = "06-magnify@2x.png",
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
			filters.Children.Add (FilterCuisinePicker, 0, 2, 3, 4);
			filters.Children.Add (FilterAreaSearchBox, 0, 2, 4, 5);
			filters.Children.Add (FiltersCloseBtn, 0, 2, 5, 6);



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
//					new StackLayout {
//						VerticalOptions = LayoutOptions.End,
//						Children = {
//							tools,
//						},
//					},
				}
			};

			grid.Children.Add (inner, 0, 0);
			grid.Children.Add (tools, 0, 1);
			filters.IsVisible = false;
			this.Content = grid;
//			new StackLayout {;
//
//				Children = {
//					inner
//				}
//			};

			ToolbarItems.Add (new ToolbarItem {
				Text = "Map",
				Icon = "icon-map.png",
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

			ToolbarItems.Add (new ToolbarItem {
				Text = "Filter",
				Icon = "filter.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("ListPage Toolbar Filter");
					filters.IsVisible = !filters.IsVisible;
				})
			});

			FilterList ();
			this.Appearing += (object sender, EventArgs e) => {
				//SetList (Persist.Instance.Places);
				FilterList ();
			};
			StartTimerIfNoGPS ();

			System.Diagnostics.Debug.WriteLine ("ListPage() Done");
		}



		/**
		 * Constructor when a list of Places is supplied
		 */
		public ListPage (List<Place> source) : this ()
		{
			listView.ItemsSource = source;
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

		public async void  DoFilterMine (object sim, EventArgs e)
		{
			MainFilter = FilterKind.Mine;
			await FilterList ();
			filters.IsVisible = currentPlaces.Count () == 0;
		}

		public async void  DoFilterAll (object sim, EventArgs e)
		{
			MainFilter = FilterKind.All;
			FilterList ();
			filters.IsVisible = currentPlaces.Count () == 0;
		}

		public async void  DoFilterWish (object sim, EventArgs e)
		{
			MainFilter = FilterKind.Wishlist;
			FilterList ();
			filters.IsVisible = currentPlaces.Count () == 0;
		}

		void ClearFilter (object s, EventArgs e)
		{ 
			FilterCuisinePicker.SelectedIndex = -1;
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

		void UpdateCuisine (Object sender, System.EventArgs e)
		{
			MainFilter = FilterKind.Cuisine;
			if (FilterCuisinePicker.SelectedIndex >= 0)
				FilterCuisineKind = FilterCuisinePicker.Items [FilterCuisinePicker.SelectedIndex];
			else
				FilterCuisineKind = null;
			FilterList ();
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
			FilterCuisinePicker.SelectedIndexChanged -= UpdateCuisine;
			FilterCuisinePicker.SelectedIndex = -1;
			FilterCuisinePicker.SelectedIndexChanged += UpdateCuisine;
		}



		async Task FilterList ()
		{
			Persist data = Persist.Instance;
			try {
				String text = FilterSearchBox.Text.ToLower ();
				switch (MainFilter) {
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
			SetList (currentPlaces);
			FilterSearchBox.Unfocus ();
		}


		public static void Setup (Page caller)
		{
			Console.WriteLine ("ListPage.Setup");
			// fire off a thread to get the data
			System.Threading.ThreadPool.QueueUserWorkItem (delegate {
				Persist.Instance.GetUserData (caller);
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


