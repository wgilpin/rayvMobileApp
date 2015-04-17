using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class BottomToolbar : StackLayout
	{
		BackgroundBox topYellow;
		BackgroundBox bottomYellow;
		Grid grid;

		void ShowList (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: List button - push ListPage");
			this.Navigation.PushModalAsync (
				new NavigationPage (new ListPage ()) { 
					BarBackgroundColor = ColorUtil.Darker (settings.BaseColor),
					BarTextColor = Color.White,
				}, false);
		}

		void ShowNews (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: News button - push NewsPage");
			this.Navigation.PushModalAsync (
				new NavigationPage (new NewsPage ()) { 
					BarBackgroundColor = ColorUtil.Darker (settings.BaseColor),
					BarTextColor = Color.White,
				}, false);
		}

		void ShowProfile (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: Profile button - push ProfilePage");
			this.Navigation.PushModalAsync (
				new NavigationPage (new ProfilePage ()) { 
					BarBackgroundColor = ColorUtil.Darker (settings.BaseColor),
					BarTextColor = Color.White,
				}, false);
		}

		void ShowAdd (object s, EventArgs e)
		{
			Console.WriteLine ("Toolbar: Add button - push AddMenu");
			this.Navigation.PushModalAsync (
				new NavigationPage (new AddWhatPage ()) { 
					BarBackgroundColor = ColorUtil.Darker (settings.BaseColor),
					BarTextColor = Color.White,
				}, false);
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


		public BottomToolbar (Page page, String pressed = null)
		{
			VerticalOptions = LayoutOptions.EndAndExpand;
			Console.WriteLine ("toolbar()");
			grid = new Grid {
				Padding = 0,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
//					new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute) },
				},
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = ColorUtil.Darker (settings.BaseColor),
				ColumnSpacing = 0,
			};

			Image addImg = new ImageButton ("TB default add.png", ShowAdd) { HorizontalOptions = LayoutOptions.Center };
			Image friendsImg = new ImageButton ("TB default friends.png", ShowFriends) { HorizontalOptions = LayoutOptions.Center };
			Image newsImg = new ImageButton ("TB default news.png", ShowNews) { HorizontalOptions = LayoutOptions.Center };
			Image ListImg = new ImageButton ("TB default search.png", ShowList) { HorizontalOptions = LayoutOptions.Center };
			Image settingsImg = new ImageButton ("TB default profile.png", ShowProfile) { HorizontalOptions = LayoutOptions.Center };


			int selectedColumn = 0;
			if (pressed != null) {
				switch (pressed) {
				case "list":
					ListImg.Source = "TB active search.png";
					selectedColumn = 0;
					break;
				case "friends":
					friendsImg.Source = "TB active friends.png";
					selectedColumn = 1;
					break;
				case "add":
					addImg.Source = "TB active add.png";
					selectedColumn = 2;
					break;
				case "news":
					newsImg.Source = "TB active news.png";
					selectedColumn = 3;
					break;
				case "profile":
					settingsImg.Source = "TB active profile.png";
					selectedColumn = 4;
					break;
				}
			}
			grid.RowSpacing = 0;
			grid.Children.Add (ListImg, 0, 0);
			grid.Children.Add (friendsImg, 1, 0);
			grid.Children.Add (addImg, 2, 0);
			grid.Children.Add (newsImg, 3, 0);
			grid.Children.Add (settingsImg, 4, 0);
			Children.Add (grid);
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

