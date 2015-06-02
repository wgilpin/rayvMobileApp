using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	class BottomToolbarButton : Grid
	{
		private EventHandler _onClickHandler;
		public ActivityIndicator Spinner;
		private ImageButton _ib;


		public ImageSource Source {
			get { return _ib.Source; }
			set { _ib.Source = value; }
		}

		void ButtonClicked (object o, EventArgs e)
		{
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Device.BeginInvokeOnMainThread (() => {
					Spinner.IsRunning = true;
					Spinner.IsVisible = true;
				});
				if (_onClickHandler != null) {
					_onClickHandler (o, e);
				}
			})).Start ();
		}

		public BottomToolbarButton (string source, EventHandler onClick) : base ()
		{
			_onClickHandler = onClick;
			_ib = new ImageButton (source, ButtonClicked);
			Spinner = new ActivityIndicator { Color = Color.White, IsVisible = false };
			VerticalOptions = LayoutOptions.FillAndExpand;
			HorizontalOptions = LayoutOptions.FillAndExpand;
			RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			Children.Add (_ib, 0, 0);
			Children.Add (Spinner, 0, 0);
		}
	}

	public class BottomToolbar : StackLayout
	{
		Grid grid;
		BottomToolbarButton friendsImg;

		void ShowFind (object s, EventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				Console.WriteLine ("Toolbar: List button - push ListPage");
				this.Navigation.PushModalAsync (
					new RayvNav (new FindChoicePage ()), false);
			});
		}

		void ShowNews (object s, EventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				Console.WriteLine ("Toolbar: News button - push NewsPage");
				this.Navigation.PushModalAsync (
					new RayvNav (new NewsPage ()), false);
			});
		}

		void ShowProfile (object s, EventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				Console.WriteLine ("Toolbar: Profile button - push ProfilePage");
				this.Navigation.PushModalAsync (
					new RayvNav (new ProfilePage ()), false);
			});
		}

		void ShowAdd (object s, EventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				Console.WriteLine ("Toolbar: Add button - push AddMenu");
				this.Navigation.PushModalAsync (
					new RayvNav (new AddWhatPage ()), false);
			});
		}

		void ShowFriends (object s, EventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				Console.WriteLine ("Toolbar: Friends button - NOT IMPLEMENTED");
				object page = this;
				while (!(page is Page))
					page = (page as View).Parent;
				if (page is Page)
					(page as Page).DisplayAlert ("Friends", "Not Implemented (yet)", "Shame");
				friendsImg.Spinner.IsVisible = false;
//			this.Navigation.PushModalAsync (new NavigationPage (new FriendsPage ()), false);
			});
		}


		public BottomToolbar (Page page, String pressed = null)
		{
			VerticalOptions = LayoutOptions.EndAndExpand;
			Console.WriteLine ("toolbar()");
			grid = new Grid {
				Padding = 5,
				ColumnSpacing = 13,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
//					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (50, GridUnitType.Absolute) },
				},
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = settings.BaseColor,
			};

			var addImg = new BottomToolbarButton (settings.DevicifyFilename ("TB default add.png"), ShowAdd) { 
				HorizontalOptions = LayoutOptions.Center
			};
			friendsImg = new BottomToolbarButton (settings.DevicifyFilename ("TB default friends.png"), ShowFriends) { 
				HorizontalOptions = LayoutOptions.Center
			};
			var newsImg = new BottomToolbarButton (settings.DevicifyFilename ("TB default news.png"), ShowNews) { 
				HorizontalOptions = LayoutOptions.Center
			};
			var ListImg = new BottomToolbarButton (settings.DevicifyFilename ("TB default search.png"), ShowFind) { 
				HorizontalOptions = LayoutOptions.Center
			};
			var settingsImg = new BottomToolbarButton (settings.DevicifyFilename ("TB default profile.png"), ShowProfile) { 
				HorizontalOptions = LayoutOptions.Center
			};


			if (pressed != null) {
				switch (pressed) {
					case "list":
						ListImg.Source = settings.DevicifyFilename ("TB active search.png");
						break;
					case "friends":
						friendsImg.Source = settings.DevicifyFilename ("TB active friends.png");
						break;
					case "add":
						addImg.Source = settings.DevicifyFilename ("TB active add.png");
						break;
					case "news":
						newsImg.Source = settings.DevicifyFilename ("TB active news.png");
						break;
					case "profile":
						settingsImg.Source = settings.DevicifyFilename ("TB active profile.png");
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

