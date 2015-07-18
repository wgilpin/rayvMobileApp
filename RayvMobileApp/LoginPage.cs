﻿using System;
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


		string[] TesterWhitelist = { "Will", "pegah", "georgia" };

		public void SetProgress (string message, Double progress)
		{
			Device.BeginInvokeOnMainThread (() => {
				LoadingMessage.Text = message;
				Console.WriteLine ("Loading message: {0}", message);
				progBar.ProgressTo (progress, 250, Easing.Linear);
			});
		}

		void DoLogin (object sender, EventArgs e)
		{
			Spinner.IsRunning = true;
			LoadingMessage.IsVisible = true;
			progBar.IsVisible = true;
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
									LoadingMessage.IsVisible = true;
									progBar.IsVisible = true;
								});
							},
							onSucceed: () => {
								Device.BeginInvokeOnMainThread (() => {
									Debug.WriteLine ("LoginPage.DoLogin: Push MainMenu");
									Spinner.IsRunning = false;
									this.Navigation.PushModalAsync (new MainMenu ());
								});
							}, 
							incremental: false,
							statusMessage: SetProgress);
					
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
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button)),
			};
			loginButton.Clicked += this.DoLogin;

			UserName = new Entry { 
				Placeholder = "Username",
				VerticalOptions = LayoutOptions.Start,
				Text = Persist.Instance.GetConfig (settings.USERNAME),
			};
			UserName.TextChanged += (sender, e) => {
				if (TesterWhitelist.Contains (e.NewTextValue))
					Servers.IsVisible = true;
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
					new RayvNav (new RegisterPage ()), false);
			};
			Error = new Label {
				Text = "User Name & Password Don't match", 
				TextColor = Color.White, 
				BackgroundColor = Color.Red, 
				FontAttributes = FontAttributes.Bold,
				IsVisible = false,
			};
			Servers = new ServerPicker ();
			var user = Persist.Instance.GetConfig (settings.USERNAME);
			if (TesterWhitelist.Contains (user))
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
					Register,
				}
			};

		}
	}
}

