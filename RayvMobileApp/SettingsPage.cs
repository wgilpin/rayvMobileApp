using System;
using Xamarin.Forms;

//using Foundation;
using System.Diagnostics;

namespace RayvMobileApp
{
	public class SettingsPage : ContentPage
	{
		RayvButton LogoutBtn;

		void DoLogout (object sender, EventArgs e)
		{
			//Persist.Instance.SetConfig ("pwd", "");
			Debug.WriteLine ("SettingsPage.DoLogout: Push LoginPage");
			this.Navigation.PushModalAsync (new LoginPage ());
		}

		public SettingsPage ()
		{
			LogoutBtn = new RayvButton {
				Text = " Logout ",
			};
			LogoutBtn.Clicked += DoLogout;


			StackLayout Inner = new StackLayout {
				Padding = 5,
				Children = {
					LogoutBtn,
					new Label {
						Text = String.Format (
							"Version {0}", 
							DependencyService.Get<IAppData> ().AppVersion ()),
					},
					new Label {
						Text = String.Format (
							"{0:0.0000},{1:0.0000}",
							Persist.Instance.GpsPosition.Latitude, 
							Persist.Instance.GpsPosition.Longitude),
					},
					new Label {
						Text = String.Format (
							"Server: {0}",
							Persist.Instance.GetConfig (settings.SERVER)),
					},
					new ButtonWide {
						BackgroundColor = Color.Red,
						Text = "[Debug] Clear Local Data",
						OnClick = async (s, e) => {
							if (await DisplayAlert (
								    "Are You Sure?", 
								    "This will wipe the list on this phone." +
								    " It will be reloaded when you go to the Find page", 
								    "Yes", 
								    "No")) {
								Persist.Instance.Wipe ();
								this.Navigation.PushModalAsync (
									new NavigationPage (new ListPage ()) { 
										BarBackgroundColor = settings.BaseColor,
										BarTextColor = Color.White,
									}, false);
							}
						}
					},
				}
			};
			Content = new StackLayout {
				Children = {
					Inner,
					new BottomToolbar (this, "settings"),
				}
			};
		}
	}
}

