using System;
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
			ListPage.Setup (caller);
		}

		public void HandleLocationChanged (object sender, LocationUpdatedEventArgs e)
		{
			Position NewPosition = new Position (
				                       e.Location.Coordinate.Latitude,
				                       e.Location.Coordinate.Longitude);
			Persist.Instance.GpsPosition = NewPosition;
			Persist.Instance.SetConfigDouble ("LastLat", e.Location.Coordinate.Latitude);
			Persist.Instance.SetConfigDouble ("LastLng", e.Location.Coordinate.Longitude);
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
				Source = "AddBigBtn.png"
			};
			var clickAdd = new TapGestureRecognizer ();
			clickAdd.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				this.Navigation.PushModalAsync (new NavigationPage (new AddMenu ()));
			};
			addImg.GestureRecognizers.Add (clickAdd);

			// FRIENDS
			Image friendsImg = new Image {
				Source = "FriendsBigBtn.png"
			};

			// NEWS
			Image newsImg = new Image {
				Source = "NewsBigBtn.png"
			};

			//  LIST
			Image placesImg = new Image {
				Source = "PlacesBigBtn.png",
			};
			var clickList = new TapGestureRecognizer ();
			clickList.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: list button - push ListPage");
				this.Navigation.PushModalAsync (new NavigationPage (new ListPage ()));
			};
			placesImg.GestureRecognizers.Add (clickList);

			// SHARE
			Image shareImg = new Image {
				Source = "ShareBigBtn.png"
			};

			grid.Children.Add (placesImg, 0, 0);
			grid.Children.Add (newsImg, 0, 1);
			grid.Children.Add (addImg, 0, 2);
			grid.Children.Add (friendsImg, 0, 3);
			grid.Children.Add (shareImg, 0, 4);
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

