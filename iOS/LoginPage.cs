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

		void DoLogin (object sender, EventArgs e)
		{
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
			this.Navigation.PushModalAsync (new MainMenu ());
		}



		public void onWebRequest (string res)
		{
		}

		public LoginPage ()
		{
			Console.WriteLine ("LoginPage()");
			Picker picker = new Picker {
				Title = "Server",
				VerticalOptions = LayoutOptions.Start
			};

			picker.Items.Add ("Local");
			picker.Items.Add ("Dev");
			picker.Items.Add ("Pre-Prod");
			picker.Items.Add ("Prod V2");
			picker.SelectedIndexChanged += (sender, args) => {
				string server_url = "";
				switch (picker.SelectedIndex) {
				case 0:
					server_url = "http://localhost:8080/";
					System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
					restConnection.Instance.setBaseUrl (server_url);
					break;
				case 1:
					server_url = "http://192.168.1.9:8080/";
					System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
					restConnection.Instance.setBaseUrl (server_url);
					break;
				case 2:
					server_url = "https://shout-about.appspot.com/";
					System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
					restConnection.Instance.setBaseUrl (server_url);
					break;
				case 3:
					server_url = "https://rayv-app.appspot.com/";
					System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
					restConnection.Instance.setBaseUrl (server_url);
					break;
				}
				Persist.Instance.SetConfig ("server", server_url);


			};
			picker.SelectedIndex = 3;

			RayvButton loginButton = new RayvButton {

				Text = " Login ",
				Font = Font.SystemFontOfSize (NamedSize.Large),

				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Start,
			};
			loginButton.Clicked += this.DoLogin;

			UserName = new Entry { 
				Placeholder = "Username",
				VerticalOptions = LayoutOptions.Start,
			};
			Password = new Entry {
				VerticalOptions = LayoutOptions.Start,
				Placeholder = "Password", 
				IsPassword = true
			};

			this.Content = new StackLayout {
				Padding = 20,
				Children = {
					new Label {
						Text = "Login",
						VerticalOptions = LayoutOptions.Start,
						HorizontalOptions = LayoutOptions.CenterAndExpand,
					},
					picker,
					UserName,
					Password,
					loginButton
				}
			};

		}
	}
}

