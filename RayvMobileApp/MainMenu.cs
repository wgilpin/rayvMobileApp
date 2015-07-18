﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace RayvMobileApp
{
	public class MainMenu: ContentPage
	{



		public static void HandleLocationChanged (object sender, LocationUpdatedEventArgs e)
		{
			if (e.Location.Latitude != Persist.Instance.GpsPosition.Latitude || e.Location.Longitude != Persist.Instance.GpsPosition.Longitude) {
				Position NewPosition = e.Location;
				Persist.Instance.GpsPosition = NewPosition;
				Persist.Instance.SetConfig (settings.LAST_LAT, e.Location.Latitude);
				Persist.Instance.SetConfig (settings.LAST_LNG, e.Location.Longitude);
			}
//			Console.WriteLine (String.Format (
//				"GPS: {0:0.0000},{1:0.0000}",
//				Persist.Instance.GpsPosition.Latitude, 
//				Persist.Instance.GpsPosition.Longitude));
		}


		public MainMenu ()
		{
			Analytics.TrackPage ("MainMenu");
			Console.WriteLine ("MainMenu()");

			BackgroundColor = settings.ColorOffWhite;
			Grid grid = new Grid {
				Padding = new Thickness (20),
				VerticalOptions = LayoutOptions.Center,
				RowSpacing = 0,
				ColumnSpacing = 0,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (2, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (2, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (2, GridUnitType.Star) },
//					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
//					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Auto) },
				}
			};
			// ADD
			Image addImg = new Image {
				Source = settings.DevicifyFilename ("Big Add.png"),
				Aspect = Aspect.AspectFit,
			};
			var clickAdd = new TapGestureRecognizer ();
			clickAdd.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				this.Navigation.PushModalAsync (
					new RayvNav (new AddWhatPage ()));
			};
			addImg.GestureRecognizers.Add (clickAdd);

			// NEWS
			Image newsImg = new Image {
				Source = settings.DevicifyFilename ("Big activity.png"),
				Aspect = Aspect.AspectFit,
			};
			var clickNews = new TapGestureRecognizer ();
			clickNews.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: news button - push NewsPage");
				this.Navigation.PushModalAsync (
					new RayvNav (new NewsPage ()));
			};
			newsImg.GestureRecognizers.Add (clickNews);



			//  FIND ME A...
			Image choiceImg = new Image {
				Source = settings.DevicifyFilename ("Big find food.png"),
				Aspect = Aspect.AspectFit,
			};
			var clickFind = new TapGestureRecognizer ();
			clickFind.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: choice button - push ListPage");
				this.Navigation.PushModalAsync (
					new RayvNav (new FindChoicePage (this)));
			};
			choiceImg.GestureRecognizers.Add (clickFind);

			var FindText = new StackLayout { 
				VerticalOptions = LayoutOptions.End, 
				TranslationY = -17,
				Children = { 
					new Label {
						Text = "Find Food",
						FontSize = 35,
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Color.White,
					},
				}
			};
			FindText.GestureRecognizers.Add (clickFind);

			// ADD...
			var AddText = new StackLayout { 
				VerticalOptions = LayoutOptions.End, 
				TranslationY = -20,
				TranslationX = -5,
				Children = { 
					new Label {
						Text = "Add",
						FontSize = 35,
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Color.White,
					},
				}
			};
			AddText.GestureRecognizers.Add (clickAdd);

			// NEWS
			var NewsText = new StackLayout { 
				VerticalOptions = LayoutOptions.End, 
				TranslationY = -17,
				Children = { 
					new Label {
						Text = "Activity",
						FontSize = 35,
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Color.White,
					},
				}
			};
			NewsText.GestureRecognizers.Add (clickNews);

			// SHARE
			grid.Children.Add (choiceImg, 0, 0);
			grid.Children.Add (FindText, 0, 0);
//			grid.Children.Add (DarkBg, 0, 1);
			grid.Children.Add (addImg, 0, 1);
			grid.Children.Add (AddText, 0, 1);
			grid.Children.Add (newsImg, 0, 2);
			grid.Children.Add (NewsText, 0, 2);
//			grid.Children.Add (friendsImg, 0, 3);
//			grid.Children.Add (profileImg, 0, 4);
			this.BackgroundColor = settings.BaseColor;
			this.Content = grid;

//			AppDelegate.locationMgr = new LocationManager ();

			App.locationMgr.StartLocationUpdates ();
			Appearing += (sender, e) => {
				double y = (Height - 250) / 15;
				FindText.TranslationY = -y;
				AddText.TranslationY = -y;
				NewsText.TranslationY = -y;
				Console.WriteLine ($"MainMenu Appearing {y}");
			};
		}


	}
}

