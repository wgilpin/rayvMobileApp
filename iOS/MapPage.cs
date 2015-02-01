using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{
	public class MapPlace : IComparable<MapPlace>
	{
		Place _place;
		Double _distanceFromMapCentre;

		public Place place { 
			get { 
				return _place;
			}
			set {
				_place = value;
			}

		}

		void SetDistance (Position from)
		{
			//based on 1/60 rule
			//delta lat. Degrees * 69 (miles)
			double d_lat = (from.Latitude - place.lat) * 69;
			//cos(lat) approx by 1/60
			double cos_lat = Math.Min (1, (90 - place.lat) / 60);
			//delta lng = degrees * cos(lat) *69 miles
			double d_lng = (from.Longitude - place.lng) * 69 * cos_lat;
			_distanceFromMapCentre = Math.Sqrt (d_lat * d_lat + d_lng * d_lng);
		}

		public MapPlace (Place p, Position mapCenter)
		{
			place = p;
			SetDistance (mapCenter);
		}

		public int CompareTo (MapPlace other)
		{
			return Math.Sign (_distanceFromMapCentre - other._distanceFromMapCentre);
		}
	}

	public class MapPage : ContentPage
	{
		Button SearchHereBtn;
		Map map;
		List<MapPlace> mapPlaces;

		private void PinClick (object send, EventArgs e)
		{
			var actionButton1 = new Button { Text = "ActionSheet Simple" };
			actionButton1.Clicked += async (sender, ev) => {
				var action = await DisplayActionSheet ("ActionSheet: Send to?", "Cancel", null, "Email", "Twitter", "Facebook");
				Debug.WriteLine ("Action: " + action); // writes the selected button label to the console
			};
		}

		private void SetupMapList (Position centre)
		{
			Console.WriteLine ("SetupMapList");
			map.Pins.Clear ();
			int debugCount = 0;
			mapPlaces.Clear ();
			foreach (Place p in Persist.Instance.Places) {
				MapPlace mp = new MapPlace (p, centre);
				mapPlaces.Add (mp);
				debugCount++;
			}
			Console.WriteLine ("SetupMapList: Added {0} places", debugCount);
			debugCount = 0;
			mapPlaces.Sort ();
			for (int i = 0; i < mapPlaces.Count; i++) {
				if (i > 9)
					break;
				Place p = mapPlaces [i].place;
				Pin pin = new Pin {
					Type = PinType.SearchResult,
					Position = p.GetPosition (),
					Label = p.place_name,
					Address = p.address,
				};
				pin.Clicked += PinClick;
					
				map.Pins.Add (pin);
				Console.WriteLine ("SetupMapList: Pin for  {0}", p.place_name);

				debugCount++;
			}
			Console.WriteLine ("SetupMapList: Pinned {0} places, map has {1}", debugCount, map.Pins.Count);

		}

		private void DoSearch (object sender, EventArgs e)
		{
			SetupMapList (map.VisibleRegion.Center);
		}

		public MapPage ()
		{
			Xamarin.FormsMaps.Init ();
			map = new Map (
				MapSpan.FromCenterAndRadius (
					Persist.Instance.GpsPosition, Distance.FromMiles (0.3))) {
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			map.IsShowingUser = true;
			mapPlaces = new List<MapPlace> ();

			//			var stack = new StackLayout { Spacing = 0 };

			SearchHereBtn = new RayvButton (" Search Here ");
			SearchHereBtn.Clicked += DoSearch;

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
			Content = mapLayout;
			SetupMapList (Persist.Instance.GpsPosition);
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
	}
}

