using System;
using Xamarin.Forms;
using Foundation;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{
	public class SettingsPage : ContentPage
	{
		RayvButton LogoutBtn;

		void DoLogout (object sender, EventArgs e)
		{
			Persist.Instance.SetConfig ("pwd", "");
			Debug.WriteLine ("SettingsPage.DoLogout: Push LoginPage");
			this.Navigation.PushModalAsync (new LoginPage ());
		}

		public SettingsPage ()
		{
			LogoutBtn = new RayvButton {
				Text = " Logout ",
			};
			LogoutBtn.Clicked += DoLogout;

			StackLayout tools = new toolbar (this);
			Content = new StackLayout {
				Children = {
					LogoutBtn,
					new Label {
						Text = String.Format (
							"Version {0}-{1}", 
							NSBundle.MainBundle.InfoDictionary ["CFBundleShortVersionString"],
							NSBundle.MainBundle.InfoDictionary ["CFBundleVersion"]),
					},
					new Label {
						Text = String.Format (
							"{0:0.0000},{1:0.0000}",
							Persist.Instance.GpsPosition.Latitude, 
							Persist.Instance.GpsPosition.Longitude),
					},
					tools
				}
			};
		}
	}
}

