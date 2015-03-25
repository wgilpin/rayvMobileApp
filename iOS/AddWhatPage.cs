using System;

using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class AddWhatPage : ContentPage
	{
		public AddWhatPage ()
		{
			Analytics.TrackPage ("AddWhatPage");
			Grid grid = new Grid {
				Padding = new Thickness (2, Device.OnPlatform (20, 2, 2), 2, 2),
				VerticalOptions = LayoutOptions.Center,
				RowSpacing = 0,
				ColumnSpacing = 0,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (2, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (2, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (2, GridUnitType.Auto) },
					//					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					//					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			Image addImg = new Image {
				Source = "Add place.png",
				Aspect = Aspect.AspectFit,
			};
			var clickAdd = new TapGestureRecognizer ();
			clickAdd.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				this.Navigation.PushModalAsync (new RayvNav (new AddPage1 ()));
			};
			addImg.GestureRecognizers.Add (clickAdd);

			// FRIENDS
			Image friendsImg = new Image {
				Source = "Add friend.png",
				Aspect = Aspect.AspectFit
			};
			var clickFriends = new TapGestureRecognizer ();
			clickFriends.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: friends button - not implemented");
				DisplayAlert ("Friends", "Not Implemented (yet)", "Shame");
			};
			friendsImg.GestureRecognizers.Add (clickFriends);

			StackLayout tools = new BottomToolbar (this, "add");

			grid.Children.Add (addImg, 0, 0);
			grid.Children.Add (friendsImg, 0, 1);
			grid.Children.Add (tools, 0, 2);
			Content = grid;
			BackgroundColor = settings.ColorDark;
		}
	}
}


