using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{
	public class MapPage : ContentPage
	{
		Button SearchHereBtn;
		Map map;


		private void PinClick (object send, EventArgs e)
		{
			Console.WriteLine ("PIN CLICKED");
			DisplayAlert ("Success!", "Inform Will Immediately. This is working", "Yay!");
			var actionButton1 = new Button { Text = "ActionSheet Simple" };
			actionButton1.Clicked += async (sender, ev) => {
				var action = await DisplayActionSheet ("ActionSheet: Send to?", "Cancel", null, "Email", "Twitter", "Facebook");
				Debug.WriteLine ("Action: " + action); // writes the selected button label to the console
			};
		}

		private void SetupMapList (Position centre)
		{
			Console.WriteLine ("SetupMapList");
//			map.Pins.Clear ();
//			int debugCount = 0;
//			Persist.Instance.DisplayList.Clear ();
//			foreach (Place p in Persist.Instance.Places) {
//				MapPlace mp = new MapPlace (p, centre);
//				Persist.Instance.DisplayList.Add (mp);
//				debugCount++;
//			}
//			Console.WriteLine ("SetupMapList: Added {0} places", debugCount);
//			debugCount = 0;
			if (Persist.Instance.DisplayPosition != centre) {
				Console.WriteLine ("MapPage set DisplayPosition mtp {0},{1}", centre.Latitude, centre.Longitude);
				Persist.Instance.DisplayPosition = centre;
				foreach (var p in Persist.Instance.DisplayList)
					p.CalculateDistanceFromPlace (centre);
			}
			;
			Persist.Instance.DisplayList.Sort ();
			for (int i = 0; i < Persist.Instance.DisplayList.Count; i++) {
				if (i > 9)
					break;
				Place p = Persist.Instance.DisplayList [i];
				Pin pin = new Pin {
					Type = PinType.SearchResult,
					Position = p.GetPosition (),
					Label = p.place_name,
					Address = p.address,
				};
				pin.Clicked += PinClick;

				map.Pins.Add (pin);
				Console.WriteLine ("SetupMapList: Pin for  {0}", p.place_name);

//				debugCount++;
			}
//			Console.WriteLine ("SetupMapList: Pinned {0} places, map has {1}", debugCount, map.Pins.Count);

		}

		private void DoSearch (object sender, EventArgs e)
		{
			SetupMapList (map.VisibleRegion.Center);
		}

		#region constructors

		public MapPage ()
		{
			Title = "Map";
			Xamarin.FormsMaps.Init ();
			map = new Map (
				MapSpan.FromCenterAndRadius (
					Persist.Instance.DisplayPosition, Distance.FromMiles (0.3))) {
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			map.IsShowingUser = true;

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

			mapLayout.Children.Add (GoToHomeBtn);
			AbsoluteLayout.SetLayoutFlags (GoToHomeBtn,
				AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (GoToHomeBtn,
				new Rectangle (1.0, 1.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
			Content = mapLayout;
			SetupMapList (Persist.Instance.DisplayPosition);
		}

		public MapPage (Place place) : this ()
		{
			//
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

