﻿using System;
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

namespace RayvMobileApp.iOS
{

	enum FilterKind: short
	{
		Mine,
		All,
		Cuisine

	}

	public class ListPage : ContentPage
	{
		#region Fields

		static ListView listView;
		static FilterKind MainFilter = FilterKind.Mine;
		static String FilterCuisineKind;

		StackLayout filters;

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

			// Define template for displaying each item.
			// (Argument of DataTemplate constructor is called for 
			//      each item; it must return a Cell derivative.)

			var FilterMineBtn = new ButtonWide { Text = "Show Mine" };
			FilterMineBtn.Clicked += (sender, e) => { 
				MainFilter = FilterKind.Mine;
				FilterList ();
				filters.IsVisible = false;
			};
			var FilterAllBtn = new ButtonWide { Text = "Show All" };
			FilterAllBtn.Clicked += (sender, e) => { 
				MainFilter = FilterKind.All;
				FilterList ();
				filters.IsVisible = false;
			};
			Picker FilterCuisinePicker = new Picker {
				Title = "Filter by Cuisine",
			};
			foreach (string cat in Persist.Instance.Categories) {
				FilterCuisinePicker.Items.Add (cat);
			}
			FilterCuisinePicker.SelectedIndex = FilterCuisinePicker.Items.IndexOf (FilterCuisineKind);
			FilterCuisinePicker.SelectedIndexChanged += (sender, e) => {
				MainFilter = FilterKind.Cuisine;
				if (FilterCuisinePicker.SelectedIndex >= 0)
					FilterCuisineKind = FilterCuisinePicker.Items [FilterCuisinePicker.SelectedIndex];
				else
					FilterCuisineKind = null;
				FilterList ();
				filters.IsVisible = false;
			};
				

			var FiltersCloseBtn = new RayvButton () { Text = "Clear Filter" };
			FiltersCloseBtn.Clicked += (sender, e) => { 
				FilterCuisinePicker.SelectedIndex = -1;
			};
			filters = new StackLayout {
				Children = {
					FilterMineBtn,
					FilterAllBtn,
					FilterCuisinePicker,
					FiltersCloseBtn,
				}
			};
			listView = new PlacesListView {
				ItemsSource = Persist.Instance.Places,
			};
			listView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
				Debug.WriteLine ("Listpage.ItemTapped: Push DetailPage");
				this.Navigation.PushAsync (new DetailPage (e.Item as Place));
			};
			StackLayout tools = new toolbar (this);
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


			this.Appearing += (object sender, EventArgs e) => {
				SetList (Persist.Instance.Places);
			};
			StartTimerIfNoGPS ();

			System.Diagnostics.Debug.WriteLine ("fillListPage");
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

		#region Methods

		void FilterList ()
		{
			Persist data = Persist.Instance;
			lock (data.Lock) {
				switch (MainFilter) {
				case FilterKind.Mine:
					String me = Convert.ToString (data.MyId);
					data.Places.Clear ();
					foreach (Vote v in data.Votes) {
						if (v.voter == me)
							data.Places.Add (data.GetPlaceFromDb (v.key));
					}
					data.SortPlaces ();
				
					break;
				case FilterKind.All:
					data.LoadFromDb ();
					break;
				case FilterKind.Cuisine:
					data.LoadFromDb (FilterCuisineKind);
					break;
				}
				data.Places.Sort ();
			}
			SetList (data.Places);
		}

		static void GetFullData (Page caller)
		{
			restConnection webReq = restConnection.Instance;
			string server = Persist.Instance.GetConfig ("server");
			if (server.Length == 0) {
				Console.WriteLine ("ListPage.Setup: No server");
				return;
			} else {
				webReq.setBaseUrl (server);
				webReq.setCredentials (Persist.Instance.GetConfig ("username"), Persist.Instance.GetConfig ("pwd"), "");
				IRestResponse resp;
				Console.WriteLine ("GetFullData Login");
				resp = webReq.get ("/api/login", null);
				if (resp == null) {
					Console.WriteLine ("GetFullData: Response NULL");
					return;
				}
				if (resp.StatusCode == HttpStatusCode.Unauthorized) {
					//TODO: This doesn't work
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("GetFullData: Need to login - push LoginPage");
						caller.Navigation.PushModalAsync (new LoginPage ());
					});
					Console.WriteLine ("GetFullData: No login");
					return;
				}
				resp = webReq.get ("/getFullUserRecord");
				try {
					Console.WriteLine ("GetFullData: lock get full");
					Persist data = Persist.Instance;
					JObject obj = JObject.Parse (resp.Content);
					data.MyId = obj ["id"].Value<Int64> ();
					string placeStr = obj ["places"].ToString ();
					Dictionary<string,Place> place_list = JsonConvert.DeserializeObject<Dictionary<string, Place>> (placeStr);
					lock (data.Lock) {
						data.Places = place_list.Values.ToList ();
						data.Places.Sort ();
						
						data.Votes.Clear ();
						foreach (JObject fr in obj["friendsData"]) {
							string fr_id = fr ["id"].ToString ();
							string name = fr ["name"].ToString ();
							data.Friends [fr_id] = name;
							Dictionary<string, Vote> vote_list = fr ["votes"].ToObject<Dictionary<string, Vote>> ();
							foreach (KeyValuePair<string, Vote> v in vote_list) {
								v.Value.voter = fr_id;
							}
							data.Votes.AddRange (vote_list.Values);
						}
						//sort
						data.updatePlaces ();
					}
					Persist.Instance.DataIsLive = true;
					Console.WriteLine ("ListPage.Setup loaded");	
				} catch (Exception ex) {
					Console.WriteLine ("GetFullData Exception {0}", ex);
					System.Diagnostics.Debug.Write ("ListPage.Setup: ");
					System.Diagnostics.Debug.WriteLine (ex.Message);
				}
			}
		}

		public static void Setup (Page caller)
		{
			Console.WriteLine ("ListPage.Setup");
			// fire off a thread to get the data
			System.Threading.ThreadPool.QueueUserWorkItem (delegate {
				GetFullData (caller);
			}, null);

			System.Diagnostics.Debug.WriteLine ("ListPage.Setup out");
		}

		public void SetList (List<Place> list)
		{
			lock (Persist.Instance.Lock) {
				Console.WriteLine ("SetList");
				ItemsSource = null;
				list.Sort ();
				ItemsSource = list;
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
				SetList (Persist.Instance.Places);
			}
			_timer.Close ();
		}

		#endregion
	}

}


