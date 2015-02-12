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
		Entry FilterSearchText;
		Page Caller;

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
			this.Title = "List";
			this.Icon = "bars-black.png";

			var FilterMineBtn = new ButtonWide ("My Places");
			FilterMineBtn.Clicked += (sender, e) => { 
				MainFilter = FilterKind.Mine;
				FilterList ();
				filters.IsVisible = currentPlaces.Count () == 0;
			};
			var FilterAllBtn = new ButtonWide ("All Places");
			FilterAllBtn.Clicked += (sender, e) => { 
				MainFilter = FilterKind.All;
				FilterList ();
				filters.IsVisible = currentPlaces.Count () == 0;
			};
			FilterCuisinePicker = new Picker {
				Title = "Filter by Cuisine",
			};
			foreach (string cat in Persist.Instance.Categories) {
				FilterCuisinePicker.Items.Add (cat);
			}
			FilterCuisinePicker.SelectedIndex = FilterCuisinePicker.Items.IndexOf (FilterCuisineKind);
			FilterCuisinePicker.SelectedIndexChanged += UpdateCuisine;
				

			var FiltersCloseBtn = new RayvButton ("Clear Filter");
			FiltersCloseBtn.Clicked += ClearFilter;

			var FilterNewBtn = new ButtonWide ("New Places");
			FilterNewBtn.Clicked += (object sender, EventArgs e) => {
				DisplayAlert ("Not Implemented", "New Places is not done yet", "OK");
			};

			var FilterWishBtn = new ButtonWide ("Wishlist");
			FilterWishBtn.Clicked += (object sender, EventArgs e) => {
				MainFilter = FilterKind.Wishlist;
				FilterList ();
				filters.IsVisible = currentPlaces.Count () == 0;
			};

			FilterSearchText = new Entry {
				Placeholder = "Search for text",
				Text = "",
			};
			FilterSearchText.TextChanged += HandleTextChanged;

			filters = new Grid {
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
			filters.Children.Add (FilterSearchText, 0, 2, 0, 1);
			filters.Children.Add (FilterMineBtn, 0, 1, 1, 2);
			filters.Children.Add (FilterAllBtn, 1, 2, 1, 2);
			filters.Children.Add (FilterWishBtn, 0, 1, 2, 3);
			filters.Children.Add (FilterNewBtn, 1, 2, 2, 3);
			filters.Children.Add (FilterCuisinePicker, 0, 2, 3, 4);
			filters.Children.Add (FiltersCloseBtn, 0, 2, 4, 5);



			listView = new PlacesListView {
				//ItemsSource = Persist.Instance.Places,
			};
			listView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
				Debug.WriteLine ("Listpage.ItemTapped: Push DetailPage");
				this.Navigation.PushAsync (new DetailPage (e.Item as Place));
			};
			StackLayout tools = new BottomToolbar (this, "list");
			StackLayout inner = new StackLayout {
				Children = {
					filters,
					listView,
					tools
				}
			};
			filters.IsVisible = false;
			this.Content = new StackLayout {
				Children = {
					inner
				}
			};

			ToolbarItems.Add (new ToolbarItem {
				Text = "Map",
				Icon = "icon-map.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("ListPage Toolbar Map: Push MapPage");
					Navigation.PushAsync (new MapPage ());
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

		void HandleTextChanged (object sender, TextChangedEventArgs e)
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

		void ResetCuisinePicker ()
		{
			FilterCuisinePicker.SelectedIndexChanged -= UpdateCuisine;
			FilterCuisinePicker.SelectedIndex = -1;
			FilterCuisinePicker.SelectedIndexChanged += UpdateCuisine;
		}

		void ClearFilter (object s, EventArgs e)
		{ 
			FilterCuisinePicker.SelectedIndex = -1;
			FilterSearchText.Text = "";
			filters.IsVisible = currentPlaces.Count () == 0;
			MainFilter = FilterKind.All;
			FilterList ();
		}

		void FilterList ()
		{
			Persist data = Persist.Instance;
			lock (data.Lock) {
				try {
					String text = FilterSearchText.Text.ToLower ();
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
					data.SortPlaces (currentPlaces);
				} catch (Exception ex) {
					Insights.Report (ex);
					restConnection.LogErrorToServer ("DoSearch: Exception {0}", ex);
				}
			}
			SetList (currentPlaces);
			FilterSearchText.Unfocus ();
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
						Console.WriteLine ("SetList");
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


