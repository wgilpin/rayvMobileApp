using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class BottomToolbar : StackLayout
	{
		void ShowList (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: List button - push ListPage");
			this.Navigation.PushModalAsync (new NavigationPage (new ListPage ()), false);
		}

		void ShowNews (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: News button - push NewsPage");
			this.Navigation.PushModalAsync (new NavigationPage (new NewsPage ()), false);
		}

		void ShowProfile (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: Profile button - push ProfilePage");
			this.Navigation.PushModalAsync (new NavigationPage (new ProfilePage ()), false);
		}

		void ShowAdd (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: Add button - push AddMenu");
			this.Navigation.PushModalAsync (new NavigationPage (new AddMenu ()), false);
		}

		void ShowFriends (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: Friends button - NOT IMPLEMENTED");
//			this.Navigation.PushModalAsync (new NavigationPage (new FriendsPage ()), false);
		}


		public BottomToolbar (Page page, String pressed = null)
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
					new RowDefinition { Height = new GridLength (22, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (12, GridUnitType.Absolute) },
				},
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.FromHex ("#554FD9"),
			};

			Image addImg = new ImageButton {
				Source = "icon-add.png",
				OnClick = ShowAdd,
			};

			// FRIENDS
			Image friendsImg = new ImageButton {
				Source = "icon-friends.png",
				OnClick = ShowFriends,
			};


			// NEWS
			Image newsImg = new ImageButton {
				Source = "icon-news.png",
				OnClick = ShowNews,
			};

			//  LIST
			Image ListImg = new ImageButton {
				Source = "icon-grid.png",
				OnClick = ShowList,
			};

			// SHARE
			Image settingsImg = new ImageButton {
				Source = "icon-profile.png",
				OnClick = ShowProfile,
			};
			int selectedColumn = 0;
			if (pressed != null) {
				switch (pressed) {
				case "news":
					newsImg.Source = "icon-news-pressed.png";
					selectedColumn = 1;
					break;
				case "list":
					ListImg.Source = "icon-grid-pressed.png";
					selectedColumn = 0;
					break;
				case "profile":
					settingsImg.Source = "icon-profile-pressed.png";
					selectedColumn = 4;
					break;
				case "friends":
					friendsImg.Source = "icon-friends-pressed.png";
					selectedColumn = 3;
					break;
				case "add":
					addImg.Source = "icon-add-pressed.png";
					selectedColumn = 2;
					break;
				}
				if (selectedColumn != 2) {
					// yellow behind the add button
					grid.Children.Add (
						new BackgroundBox ("#F3E90A"), 2, 3, 0, 1);
					grid.Children.Add (
						new BackgroundBox ("#F3E90A"), 2, 3, 1, 2);
				}
				// grey behind the current page
				grid.Children.Add (
					new BackgroundBox ("#444444"), selectedColumn, selectedColumn + 1, 0, 1);
				grid.Children.Add (
					new BackgroundBox ("#444444"), selectedColumn, selectedColumn + 1, 1, 2);
			}
			grid.RowSpacing = 0;
			grid.Children.Add (ListImg, 0, 1, 0, 1);
			grid.Children.Add (newsImg, 1, 2, 0, 1);
			grid.Children.Add (addImg, 2, 3, 0, 1);
			grid.Children.Add (friendsImg, 3, 4, 0, 1);
			grid.Children.Add (settingsImg, 4, 5, 0, 1);

			grid.Children.Add (new ToolbarButton ("List") { 
				OnClick = ShowList,
			}, 0, 1, 1, 2);
			grid.Children.Add (new ToolbarButton ("News") { 
				OnClick = ShowNews,
			}, 1, 2, 1, 2);
			ButtonWide AddBtn = new ToolbarButton ("Add") { 
				TextColor = Color.Black,
				OnClick = ShowAdd,
			};
			if (pressed == "add")
				AddBtn.TextColor = Color.White;
			grid.Children.Add (AddBtn, 2, 3, 1, 2);
			grid.Children.Add (new ToolbarButton ("Friends") { 
				OnClick = ShowFriends,
			}, 3, 4, 1, 2);
			grid.Children.Add (new ToolbarButton ("Profile") { 
				OnClick = ShowProfile,
			}, 4, 5, 1, 2);

			this.Children.Add (grid);
		}
	}

	class BackgroundBox : BoxView
	{
		public BackgroundBox (string hexColor) : base ()
		{
			BackgroundColor = Color.FromHex (hexColor);
			HorizontalOptions = LayoutOptions.FillAndExpand;
			VerticalOptions = LayoutOptions.FillAndExpand;
		}
	}

	class ToolbarButton: ButtonWide
	{
		public ToolbarButton (String text) : base ()
		{
			Text = text;
			TextColor = Color.White;
			HorizontalOptions = LayoutOptions.Center;
		}
	}
}

