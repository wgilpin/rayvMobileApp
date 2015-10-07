using System;
using Xamarin.Forms;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin;
using System.Linq;

namespace RayvMobileApp
{
	public class LoginPage : ContentPage
	{
		Entry UserName;
		Entry Password;
		ActivityIndicator Spinner;
		Label Error;
		ServerPicker Servers;
		Label LoadingMessage;
		ProgressBar progBar;




		public void SetProgress (string message, Double progress)
		{
			Device.BeginInvokeOnMainThread (() => {
				LoadingMessage.Text = message;
				Console.WriteLine ("Loading message: {0}", message);
				progBar.ProgressTo (progress, 250, Easing.Linear);
			});
		}

		void ShowLogin ()
		{
			Device.BeginInvokeOnMainThread (() => {
				Spinner.IsRunning = false;
				LoadingMessage.IsVisible = false;
				progBar.IsVisible = false;
				if (!string.IsNullOrEmpty (Error.Text))
					Error.IsVisible = true;
			});
		}

		void DoLogin (object sender, EventArgs e)
		{
			Spinner.IsRunning = true;
			LoadingMessage.IsVisible = true;
			progBar.IsVisible = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				if (string.IsNullOrEmpty (Persist.Instance.GetConfig (settings.SERVER)))
					Persist.Instance.SetConfig (settings.SERVER, settings.SERVER_DEFAULT);
				
				try {
					Persist.Instance.Online = false;
					if (Persist.Instance.Online) {
						//Delete Last Sync time to force a full refresh
						Persist.Instance.SetConfig (settings.LAST_SYNC, null);
						restConnection.Instance.setCredentials (UserName.Text, Password.Text.ToLowerInvariant (), "");
						Insights.Identify (UserName.Text, "server", Persist.Instance.GetConfig (settings.SERVER));
						Persist.Instance.SetConfig (settings.USERNAME, UserName.Text);
						Persist.Instance.SetConfig (settings.PASSWORD, Password.Text.ToLowerInvariant ());
						Persist.Instance.Wipe ();
						Persist.Instance.LoadFromDb ();
						Persist.Instance.GetUserData (
							onFail: () => {
								Device.BeginInvokeOnMainThread (() => {
									Error.Text = "Bad Login";
									Persist.Instance.SetConfig (settings.USERNAME, null);
									Persist.Instance.SetConfig (settings.PASSWORD, null);
									ShowLogin ();
								});
							},
							onSucceed: () => {
								Device.BeginInvokeOnMainThread (() => {
									Debug.WriteLine ("LoginPage.DoLogin: Push MainMenu");
									Spinner.IsRunning = false;
									Navigation.PushModalAsync (new RayvNav (new MainMenu ()));
								});
							}, 
							onFailVersion: () => {
								Device.BeginInvokeOnMainThread (() => {
									Error.Text = "Wrong Server Version";
									ShowLogin ();
								});
							},
							incremental: false,
							setStatusMessage: SetProgress,
							timeoutMs: 30000
						);
					
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

		void SendResetEmail (string email)
		{
			var cparams = new Dictionary<string,string> ();
			cparams ["username"] = email;

			try {
				Persist.Instance.GetWebConnection ().post ("/forgot", cparams);
				DisplayAlert ("Password", "An email has been sent to your registered account", "OK");
			} catch (Exception ex) {
				Insights.Report (ex);
				DisplayAlert ("Password", "Unable to contact server - try later", "OK");
			}
		}

		void AskForEmail ()
		{
			// no password - ask for it
			var emailEd = new Entry {
				Placeholder = "Email Address"
			};
			var emailSend = new Button {
				Text = "Reset Password",
				BackgroundColor = ColorUtil.Lighter (settings.BaseColor),
				TextColor = ColorUtil.Darker (settings.BaseColor),
			};
			Content = new StackLayout {
				Children = {
					new LabelWide ("Enter your email address"),
					emailEd,
					emailSend
				}
			};
			emailSend.Clicked += (sender, ev) => {
				SendResetEmail (emailEd.Text);
			};
		}

		public LoginPage ()
		{
			Analytics.TrackPage ("LoginPage");
			Console.WriteLine ("LoginPage()");
			Spinner = new ActivityIndicator { Color = settings.BaseColor, };
			LoadingMessage = new Label { 
				Text = "Loading...",
				TextColor = settings.BaseColor,
				HorizontalOptions = LayoutOptions.Center,
				IsVisible = false,
			};
			progBar = new ProgressBar () { 
				HorizontalOptions = LayoutOptions.FillAndExpand, 
				IsVisible = false 
			};
			RayvButton loginButton = new RayvButton {
				Text = " Login ",
				FontSize = settings.FontSizeButtonLarge,
			};
			loginButton.Clicked += this.DoLogin;

			UserName = new Entry { 
				Placeholder = "Username",
				VerticalOptions = LayoutOptions.Start,
				Text = Persist.Instance.GetConfig (settings.USERNAME),
			};
			UserName.TextChanged += (sender, e) => {
				if (settings.TesterWhitelist.Contains (e.NewTextValue))
					Servers.IsVisible = true;
			};
			Password = new Entry {
				VerticalOptions = LayoutOptions.Start,
				Placeholder = "Password", 
				Text = Persist.Instance.GetConfig (settings.PASSWORD), 
			};
			RayvButton Register = new RayvButton {
				Text = "Sign Up", 
				BackgroundColor = ColorUtil.Lighter (settings.BaseColor), 
				TextColor = ColorUtil.Darker (settings.BaseColor),
			};
			Register.Clicked += (s, e) => {
				this.Navigation.PushModalAsync (
					new RayvNav (new RegisterPage ()), false);
			};
			RayvButton Reset = new RayvButton {
				Text = "Forgot Password", 
				BackgroundColor = ColorUtil.Lighter (settings.BaseColor), 
				TextColor = ColorUtil.Darker (settings.BaseColor),
			};
			Reset.Clicked += (s, e) => {
				var email = new UserProfile ().Email;
				if (string.IsNullOrEmpty (email)) {
					AskForEmail ();
				}
				;
				SendResetEmail (email);
			};
			Error = new Label {
				Text = "User Name & Password Don't match", 
				TextColor = Color.White, 
				BackgroundColor = Color.Red, 
				FontAttributes = FontAttributes.Bold,
				IsVisible = false,
				HeightRequest = 30,
				YAlign = TextAlignment.Center,
				XAlign = TextAlignment.Center,
			};
			Servers = new ServerPicker ();
			var user = Persist.Instance.GetConfig (settings.USERNAME);
			if (settings.TesterWhitelist.Contains (user))
				Servers.IsVisible = true;
			this.Content = new StackLayout {
				Padding = 20,
				Children = {
					new Label {
						Text = "Login",
						VerticalOptions = LayoutOptions.Start,
						HorizontalOptions = LayoutOptions.CenterAndExpand,
					},
					Servers,
					UserName,
					Password,
					Error,
					Spinner,
					LoadingMessage,
					progBar,
					loginButton,
					Reset,
					Register,
				}
			};

		}

		public LoginPage (string message) : this ()
		{
			Error.Text = message;
			Error.IsVisible = true;
		}
	}
}

