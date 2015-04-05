using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace RayvMobileApp.iOS
{
	public class MainMenu: ContentPage
	{



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
			Analytics.TrackPage ("MainMenu");
			Console.WriteLine ("MainMenu()");

			BackgroundColor = settings.ColorOffWhite;
			Grid grid = new Grid {
				Padding = new Thickness (2, Device.OnPlatform (20, 2, 2), 2, 2),
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
				}
			};
			// ADD
			Image addImg = new Image {
				Source = "Big Add.png",
				Aspect = Aspect.AspectFit,
			};
			var clickAdd = new TapGestureRecognizer ();
			clickAdd.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				this.Navigation.PushModalAsync (
					new NavigationPage (new AddWhatPage ()) { 
						BarBackgroundColor = settings.ColorOffWhite,
						BarTextColor = settings.ColorDark,
					});
			};
			addImg.GestureRecognizers.Add (clickAdd);

			// NEWS
			Image newsImg = new Image {
				Source = "Big activity.png",
				Aspect = Aspect.AspectFit,
			};
			var clickNews = new TapGestureRecognizer ();
			clickNews.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: news button - push NewsPage");
				this.Navigation.PushModalAsync (
					new NavigationPage (new NewsPage ()){ BarBackgroundColor = settings.ColorDark });
			};
			newsImg.GestureRecognizers.Add (clickNews);



			//  FIND ME A...
			Image choiceImg = new Image {
				Source = "Big find food.png",
				Aspect = Aspect.AspectFit,
			};
			var clickChoice = new TapGestureRecognizer ();
			clickChoice.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: choice button - push ListPage");
				this.Navigation.PushModalAsync (
					new NavigationPage (new ListPage ()) { 
						BarBackgroundColor = settings.ColorDark,
						BarTextColor = Color.White,
					});
			};
			choiceImg.GestureRecognizers.Add (clickChoice);


			// SHARE
			grid.Children.Add (choiceImg, 0, 0);
			grid.Children.Add (addImg, 0, 1);
			grid.Children.Add (newsImg, 0, 2);
//			grid.Children.Add (friendsImg, 0, 3);
//			grid.Children.Add (profileImg, 0, 4);
			this.Content = grid;

			AppDelegate.locationMgr = new LocationManager ();
			AppDelegate.locationMgr.StartLocationUpdates ();
			AppDelegate.locationMgr.LocationUpdated += HandleLocationChanged;
		}


	}
}

