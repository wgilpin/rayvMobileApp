using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class BottomToolbar : StackLayout
	{
		BackgroundBox topYellow;
		BackgroundBox bottomYellow;

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
			this.Navigation.PushModalAsync (new NavigationPage (new AddWhatPage ()), false);
		}

		void ShowFriends (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: Friends button - NOT IMPLEMENTED");
			object page = this;
			while (!(page is Page))
				page = (page as View).Parent;
			if (page is Page)
				(page as Page).DisplayAlert ("Friends", "Not Implemented (yet)", "Shame");
//			this.Navigation.PushModalAsync (new NavigationPage (new FriendsPage ()), false);
		}


		void SetAddBtnYellow (string pressed, Grid gridMain, Grid selectedGrid)
		{
			if (pressed != "add") {
				// yellow behind the add button
				topYellow.Color = Color.FromHex ("#F3E90A");
				bottomYellow.Color = Color.FromHex ("#F3E90A");
			}
		}

		public BottomToolbar (Page page, String pressed = null)
		{
			VerticalOptions = LayoutOptions.EndAndExpand;
			Console.WriteLine ("toolbar()");
			Grid gridMain = new Grid {
				Padding = 2,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (30, GridUnitType.Absolute) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (30, GridUnitType.Absolute) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (25, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute) },
				},
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.FromHex ("#554FD9"),
			};

			Grid gridSecond = new Grid {
				Padding = 2,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (40, GridUnitType.Absolute) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (40, GridUnitType.Absolute) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (25, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute) },
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

			// PROFILE
			Image settingsImg = new ImageButton {
				Source = "icon-profile.png",
				OnClick = ShowProfile,
			};

			Button MoreBtn = new Button {
				Text = "more",
				TextColor = Color.White,
				FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Button)),
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.End,
			};
			MoreBtn.Clicked += (sender, e) => {
				gridMain.IsVisible = false;
				gridSecond.IsVisible = true;
				SetAddBtnYellow (pressed, gridMain, gridSecond);
			};

			Button BackBtn = new Button {
				Text = "< back",
				TextColor = Color.White,
				FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Button)),
				HorizontalOptions = LayoutOptions.End,
			};
			BackBtn.Clicked += (sender, e) => {
				gridMain.IsVisible = true;
				gridSecond.IsVisible = false;
				SetAddBtnYellow (pressed, gridMain, gridSecond);
			};

			gridMain.IsVisible = true;
			gridSecond.IsVisible = false;
			Grid selectedGrid = gridMain;
			int selectedColumn = 0;
			if (pressed != null) {
				switch (pressed) {
				case "news":
					newsImg.Source = "icon-news-pressed.png";
					selectedColumn = 3;
					break;
				case "list":
					ListImg.Source = "icon-grid-pressed.png";
					selectedColumn = 1;
					break;
				case "profile":
					selectedGrid = gridSecond;
					settingsImg.Source = "icon-profile-pressed.png";
					selectedColumn = 2;
					break;
				case "friends":
					selectedGrid = gridSecond;
					friendsImg.Source = "icon-friends-pressed.png";
					selectedColumn = 1;
					break;
				case "add":
					addImg.Source = "icon-add-pressed.png";
					selectedColumn = 2;
					break;
				}
				gridMain.IsVisible = selectedGrid == gridMain;
				gridSecond.IsVisible = selectedGrid == gridSecond;
				topYellow = new BackgroundBox ("#F3E90A");
				bottomYellow = new BackgroundBox ("#F3E90A");
				gridMain.Children.Add (topYellow, 2, 3, 0, 1);
				gridMain.Children.Add (bottomYellow, 2, 3, 1, 2);
				SetAddBtnYellow (pressed, gridMain, selectedGrid);
				// grey behind the current page
				selectedGrid.Children.Add (
					new BackgroundBox ("#444444"), selectedColumn, selectedColumn + 1, 0, 1);
				selectedGrid.Children.Add (
					new BackgroundBox ("#444444"), selectedColumn, selectedColumn + 1, 1, 2);
			}
			gridMain.RowSpacing = 0;
			gridSecond.RowSpacing = 0;
			gridMain.Children.Add (ListImg, 1, 2, 0, 1);
			gridMain.Children.Add (addImg, 2, 3, 0, 1);
			gridMain.Children.Add (newsImg, 3, 4, 0, 1);
			gridSecond.Children.Add (friendsImg, 1, 2, 0, 1);
			gridSecond.Children.Add (settingsImg, 2, 3, 0, 1);

			gridMain.Children.Add (new ToolbarButton ("List") { 
				OnClick = ShowList,
			}, 1, 2, 1, 2);
			gridMain.Children.Add (new ToolbarButton ("News") { 
				OnClick = ShowNews,
			}, 3, 4, 1, 2);
			ButtonWide AddBtn = new ToolbarButton ("Add") { 
				TextColor = Color.Black,
				OnClick = ShowAdd,
			};
			if (pressed == "add")
				AddBtn.TextColor = Color.White;
			gridMain.Children.Add (AddBtn, 2, 3, 1, 2);
			gridMain.Children.Add (MoreBtn, 4, 5, 0, 2);
			gridSecond.Children.Add (new ToolbarButton ("Friends") { 
				OnClick = ShowFriends,
			}, 1, 2, 1, 2);
			gridSecond.Children.Add (new ToolbarButton ("Profile") { 
				OnClick = ShowProfile,
			}, 2, 3, 1, 2);
			gridSecond.Children.Add (BackBtn, 0, 1, 0, 2);
			this.Children.Add (gridMain);
			this.Children.Add (gridSecond);

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

