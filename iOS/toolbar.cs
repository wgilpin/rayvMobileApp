using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class toolbar : StackLayout
	{
		void GoSettingsPage (object s, EventArgs e)
		{
			this.Navigation.PushModalAsync (new NavigationPage (new SettingsPage ()), false);
		}

		public toolbar (Page page, String pressed = null)
		{
			VerticalOptions = LayoutOptions.EndAndExpand;
			Console.WriteLine ("toolbar()");
			Grid grid = new Grid {
				Padding = 2,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (36, GridUnitType.Absolute) },
//					new RowDefinition { Height = new GridLength (18, GridUnitType.Absolute) },
				},
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.Black,
			};

			Image addImg = new ImageButton {
				Source = "icon-add.png",
				OnClick = (s, e) => {
					Console.WriteLine ("Toolbar: Add button - push AddMenu");
					this.Navigation.PushModalAsync (new NavigationPage (new AddMenu ()), false);
				},
			};

			// FRIENDS
			Image friendsImg = new ImageButton {
				Source = "icon-friends.png",
			};


			// NEWS
			Image newsImg = new ImageButton {
				Source = "icon-news.png",
				OnClick = (s, e) => {
					this.Navigation.PushModalAsync (new NavigationPage (new NewsPage ()), false);
				},
			};

			//  LIST
			Image ListImg = new ImageButton {
				Source = "icon-grid.png",
				OnClick = (s, e) => {
					this.Navigation.PushModalAsync (new NavigationPage (new ListPage ()), false);
				},
			};

			// SHARE
			Image settingsImg = new ImageButton {
				Source = "icon-gear.png",
				OnClick = GoSettingsPage,
			};
			if (pressed != null) {
				switch (pressed) {
				case "news":
					newsImg.Source = "icon-news-pressed.png";
					break;
				case "list":
					ListImg.Source = "icon-grid-pressed.png";
					break;
				case "settings":
					settingsImg.Source = "icon-gear-pressed.png";
					break;
				case "friends":
					friendsImg.Source = "icon-friends-pressed.png";
					break;
				case "add":
					addImg.Source = "icon-add-pressed.png";
					break;
				}
				;
			}

			grid.Children.Add (newsImg, 0, 1, 0, 1);
			grid.Children.Add (ListImg, 1, 2, 0, 1);
			grid.Children.Add (addImg, 2, 3, 0, 1);
			grid.Children.Add (friendsImg, 3, 4, 0, 1);
			grid.Children.Add (settingsImg, 4, 5, 0, 1);

//			Label NewsLbl = new Label { Text = "News",  TextColor = Color.White };
//			grid.Children.Add (new Label { Text = "News",  TextColor = Color.White }, 0, 1, 0, 1);
//			grid.Children.Add (new LabelWide ("List"){ TextColor = Color.White }, 1, 2, 1, 2);
//			grid.Children.Add (new LabelWide ("Add"){ TextColor = Color.White }, 2, 3, 1, 2);
//			grid.Children.Add (new LabelWide ("Friends"){ TextColor = Color.White }, 3, 4, 1, 2);
//			grid.Children.Add (new LabelWide ("Settings"){ TextColor = Color.White }, 4, 5, 1, 2);

			this.Children.Add (grid);
		}
	}
}

