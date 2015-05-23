using System;
using Xamarin.Forms;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin;

namespace RayvMobileApp
{
	public class LoginPage : ContentPage
	{
		Entry UserName;
		Entry Password;
		ActivityIndicator Spinner;
		Label Error;

		void DoLogin (object sender, EventArgs e)
		{
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				if (string.IsNullOrEmpty (Persist.Instance.GetConfig (settings.SERVER)))
					Persist.Instance.SetConfig (settings.SERVER, settings.DEFAULT_SERVER);
				Persist.Instance.SetConfig (settings.USERNAME, UserName.Text);
				Persist.Instance.SetConfig (settings.PASSWORD, Password.Text);
				try {
					Persist.Instance.Online = false;
					if (Persist.Instance.Online) {
						//Delete Last Sync time to force a full refresh
						Persist.Instance.SetConfig (settings.LAST_SYNC, null);
						restConnection.Instance.setCredentials (UserName.Text, Password.Text, "");
						Insights.Identify (UserName.Text, "server", Persist.Instance.GetConfig (settings.SERVER));
						Persist.Instance.Wipe ();
						Persist.Instance.LoadFromDb ();
						Persist.Instance.GetUserData (
							onFail: () => {
								Device.BeginInvokeOnMainThread (() => {
									Spinner.IsRunning = false;
								});
							},
							onSucceed: () => {
								Device.BeginInvokeOnMainThread (() => {
									Debug.WriteLine ("LoginPage.DoLogin: Push MainMenu");
									Spinner.IsRunning = false;
									this.Navigation.PushModalAsync (new MainMenu ());
								});
							}, 
							incremental: false);
					
					} else {
						//login failed
						Device.BeginInvokeOnMainThread (() => {
							Error.IsVisible = true;
							Spinner.IsRunning = false;
						});
					}
				} catch (ProtocolViolationException) {
					Device.BeginInvokeOnMainThread (() => {
						DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
						Spinner.IsRunning = false;
					});
				} catch (UnauthorizedAccessException) {
					Device.BeginInvokeOnMainThread (() => {
						Error.IsVisible = true;
						Spinner.IsRunning = false;
					});
				}


			})).Start ();
		}



		public void onWebRequest (string res)
		{
		}

		public LoginPage ()
		{
			Analytics.TrackPage ("LoginPage");
			Console.WriteLine ("LoginPage()");
			Spinner = new ActivityIndicator { Color = Color.Red, };

			RayvButton loginButton = new RayvButton {
				Text = " Login ",
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button)),
			};
			loginButton.Clicked += this.DoLogin;

			UserName = new Entry { 
				Placeholder = "Username",
				VerticalOptions = LayoutOptions.Start,
				Text = Persist.Instance.GetConfig (settings.USERNAME),
			};
			Password = new Entry {
				VerticalOptions = LayoutOptions.Start,
				Placeholder = "Password", 
				Text = Persist.Instance.GetConfig (settings.PASSWORD), 
			};
			RayvButton Register = new RayvButton {
				Text = "Register New Account", 
				BackgroundColor = Color.Yellow, 
				TextColor = ColorUtil.Darker (settings.BaseColor),
			};
			Register.Clicked += (s, e) => {
				this.Navigation.PushModalAsync (
					new NavigationPage (new RegisterPage ()) { 
						BarBackgroundColor = settings.BaseColor,
						BarTextColor = Color.White,
					}, false);
			};
			Error = new Label {
				Text = "User Name & Password Don't match", 
				TextColor = Color.White, 
				BackgroundColor = Color.Red, 
				FontAttributes = FontAttributes.Bold,
				IsVisible = false,
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
					Error,
					Spinner,
					loginButton,
					Register,
				}
			};

		}
	}
}

