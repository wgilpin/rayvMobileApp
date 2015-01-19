using System;
using Xamarin.Forms;
using CoreLocation;
using Xamarin.Forms.Maps;


namespace RayvMobileApp.iOS
{
	public class AddMapPage : ContentPage
	{
		#region Private Fields


		Map map;

		#endregion

		#region Events

		public void DoAdd (object sender, EventArgs e)
		{
			Console.WriteLine ("AddMapPage.DoAdd");
			MapSpan span = map.VisibleRegion;
			this.Navigation.PushAsync (new EditPage (span.Center));
		}

		#endregion

		#region Constructors

		public AddMapPage ()
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

			Entry addressBox = new Entry {
				IsVisible = false,
			};
//			var stack = new StackLayout { Spacing = 0 };
			BoxView vertical = new BoxView {
				Color = Color.Blue,
				WidthRequest = 1,
				HeightRequest = 1000,
			};
			BoxView horizontal = new BoxView {
				Color = Color.Blue,
				WidthRequest = 1000,
				HeightRequest = 1,
			};
			RayvButton addHereBtn = new RayvButton {
				Text = "  Add Here ",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			addHereBtn.Clicked += DoAdd;
			AbsoluteLayout mapLayout = new AbsoluteLayout {
				BackgroundColor = Color.Blue.WithLuminosity (0.9),
//				VerticalOptions = LayoutOptions.FillAndExpand,

			};
			mapLayout.Children.Add (map);
			AbsoluteLayout.SetLayoutFlags (map,
				AbsoluteLayoutFlags.SizeProportional);
			AbsoluteLayout.SetLayoutBounds (map,
				new Rectangle (0, 0, 1, 1));
			mapLayout.Children.Add (vertical);
			AbsoluteLayout.SetLayoutFlags (vertical,
				AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (vertical,
				new Rectangle (0.5, 0, 2, AbsoluteLayout.AutoSize));
			mapLayout.Children.Add (horizontal);
			AbsoluteLayout.SetLayoutFlags (horizontal,
				AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (horizontal,
				new Rectangle (0, 0.5, AbsoluteLayout.AutoSize, 2));

			mapLayout.Children.Add (addHereBtn);
			AbsoluteLayout.SetLayoutFlags (addHereBtn,
				AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (addHereBtn,
				new Rectangle (0.5, 1.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			mapLayout.Children.Add (addressBox);
			AbsoluteLayout.SetLayoutFlags (addressBox,
				AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (addressBox,
				new Rectangle (0.5, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			Console.WriteLine (String.Format ("vert {0},{1}:{2},{3}", vertical.X, vertical.Y, vertical.Width, vertical.Height));

			Content = mapLayout;


		}

		#endregion

		#region Public Methods



		#endregion
	}
}
