using System;
using Xamarin.Forms;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin;

namespace RayvMobileApp.iOS
{
	public class LoginPage : ContentPage
	{
		Entry UserName;
		Entry Password;
		ActivityIndicator Spinner;

		void DoLogin (object sender, EventArgs e)
		{
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Persist.Instance.SetConfig ("username", UserName.Text);
				Persist.Instance.SetConfig ("pwd", Password.Text);
				restConnection.Instance.setCredentials (UserName.Text, Password.Text, "");
				Persist.Instance.Wipe ();
				Debug.WriteLine ("LoginPage.DoLogin: Push MainMenu");
				try {
					String user = Persist.Instance.GetConfig ("username");
					Insights.Identify (user, "email", user);
					Console.WriteLine ("AppDelegate Analytics ID: {0}", user);
				} catch (Exception ex) {
					Insights.Report (ex);
				}
				Device.BeginInvokeOnMainThread (() => {
					Spinner.IsRunning = false;
					this.Navigation.PushModalAsync (new MainMenu ());
				});
			})).Start ();
		}



		public void onWebRequest (string res)
		{
		}

		public LoginPage ()
		{
			Console.WriteLine ("LoginPage()");
			Spinner = new ActivityIndicator ();

			RayvButton loginButton = new RayvButton {
				Text = " Login ",
				Font = Font.SystemFontOfSize (NamedSize.Large),
			};
			loginButton.Clicked += this.DoLogin;

			UserName = new Entry { 
				Placeholder = "Username",
				VerticalOptions = LayoutOptions.Start,
				Text = Persist.Instance.GetConfig ("username"),
			};
			Password = new Entry {
				VerticalOptions = LayoutOptions.Start,
				Placeholder = "Password", 
				IsPassword = true,
				Text = Persist.Instance.GetConfig ("pwd"), 
			};
			RayvButton Register = new RayvButton ("Register New Account"){ BackgroundColor = Color.Yellow, };
			Register.Clicked += (s, e) => {
				this.Navigation.PushModalAsync (new RegisterPage ());
			};
			this.Content = new StackLayout {
				Padding = 20,
				Children = {
					new Label {
						Text = "Login",
						VerticalOptions = LayoutOptions.Start,
						HorizontalOptions = LayoutOptions.CenterAndExpand,
					},
					new ServerPicker (),
					UserName,
					Password,
					Spinner,
					loginButton,
					Register,
				}
			};

		}
	}
}

