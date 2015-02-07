using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{

	public class AddResultsPage : ContentPage
	{
		#region Strings

		const String ADD_FROM_MAP = "Add From Map";

		#endregion

		#region Fields

		static ListView listView;
		bool FirstTime;

		#endregion

		#region Events

		void DoEdit (object sender, SelectedItemChangedEventArgs e)
		{
			Place p = (Place)e.SelectedItem;
			Debug.WriteLine ("AddResultsPage.DoEdit Push EditPage");
			this.Navigation.PushAsync (new EditPage (p));
		}

		#endregion

		#region Constructors

		public AddResultsPage ()
		{
			Console.WriteLine ("ListView()");
			FirstTime = true;
			this.Title = "List";
			this.Icon = "bars-black.png";

			// Define template for displaying each item.
			// (Argument of DataTemplate constructor is called for 
			//      each item; it must return a Cell derivative.)

			listView = new PlacesListView (false);
			listView.ItemsSource = Persist.Instance.Places;
			listView.ItemSelected += DoEdit;


			RayvButton addFromMapBtn = new RayvButton {
				Text = ADD_FROM_MAP,
			};
			addFromMapBtn.Clicked += async (object sender, EventArgs e) => {
				await Navigation.PushAsync (new AddMapPage ());
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
			this.Appearing += (object sender, EventArgs e) => {
				if (!FirstTime)
					this.Navigation.PopToRootAsync ();
				else
					FirstTime = false;
			};
			System.Diagnostics.Debug.WriteLine ("fillListPage");
		}

		/**
		 * Constructor when a list of Places is supplied
		 */
		public AddResultsPage (List<Place> source) : this ()
		{
			listView.ItemsSource = source;
		}

		#endregion

		#region Properties

		public static IEnumerable ItemsSource {
			set {
				listView.ItemsSource = value;
			}
		}

		#endregion

	}

}


