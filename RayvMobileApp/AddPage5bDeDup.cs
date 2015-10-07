using System;

using Xamarin.Forms;
using System.Diagnostics;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xamarin;

namespace RayvMobileApp
{
	

	public class AddPage5bDeDup : ContentPage
	{
		public event EventHandler Confirmed;
		public event EventHandler Cancelled;

		protected virtual void OnConfirm (EventArgs e)
		{
			if (Confirmed != null)
				Confirmed (this, e);
		}

		protected virtual void OnCancel (EventArgs e)
		{
			if (Cancelled != null)
				Cancelled (this, e);
		}

		#region Fields

		static PlacesListView listView;
		//		bool FirstTime;
		Label NothingFound;
		StackLayout ProposedDetails;
		string PlaceName;
		string Address;
		Position LatLng;
		ActivityIndicator Spinner;
		Place addingPlace;

		#endregion

		#region Events

		void ShowPlaceOnDetailPage (object sender, PlaceSavedEventArgs e) =>
		    this.Navigation.PushModalAsync (new RayvNav (new DetailPage (e.EditedPlace, true)));

		void BackToRoot (object sender, EventArgs e) => this.Navigation.PopToRootAsync ();


		void DoEditFromList (object sender, ItemTappedEventArgs e)
		{
			addingPlace = (Place)e.Item;
			Debug.WriteLine ("AddPage5bDeDup.DoEdit Push EditPage");
			var editor = new PlaceEditor (addingPlace);
			editor.Cancelled += (s, ev) => Navigation.PopModalAsync ();
			editor.Saved += (s, ev) => Navigation.PopModalAsync ();
			Navigation.PushModalAsync (editor);
		}

		void DoConfirmedManualDetails (object sender, EventArgs e)
		{
			Debug.WriteLine ("AddPage5bDeDup.DoConfirmed Push EditPage");
			addingPlace = new Place {
				lat = LatLng.Latitude,
				lng = LatLng.Longitude,
				place_name = PlaceName,
			};
			var editor = new PlaceEditor (addingPlace, false);
			editor.Saved += (s, ev) => {
				Navigation.PopModalAsync ();
			};
			editor.Cancelled += (s, ev) => Navigation.PopModalAsync ();
			Navigation.PushModalAsync (editor);
		}

		#endregion



		void DoSearch ()
		{
			Console.WriteLine ("AddPage5bDeDup.DoSearch: Activity");
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Console.WriteLine ("AddPage5bDeDup.DoSearch: Thread");
				Dictionary<string, string> parameters = new Dictionary<string, string> ();
				parameters ["lat"] = LatLng.Latitude.ToString ();
				parameters ["lng"] = LatLng.Longitude.ToString ();
				if (!string.IsNullOrWhiteSpace (Address)) {
					parameters ["addr"] = Address;
				}
				if (!string.IsNullOrWhiteSpace (PlaceName)) {
					parameters ["place_name"] = PlaceName;
				}
				parameters ["near_me"] = "1";
				try {
					string result = Persist.Instance.GetWebConnection ().get ("/getAddresses_ajax", parameters).Content;
					JObject obj = JObject.Parse (result);
					List<Place> points = JsonConvert.DeserializeObject<List<Place>> (obj.SelectToken ("local.points").ToString ());
					foreach (Place point in points) {
						point.CalculateDistanceFromPlace ();
					}
					Console.WriteLine ("DoSearch SORT");

					points.Sort ();
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("AddPage5bDeDup.DoSearch: MainThread"); 
						Spinner.IsRunning = false;
						Console.WriteLine ("AddPage5bDeDup.DoSearch: Activity Over.");
						listView.DisplayedList.ItemsSource = points;
						listView.IsVisible = points.Count > 0;
						NothingFound.IsVisible = points.Count == 0;
					});
				} catch (Exception e) {
					Insights.Report (e);
					restConnection.LogErrorToServer ("AddPage5bDeDup.DoSearch: Exception {0}", e);
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("AddPage5bDeDup.DoSearch: MainThread Exception");
						Spinner.IsRunning = false;
						DisplayAlert ("Oops", "Unable to search. Network problems?", "Close");
					});
				}
			})).Start ();
		}

		public AddPage5bDeDup (string placeName, string address, Position posn)
		{
			Analytics.TrackPage ("AddPage5bDeDup");
			Console.WriteLine ("AddPage5bDeDup");
			this.Title = "Confirm Address";
			PlaceName = placeName;
			Address = address;
			LatLng = posn;

			Spinner = new ActivityIndicator {
				IsRunning = true,
				Color = Color.Red,
			};

			ProposedDetails = new StackLayout {
				Padding = 5,
				Children = {
					new Label {
						Text = placeName,
						FontSize = settings.FontSizeLabelLarge,
						FontAttributes = FontAttributes.Bold,
					},
					new Label {
						Text = address,
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
						FontAttributes = FontAttributes.Italic,
					},

					new RayvButton {
						Text = "Confirm",
						OnClick = DoConfirmedManualDetails,
					},
				}
			};
					
			listView = new PlacesListView (false);
			listView.OnItemTapped = DoEditFromList;


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

			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			StackLayout inner = new StackLayout {
				Spacing = 5,
				Children = {
					ProposedDetails,
					new LabelWide ("or pick one ...") { FontAttributes = FontAttributes.Bold },
					Spinner,
					listView,
					NothingFound,
				}
			};

			StackLayout tools = new BottomToolbar (this, "add");
			grid.Children.Add (inner, 0, 0);
			grid.Children.Add (tools, 0, 1);
			this.Content = grid;

			this.Appearing += (sender, e) => {
				DoSearch ();
			};
		}
	}
}


