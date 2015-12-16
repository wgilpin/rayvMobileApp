using System;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class AddPage4_Map : ContentPage
	{
		Map map;
		EntryWithButton AddressEd;
		EntryClearable NameEd;
		Button SaveBtn;
		Grid MainGrid;

		public event EventHandler Succeeded;
		public event EventHandler Failed;

		protected virtual void OnPositionSet (EventArgs e)
		{
			Navigation.PopAsync ();
			if (Succeeded != null)
				Succeeded (this, e);
		}

		protected virtual void OnPositionNotSet (EventArgs e)
		{
			Navigation.PopAsync ();
			if (Failed != null)
				Failed (this, e);
		}

		public AddPage4_Map (Position searchPosition, string place_name)
		{
			Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0);
			BackgroundColor = settings.BaseColor;
			map = new Map (
				MapSpan.FromCenterAndRadius (
					Persist.Instance.GpsPosition, Distance.FromMiles (0.3))) {
				IsShowingUser = true,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			map.IsShowingUser = true;
			Appearing += (se, ev) => {
				map.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
					if (e.PropertyName == "VisibleRegion") {
						GetAddressFromMap (sender, null);
						Console.WriteLine ("AddPage4_Map Dragged");
					}
				};
			};

			MainGrid = new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (30, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (30, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (500, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (40, GridUnitType.Absolute) },
				},
			};

			NameEd = new EntryClearable {
				Placeholder = "Place name",
				Text = place_name
			};
			AddressEd = new EntryWithButton ("Find address on map", "TB active search.png") { 
				OnClick = SetMapFromAddress,
			};
			AddressEd.TextEntry.Completed += SetMapFromAddress;

			SaveBtn = new ButtonWide { 
				BackgroundColor = ColorUtil.Darker (settings.BaseColor),
				TextColor = Color.White,
				HeightRequest = 40,
				FontAttributes = FontAttributes.Bold,
				FontSize = settings.FontSizeButtonLarge,
				Text = "Save",
				OnClick = DoAdd,
				IsVisible = false,
			};
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

			RelativeLayout relativeLayout = new RelativeLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
			};

			relativeLayout.Children.Add (
				view: map,
				xConstraint: Constraint.Constant (0),
				yConstraint: Constraint.Constant (0), 
				widthConstraint: Constraint.RelativeToParent ((parent) => {
					return parent.Width;
				}),
				heightConstraint: Constraint.RelativeToParent ((parent) => {
					return parent.Height;
				}));

			relativeLayout.Children.Add (
				view: vertical,
				xConstraint: Constraint.RelativeToParent ((parent) => {
					return parent.Width / 2;
				}), 
				yConstraint: Constraint.Constant (0),
				widthConstraint: Constraint.Constant (1),
				heightConstraint: Constraint.RelativeToParent ((parent) => {
					return parent.Height;
				}));

			relativeLayout.Children.Add (
				view: horizontal,
				xConstraint: Constraint.Constant (0),
				yConstraint: Constraint.RelativeToParent ((parent) => {
					return parent.Height / 2;
				}), 
				widthConstraint: Constraint.RelativeToParent ((parent) => {
					return parent.Width;
				}),
				heightConstraint: Constraint.Constant (1));

			MainGrid.Children.Add (new Frame { 
				BackgroundColor = settings.BaseColor,
				HasShadow = false, 
				Content = NameEd, 
				Padding = new Thickness (5, 2),
			}, 0, 0);
			MainGrid.Children.Add (AddressEd, 0, 1);
			MainGrid.Children.Add (relativeLayout, 0, 2);
			MainGrid.Children.Add (SaveBtn, 0, 3);
			BackgroundColor = settings.ColorDarkGray;
			this.Appearing += async (sender, e) => {
				await DisplayAlert ("Add", "Drag the map, or enter an address", "OK");
				BackgroundColor = settings.BaseColor;
				Content = MainGrid;
			};

		}

		#region Events

		async public void GetAddressFromMap (object o, EventArgs e)
		{
			var geo = new Geocoder ();
			IEnumerable<string> addresses = await geo.GetAddressesForPositionAsync (map.VisibleRegion.Center);
			string firstAddress = "";

			foreach (string addr in addresses) {
				if (firstAddress.Length == 0)
					firstAddress = addr;
			}
			AddressEd.Text = firstAddress.Replace ("\n", ", ");
			SaveBtn.IsVisible = true;
		}


		async public void SetMapFromAddress (object o, EventArgs e)
		{
			if (String.IsNullOrEmpty (AddressEd.Text)) {
				await DisplayAlert ("Address", "You need to type an address to find it. If you want to look up the map location, press 'Use this location' below the map", "OK");
				return;
			}
			var geo = new Geocoder ();
			IEnumerable<Position> posns = await geo.GetPositionsForAddressAsync (AddressEd.Text);
			Position? firstPosition = null;

			foreach (Position pos in posns) {
				if (firstPosition == null)
					firstPosition = pos;
			}
			if (firstPosition != null) {
				map.MoveToRegion (MapSpan.FromCenterAndRadius ((Position)firstPosition, Distance.FromMiles (0.3)));
				AddressEd.Unfocus ();
				SaveBtn.IsVisible = true;
			}
		}

		public async void DoAdd (object sender, EventArgs e)
		{
			if (String.IsNullOrWhiteSpace (NameEd.Text)) {
				await DisplayAlert ("Save", "You need to enter a place name", "OK");
				NameEd.Focus ();
				return;
			}
			Console.WriteLine ("AddPage4.DoAdd Push DedupPage");
			MapSpan span = map.VisibleRegion;
			var deDupPage = new AddPage5bDeDup (NameEd.Text, AddressEd.Text, span.Center);
			deDupPage.Cancelled += Failed;
			deDupPage.Confirmed += Succeeded;
			this.Navigation.PushAsync (deDupPage);
		}

		#endregion
	}
}


