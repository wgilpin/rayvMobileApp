using System;

using Xamarin.Forms;


namespace RayvMobileApp
{
	public class AddWhatPage : ContentPage
	{
		public AddWhatPage ()
		{
			Analytics.TrackPage ("AddWhatPage");
			Grid grid = new Grid {
				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				VerticalOptions = LayoutOptions.EndAndExpand,
				RowSpacing = 0,
				ColumnSpacing = 0,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (120, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (120, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (2, GridUnitType.Auto) },
					//					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					//					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			Image addImg = new Image {
				Source = settings.DevicifyFilename ("Add place.png"),
				Aspect = Aspect.AspectFit,
				HeightRequest = 80,
			};
			var clickAdd = new TapGestureRecognizer ();
			clickAdd.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				this.Navigation.PushModalAsync (new RayvNav (new AddPage1 ()));
			};
			addImg.GestureRecognizers.Add (clickAdd);

			// FRIENDS
			Image friendsImg = new Image {
				Source = settings.DevicifyFilename ("Add friend.png"),
				Aspect = Aspect.AspectFit,
				HeightRequest = 80,
			};
			var clickFriends = new TapGestureRecognizer ();
			clickFriends.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: friends button - not implemented");
				DisplayAlert ("Friends", "Not Implemented (yet)", "Shame");
			};
			friendsImg.GestureRecognizers.Add (clickFriends);

			StackLayout tools = new BottomToolbar (this, "add");

			var PlaceText = new StackLayout { 
				VerticalOptions = LayoutOptions.End, 
				TranslationY = -70,
				Children = { 
					new Label {
						Text = "Add Place",
						FontSize = 35,
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Color.White,
					},
				}
			};
			PlaceText.GestureRecognizers.Add (clickAdd);
			var FriendText = new StackLayout { 
				VerticalOptions = LayoutOptions.End, 
				TranslationY = -70,
				Children = { 
					new Label {
						Text = "Add Friend",
						FontSize = 35,
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Color.White,
					},
				}
			};
			FriendText.GestureRecognizers.Add (clickFriends);
			grid.Children.Add (addImg, 0, 0);
			grid.Children.Add (PlaceText, 0, 1);
			grid.Children.Add (friendsImg, 0, 2);
			grid.Children.Add (FriendText, 0, 3);
			grid.Children.Add (tools, 0, 4);
			Content = grid;
			BackgroundColor = settings.BaseColor;
		}
	}
}


