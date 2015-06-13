using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using System.Diagnostics;

namespace RayvMobileApp
{
	public class MapPage : ContentPage
	{
		public event EventHandler Changed;

		Button SearchHereBtn;
		Map map;
		ToolbarItem ListBtn;
		ActivityIndicator Spinner;

		private Dictionary<string, Pin> PinList;


		private void PinClick (object send, EventArgs e)
		{
			Console.WriteLine ("PIN CLICKED");
			string key = "";
			foreach (KeyValuePair<string, Pin> kvp in PinList) {
				if (kvp.Value == send)
					key = kvp.Key;
			}
			if (string.IsNullOrEmpty (key)) {
				Console.WriteLine ("Pin not found");
				return;
			}
			Place p = Persist.Instance.GetPlace (key);
			if (p != null)
				this.Navigation.PushAsync (new DetailPage (p, showMapBtn: false));
		}

		private void SetupMapList (Position center)
		{
			Console.WriteLine ("SetupMapList");
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Console.WriteLine ("MapPage set DisplayPosition mtp {0},{1}", center.Latitude, center.Longitude);
				if (center != Persist.Instance.DisplayPosition)
					Changed?.Invoke (this, null);
				Persist.Instance.DisplayPosition = center;
				foreach (var p in Persist.Instance.DisplayList)
					p.distance_for_search = p.distance_from (center);
				Persist.Instance.DisplayList.Sort ((a, b) => 
	                                   a.distance_for_search.CompareTo (b.distance_for_search));
				Console.WriteLine ("SetupMapList SORT");
				Device.BeginInvokeOnMainThread (() => {
					map.Pins.Clear ();
					PinList.Clear ();
					for (int i = 0; i < Math.Min (9, Persist.Instance.DisplayList.Count); i++) {
						Place p = Persist.Instance.DisplayList [i];
						Pin pin = new Pin {
							Type = PinType.SearchResult,
							Label = p.place_name,
							Position = p.GetPosition (),
							Address = p.address,
						};
						pin.Clicked += PinClick;
						map.Pins.Add (pin);
						PinList.Add (p.key, pin);
						Console.WriteLine ("SetupMapList: Pin for  {0}", p.place_name);
					}
					Spinner.IsRunning = false;
				});
			})).Start ();
//			Console.WriteLine ("SetupMapList: Pinned {0} places, map has {1}", debugCount, map.Pins.Count);

		}

		private void DoSearch (object sender, EventArgs e)
		{
			ListBtn.Text = "View as List";
			SetupMapList (map.VisibleRegion.Center);
		}

		#region constructors

		public MapPage (Place place = null)
		{
			Analytics.TrackPage ("MapPage");
			Title = "Map";
			map = new Map (
				MapSpan.FromCenterAndRadius (
					Persist.Instance.DisplayPosition, Distance.FromMiles (0.3))) {
				IsShowingUser = false,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
//			map.IsShowingUser = true;

			PinList = new Dictionary<string, Pin> ();

			SearchHereBtn = new RayvButton (" Search Here ");
			SearchHereBtn.Clicked += DoSearch;

			Image GoToHomeBtn = new Image { Source = settings.DevicifyFilename ("centre-button.png"), };
			var clickHome = new TapGestureRecognizer ();
			clickHome.Tapped += (s, e) => {
				map.MoveToRegion (MapSpan.FromCenterAndRadius (
					Persist.Instance.GpsPosition, Distance.FromMiles (0.3)));
				DoSearch (s, e);
			};
			GoToHomeBtn.GestureRecognizers.Add (clickHome);

			Spinner = new ActivityIndicator (){ Color = Color.Red };

			AbsoluteLayout mapLayout = new AbsoluteLayout {
				BackgroundColor = Color.Blue.WithLuminosity (0.9),
				//				VerticalOptions = LayoutOptions.FillAndExpand,

			};
			mapLayout.Children.Add (map);
			AbsoluteLayout.SetLayoutFlags (map,
			                               AbsoluteLayoutFlags.SizeProportional);
			AbsoluteLayout.SetLayoutBounds (map,
			                                new Rectangle (0, 0, 1, 1));

			mapLayout.Children.Add (SearchHereBtn);
			AbsoluteLayout.SetLayoutFlags (SearchHereBtn,
			                               AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (SearchHereBtn,
			                                new Rectangle (0.5, 1.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			mapLayout.Children.Add (Spinner);
			AbsoluteLayout.SetLayoutFlags (Spinner,
			                               AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (Spinner,
			                                new Rectangle (0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			mapLayout.Children.Add (GoToHomeBtn);
			AbsoluteLayout.SetLayoutFlags (GoToHomeBtn,
			                               AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (GoToHomeBtn,
			                                new Rectangle (1.0, 1.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
			Content = mapLayout;
			ListBtn = new ToolbarItem {
				Text = "",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					this.Navigation.PushAsync (new MapListPage ());
				}),
			};
//			ToolbarItems.Add (ListBtn);
			if (place == null)
				SetupMapList (Persist.Instance.DisplayPosition);
			else {
				Console.WriteLine ($"MapPage for {place.place_name}");
				map.MoveToRegion (MapSpan.FromCenterAndRadius (
					place.GetPosition (), Distance.FromMiles (0.3)));
				var pin = new Pin {
					Type = PinType.Place,
					Position = place.GetPosition (),
					Label = place.place_name,
					Address = place.address,
				};
				map.Pins.Clear ();
				map.Pins.Add (pin);
			}
			Appearing += ( se, ev) => {
				map.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
					if (e.PropertyName == "VisibleRegion") {
						DoSearch (sender, null);
						Console.WriteLine ("MapPage Dragged");
					}
				};
			};
		}

		#endregion
	}
}

