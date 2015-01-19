using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections;

namespace RayvMobileApp.iOS
{

	public class AddResultsPage : ContentPage
	{
		static ListView listView;

		void DoEdit (object sender, SelectedItemChangedEventArgs e)
		{
			Place p = (Place)e.SelectedItem;
			this.Navigation.PushAsync (new EditPage (p));
		}

		public AddResultsPage ()
		{
			Console.WriteLine ("ListView()");
			this.Title = "List";
			this.Icon = "bars-black.png";

			// Define template for displaying each item.
			// (Argument of DataTemplate constructor is called for 
			//      each item; it must return a Cell derivative.)

			listView = new PlacesListView {
				ItemsSource = Persist.Instance.Places,
			};
			listView.ItemSelected += DoEdit;


			RayvButton addFromMapBtn = new RayvButton {
				Text = "Find on Map",
			};

			StackLayout tools = new toolbar (this);
			StackLayout inner = new StackLayout {
				Children = {
					addFromMapBtn,
					listView,
					tools
				}
			};

			this.Content = new StackLayout {
				Children = {
					inner
				}
			};
			System.Diagnostics.Debug.WriteLine ("fillListPage");
		}

		/**
		 * Constructor when a list of Places is supplied
		 */
		public AddResultsPage (List<Place> source) : this ()
		{
			listView.ItemsSource = source;
			listView.ItemSelected += DoEdit;

		}

		private static NavigationPage _instance;

		public static NavigationPage Instance {
			get {
				if (_instance == null) {
					_instance = new NavigationPage (new AddResultsPage ());
				}
				return _instance;
			}
		}

		public static IEnumerable ItemsSource {
			set {
				listView.ItemsSource = value;
			}
		}



	}

}


