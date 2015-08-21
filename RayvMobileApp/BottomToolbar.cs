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
			HeightRequest = 15;
		}
	}

	public class BottomToolbar : StackLayout
	{
		StackLayout bar;
		ImageButton friendsBtn;
		ImageButton FindBtn;
		Page CurrentPage;
		ImageButton newsBtn;

		public void SetActivityIcon ()
		{
			if (!(CurrentPage is NewsPage))
				newsBtn.Source = Persist.Instance.HaveActivity ? "Alert_activity2.png" : settings.DevicifyFilename ("TB default news.png");
		}

		void ShowFind (object s, EventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				Console.WriteLine ("Toolbar: List button - push ListPage");
				this.Navigation.PushModalAsync (new RayvNav (new FindChoicePage (CurrentPage)), false);

//			FindBtn.Spinner.IsRunning = false;
//				FindBtn.Spinner.IsVisible = false;
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
				Console.WriteLine ("Toolbar: Add button - push FriendsPage");
				this.Navigation.PushModalAsync (
					new RayvNav (new FriendsPage ()), false);
			});
		}


		public BottomToolbar (Page page, String pressed = null)
		{
			CurrentPage = page;
			VerticalOptions = LayoutOptions.End;
			Console.WriteLine ("toolbar()");
			HeightRequest = 70;
			Padding = 7;
			HorizontalOptions = LayoutOptions.FillAndExpand;
			BackgroundColor = settings.BaseColor;
			Orientation = StackOrientation.Horizontal;
			var addBtn = new ImageButton (settings.DevicifyFilename ("TB default add.png"), ShowAdd) { 
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				HeightRequest = 40,
			};
			friendsBtn = new ImageButton (settings.DevicifyFilename ("TB default friends.png"), ShowFriends) { 
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				HeightRequest = 40,
			};
			var newsIconFile = Persist.Instance.HaveActivity ? "Alert_activity2.png" : "TB default news.png";
			newsBtn = new ImageButton (settings.DevicifyFilename (newsIconFile), ShowNews) { 
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				HeightRequest = 40,
			};
			FindBtn = new ImageButton (settings.DevicifyFilename ("TB default search.png"), ShowFind) { 
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				HeightRequest = 40,
			};
			var settingsBtn = new ImageButton (settings.DevicifyFilename ("TB default profile.png"), ShowProfile) { 
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				HeightRequest = 40,
			};


			if (pressed != null) {
				switch (pressed) {
					case "list":
						FindBtn.Source = settings.DevicifyFilename ("TB active search.png");
						break;
					case "friends":
						friendsBtn.Source = settings.DevicifyFilename ("TB active friends.png");
						break;
					case "add":
						addBtn.Source = settings.DevicifyFilename ("TB active add.png");
						break;
					case "news":
						newsBtn.Source = settings.DevicifyFilename ("TB active news.png");
						break;
					case "profile":
						settingsBtn.Source = settings.DevicifyFilename ("TB active profile.png");
						break;
				}
			}
			Children.Add (FindBtn);
			Children.Add (friendsBtn);
			Children.Add (addBtn);
			Children.Add (newsBtn);
			Children.Add (settingsBtn);
			SetActivityIcon ();
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

