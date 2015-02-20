﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace RayvMobileApp.iOS
{
	public class MainMenu: ContentPage
	{

		void loadDataFromServer (ContentPage caller)
		{
			Persist.Instance.LoadFromDb ();
			Console.WriteLine ("loadDataFromServer");
			Persist.Instance.GetUserData (this, incremental: true);
//			ListPage.Setup (caller);
		}

		public void HandleLocationChanged (object sender, LocationUpdatedEventArgs e)
		{
			Position NewPosition = new Position (
				                       e.Location.Coordinate.Latitude,
				                       e.Location.Coordinate.Longitude);
			Persist.Instance.GpsPosition = NewPosition;
			Persist.Instance.SetConfig (settings.LAST_LAT, e.Location.Coordinate.Latitude);
			Persist.Instance.SetConfig (settings.LAST_LNG, e.Location.Coordinate.Longitude);
//			Console.WriteLine (String.Format (
//				"GPS: {0:0.0000},{1:0.0000}",
//				Persist.Instance.GpsPosition.Latitude, 
//				Persist.Instance.GpsPosition.Longitude));
		}


		public MainMenu ()
		{
			Console.WriteLine ("MainMenu()");
			loadDataFromServer (this);

			Grid grid = new Grid {
				Padding = 5,
				VerticalOptions = LayoutOptions.FillAndExpand,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			// ADD
			Image addImg = new Image {
				Source = "big-btn-add.png"
			};
			var clickAdd = new TapGestureRecognizer ();
			clickAdd.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				this.Navigation.PushModalAsync (new NavigationPage (new AddMenu ()));
			};
			addImg.GestureRecognizers.Add (clickAdd);

			// FRIENDS
			Image friendsImg = new Image {
				Source = "big-btn-friends.png"
			};
			var clickFriends = new TapGestureRecognizer ();
			clickFriends.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: friends button - not implemented");
				DisplayAlert ("Friends", "Not Implemented (yet)", "Shame");
			};
			friendsImg.GestureRecognizers.Add (clickFriends);

			// NEWS
			Image newsImg = new Image {
				Source = "big-btn-news.png"
			};
			var clickNews = new TapGestureRecognizer ();
			clickNews.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: news button - push NewsPage");
				this.Navigation.PushModalAsync (new NavigationPage (new NewsPage ()));
			};
			newsImg.GestureRecognizers.Add (clickNews);

			//  LIST
			Image placesImg = new Image {
				Source = "big-btn-places.png",
			};
			var clickList = new TapGestureRecognizer ();
			clickList.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: list button - push ListPage");
				this.Navigation.PushModalAsync (new NavigationPage (new ListPage ()));
			};
			placesImg.GestureRecognizers.Add (clickList);

			// SHARE
			Image profileImg = new Image {
				Source = "big-btn-profile.png"
			};
			var clickProfile = new TapGestureRecognizer ();
			clickProfile.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: profile button - push ProfilePage");
				this.Navigation.PushModalAsync (new NavigationPage (new ProfilePage ()));
			};
			profileImg.GestureRecognizers.Add (clickProfile);

			grid.Children.Add (placesImg, 0, 0);
			grid.Children.Add (newsImg, 0, 1);
			grid.Children.Add (addImg, 0, 2);
			grid.Children.Add (friendsImg, 0, 3);
			grid.Children.Add (profileImg, 0, 4);
			this.Content = grid;

			AppDelegate.locationMgr = new LocationManager ();
			AppDelegate.locationMgr.StartLocationUpdates ();
			AppDelegate.locationMgr.LocationUpdated += HandleLocationChanged;
		}

		private static NavigationPage _instance;

		public static NavigationPage Instance {
			get {
				if (_instance == null) {
					_instance = new NavigationPage (new MainMenu ());
				}
				return _instance;
			}
		}
	}
}

