using System;
using Xamarin.Forms;

//using CoreLocation;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using Xamarin;
using System.ComponentModel;


namespace RayvMobileApp
{
	public class AddMapPage : ContentPage
	{
		#region Private Fields

		Map map;
		Label AddressBox;
		RayvButton addHereBtn;
		bool Saving;

		#endregion

		#region Events

		async public void GetAddressFromMap (bool flipButton = true)
		{
			var geo = new Geocoder ();
			try {
				IEnumerable<string> addresses = await geo.GetAddressesForPositionAsync (map.VisibleRegion.Center);
				string firstAddress = "";

				foreach (string addr in addresses) {
					if (firstAddress.Length == 0)
						firstAddress = addr;
				}
				AddressBox.Text = firstAddress;
			} catch (Exception) {
				AddressBox.Text = "";
			}

		}

		// If address has been found the bottom btn is SAVE else ADD HERE
		void CheckBottomButton (object sender, EventArgs e)
		{
			if (AddressBox.Text != null && AddressBox.Text.Length > 0) {
				addHereBtn.Text = "  Save  ";
				addHereBtn.Clicked -= DoGetAddress;
				addHereBtn.Clicked += DoSave;
			} else {
				addHereBtn.Text = " Add Here ";
				addHereBtn.Clicked -= DoSave;
				addHereBtn.Clicked += DoGetAddress;
			}
		}

		// Get the address from the current map position
		public void DoGetAddress (object sender, EventArgs e)
		{
			GetAddressFromMap ();
			CheckBottomButton (null, null);
		}

		// Save button clicked - set the data on th parent and return
		public void DoSave (object sender, EventArgs e)
		{
			if (Saving)
				return;
			Saving = true;
			Page parent = Navigation.NavigationStack [Navigation.NavigationStack.Count - 2];
			// stack[count - 1] is top (this page), stack[count-2] is parent
			if (parent is EditPage) {
				//go back there
				var editParent = (parent as EditPage);
				var lat = map.VisibleRegion.Center.Latitude;
				var lng = map.VisibleRegion.Center.Longitude;
				Console.WriteLine ("AddMapPage - return to edit {0} - {1}/{2}", AddressBox.Text, lat, lng);
				editParent.Address = AddressBox.Text;
				editParent.Lat = lat;
				editParent.Lng = lng;
				Navigation.PopAsync ();
			} else {
				Console.WriteLine ("AddMapPage.DoAdd Push EditPage");
				MapSpan span = map.VisibleRegion;
				this.Navigation.PushAsync (new EditPage (span.Center, AddressBox.Text));
			}
		}

		#endregion

		#region Constructors

		public AddMapPage ()
		{
			Console.WriteLine ("AddMapPage ctor");
			Analytics.TrackPage ("AddMapPage");
			Saving = false;
			ToolbarItems.Add (new ToolbarItem {
				Text = "Map",
				Icon = "01-refresh@2x.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => SetupPage ())
			});

			map = new Map (
				MapSpan.FromCenterAndRadius (
					Persist.Instance.GpsPosition, Distance.FromMiles (0.3))) {
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			map.IsShowingUser = true;

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
			addHereBtn = new RayvButton (" Add Here ");
			addHereBtn.Clicked += DoGetAddress;
			AddressBox = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				XAlign = TextAlignment.Start,
				LineBreakMode = LineBreakMode.TailTruncation,
				BackgroundColor = Color.White,
			};
			AbsoluteLayout mapLayout = new AbsoluteLayout {
				BackgroundColor = Color.Blue.WithLuminosity (0.9),
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

			mapLayout.Children.Add (AddressBox);
			AbsoluteLayout.SetLayoutFlags (AddressBox,
				AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds (AddressBox,
				new Rectangle (0.5, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			Console.WriteLine (String.Format ("vert {0},{1}:{2},{3}", vertical.X, vertical.Y, vertical.Width, vertical.Height));

			Content = mapLayout;
			Appearing += (object sender, EventArgs ev) => {
				map.PropertyChanged += (object s, PropertyChangedEventArgs e) => {
					if (e.PropertyName == "VisibleRegion") {
						Console.WriteLine ("map property changed");
						DoGetAddress (null, null);
					}
				};
			};
		}

		public AddMapPage (Position posn) : this ()
		{
			map.MoveToRegion (MapSpan.FromCenterAndRadius (
				posn, 
				Distance.FromMiles (0.3)
			));
			Appearing += async (sender, e) => {
				GetAddressFromMap ();
			};
		}

		#endregion

		#region Logic

		void SetupPage ()
		{
			AddressBox.Text = "";
			addHereBtn.Text = " Add Here ";
			addHereBtn.Clicked -= DoSave;
			addHereBtn.Clicked += DoGetAddress;
		}

		#endregion
	}
}
