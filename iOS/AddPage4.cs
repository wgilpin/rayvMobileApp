﻿using System;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections.Generic;

namespace RayvMobileApp.iOS
{
	public class AddPage4 : ContentPage
	{
		Map map;
		EntryWithChangeButton AddressEd;
		Button SaveBtn;

		public AddPage4 (Position searchPosition)
		{
			Xamarin.FormsMaps.Init ();
			map = new Map (
				MapSpan.FromCenterAndRadius (
					Persist.Instance.GpsPosition, Distance.FromMiles (0.3))) {
				IsShowingUser = true,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			map.IsShowingUser = true;

			Grid grid = new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (30, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (500, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (30, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (30, GridUnitType.Absolute) },
				},
			};

			AddressEd = new EntryWithChangeButton { 
				PlaceHolder = "Find address on map", 
				ButtonText = "Find",
				OnClick = SetMapFromAddress,
			};
			Button HereBtn = new RayvButton { 
				Text = "Use this location",
				OnClick = GetAddressFromMap,
			};
			SaveBtn = new ButtonWide { 
				BackgroundColor = Color.Blue,
				TextColor = Color.White,
				FontAttributes = FontAttributes.Bold,
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

			grid.Children.Add (AddressEd, 0, 0);
			grid.Children.Add (relativeLayout, 0, 1);
			grid.Children.Add (HereBtn, 0, 2);
			grid.Children.Add (SaveBtn, 0, 3);
			this.Content = grid;
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
			AddressEd.Text = firstAddress;
			SaveBtn.IsVisible = true;
		}


		async public void SetMapFromAddress (object o, EventArgs e)
		{
			if (String.IsNullOrEmpty (AddressEd.Text)) {
				DisplayAlert ("Address", "You need to type an address to find it. If you want to look up the map location, press 'Use this location' below the map", "OK");
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
				AddressEd.Entry.Unfocus ();
				SaveBtn.IsVisible = true;
			}
		}

		public void DoAdd (object sender, EventArgs e)
		{
			Console.WriteLine ("AddMapPage.DoAdd Push EditPage");
			MapSpan span = map.VisibleRegion;
			this.Navigation.PushAsync (new EditPage (span.Center, AddressEd.Text, addingNewPlace: true));
		}

		#endregion
	}
}

