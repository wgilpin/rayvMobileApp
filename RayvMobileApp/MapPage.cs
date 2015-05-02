using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using System.Diagnostics;

namespace RayvMobileApp
{
	public class MapPage : ContentPage
	{
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

		private void SetupMapList (Position centre)
		{
			Console.WriteLine ("SetupMapList");
			Spinner.IsRunning = true;
//			int debugCount = 0;
//			Persist.Instance.DisplayList.Clear ();
//			foreach (Place p in Persist.Instance.Places) {
//				MapPlace mp = new MapPlace (p, centre);
//				Persist.Instance.DisplayList.Add (mp);
//				debugCount++;
//			}
//			Console.WriteLine ("SetupMapList: Added {0} places", debugCount);
//			debugCount = 0;

			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				if (Persist.Instance.DisplayPosition != centre) {
					Console.WriteLine ("MapPage set DisplayPosition mtp {0},{1}", centre.Latitude, centre.Longitude);
					Persist.Instance.DisplayPosition = centre;
					foreach (var p in Persist.Instance.Places)
						p.CalculateDistanceFromPlace (centre);
				}
				Console.WriteLine ("SetupMapList SORT");
				Persist.Instance.Places.Sort ();
				Device.BeginInvokeOnMainThread (() => {
					foreach (Pin p in map.Pins)
						p.Clicked -= PinClick;
					
					PinList.Clear ();
					
					for (int i = 0; i < Persist.Instance.Places.Count; i++) {
						if (i > 9)
							break;
						Place p = Persist.Instance.Places [i];
						Pin pin;
						if (i >= map.Pins.Count - 1) {
							pin = new Pin {
								Type = PinType.SearchResult,
							};
							pin.Clicked += PinClick;
							pin.Label = p.place_name;
							map.Pins.Add (pin);
							pin.Position = p.GetPosition ();
							pin.Address = p.address;
						} else {
							map.Pins [i].Label = p.place_name;
							map.Pins [i].Position = p.GetPosition ();
							map.Pins [i].Address = p.address;
						}
						PinList [p.key] = map.Pins [i];
						Console.WriteLine ("SetupMapList: Pin for  {0}", p.place_name);
						
						//				debugCount++;
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

		public MapPage ()
		{
			Analytics.TrackPage ("MapPage");
			Title = "Map";
			map = new Map (
				MapSpan.FromCenterAndRadius (
					Persist.Instance.DisplayPosition, Distance.FromMiles (0.3))) {
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			map.IsShowingUser = true;
			PinList = new Dictionary<string, Pin> ();


			SearchHereBtn = new RayvButton (" Search Here ");
			SearchHereBtn.Clicked += DoSearch;

			Image GoToHomeBtn = new Image { Source = "centre-button.png", };
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
			ToolbarItems.Add (ListBtn);
			SetupMapList (Persist.Instance.DisplayPosition);
		}

		public MapPage (Place place) : this ()
		{
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

		#endregion
	}
}

