using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class toolbar : StackLayout
	{
		public toolbar (Page page)
		{
			Console.WriteLine ("toolbar()");
			Orientation = StackOrientation.Horizontal;
			VerticalOptions = LayoutOptions.EndAndExpand;
			Button recentBtn = new Button {
				Text = "Recent",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			Button listBtn = new Button {
				Text = "List",
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			listBtn.Clicked += (object sender, EventArgs e) => {
				Console.WriteLine ("Toolbar: list button");
				this.Navigation.PushModalAsync (new NavigationPage (new ListPage ()));
			};
			Button addBtn = new Button {
				Text = "Add",
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			addBtn.Clicked += (object sender, EventArgs e) => {
				Console.WriteLine ("Toolbar: add button");
				this.Navigation.PushModalAsync (new NavigationPage (new AddMenu ()));
			};
			Button friendsBtn = new Button {
				Text = "Friends",
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			Button settingsBtn = new Button {
				Text = "Settings",
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			settingsBtn.Clicked += (object sender, EventArgs e) => {
				Console.WriteLine ("Toolbar: settings button");
				this.Navigation.PushModalAsync (new NavigationPage (new SettingsPage ()));
			};
			Children.Add (recentBtn);
			Children.Add (listBtn);
			Children.Add (addBtn);
			Children.Add (friendsBtn);
			Children.Add (settingsBtn);
		}
	}
}

