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
		RayvButton loginButton;
		RayvButton Register;
		RayvButton Reset;

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
			loginButton.IsEnabled = false;
			progBar.IsVisible = true;
			// remove leading & trailing whitespace #780
			UserName.Text = UserName.Text.Trim ();
			Password.Text = Password.Text.Trim ();
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				if (string.IsNullOrEmpty (Persist.Instance.GetConfig (settings.SERVER)))
					Persist.Instance.SetConfig (settings.SERVER,  $"https://{settings.SERVER_DEFAULT}");
				
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
									loginButton.IsEnabled = true;
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
									loginButton.IsEnabled = true;
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
							loginButton.IsEnabled = true;
						});
					}
				} catch (ProtocolViolationException) {
					Device.BeginInvokeOnMainThread (() => {
						DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
						Spinner.IsRunning = false;
						loginButton.IsEnabled = true;
					});
				} catch (UnauthorizedAccessException) {
					Device.BeginInvokeOnMainThread (() => {
						Error.IsVisible = true;
						Spinner.IsRunning = false;
						loginButton.IsEnabled = true;
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
				DisplayAlert ("Password", $"An email has been sent to {email}", "OK");
			} catch (Exception ex) {
				Insights.Report (ex);
				DisplayAlert ("Password", "Unable to contact server - try later", "OK");
			}
		}

		void AskForEmail (string email)
		{
			// no password - ask for it
			var emailEd = new Entry {
				Placeholder = "Email Address",
				Text = email
			};
			var emailSend = new DoubleButton {
				LeftText = "Cancel",
				RightText = "Reset",
				LeftSource = "back_1.png",
				RightSource = "forward_1.png"
			};
			var inner = new StackLayout {
				Padding = new Thickness (2),
				Spacing = 15,
				BackgroundColor = Color.White,
				VerticalOptions = LayoutOptions.StartAndExpand,
				Children = {
					new LabelWide ("Enter your email address"),
					emailEd,
					emailSend
				}
			};
			Content = new StackLayout {
				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				BackgroundColor = settings.BaseColor,
				VerticalOptions = LayoutOptions.StartAndExpand,
				Children = {
					new LabelWide ("Reset Password") {
						FontSize = settings.FontSizeLabelLarge,
						TextColor = Color.White,
						XAlign = TextAlignment.Center,
					},
					inner,
				}
			};
			emailSend.RightClick = (sender, ev) => {
				SendResetEmail (emailEd.Text);
			};
			emailSend.LeftClick = (sender, ev) => {
				ShowLoginView ();
			};
		}

		void ShowLoginView ()
		{
			Servers = new ServerPicker ();
			var user = Persist.Instance.GetConfig (settings.USERNAME);
			if (settings.TesterWhitelist.Contains (user))
				Servers.IsVisible = true;
			Content = new StackLayout {
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
			loginButton = new RayvButton {
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
			Register = new RayvButton {
				Text = "Sign Up", 
				BackgroundColor = ColorUtil.Lighter (settings.BaseColor), 
				TextColor = ColorUtil.Darker (settings.BaseColor),
			};
			Register.Clicked += (s, e) => {
				this.Navigation.PushModalAsync (
					new RayvNav (new RegisterPage ()), false);
			};
			Reset = new RayvButton {
				Text = "Forgot Password", 
				BackgroundColor = ColorUtil.Lighter (settings.BaseColor), 
				TextColor = ColorUtil.Darker (settings.BaseColor),
			};
			Reset.Clicked += (s, e) => {
				var email = new UserProfile ().Email;
				AskForEmail (email);
			};
			Error = new Label {
				Text = "User Name & Password Don't match", 
				TextColor = Color.Red, 
				FontAttributes = FontAttributes.Bold,
				IsVisible = false,
				HeightRequest = 30,
				YAlign = TextAlignment.Center,
				XAlign = TextAlignment.Center,
			};
			ShowLoginView ();

		}

		public LoginPage (string message) : this ()
		{
			Error.Text = message;
			Error.IsVisible = true;
		}
	}
}

