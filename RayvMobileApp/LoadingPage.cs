using System;

using Xamarin.Forms;
using System.ComponentModel;
using System.Net;

namespace RayvMobileApp
{
	public class LoadingPage : ContentPage
	{
		Label LoadingMessage;
		Label ServerMessage;
		ProgressBar progBar;

		void loadDataFromServer ()
		{
			Console.WriteLine ("loadDataFromServer");
			Persist.Instance.LoadFromDb (loader: this);
			Console.WriteLine ("loadDataFromServer");
			if (Persist.Instance.Online) {
				Persist.Instance.GetUserData (
					onFail: () => {
						Navigation.PushModalAsync (new LoginPage ());
					}, 
					onSucceed: () => {
						Persist.Instance.Online = true;
						Navigation.PushModalAsync (new MainMenu ());
					},
					incremental: true, 
					statusMessage: SetMessage);
				Persist.Instance.LoadCategories ();
			} else
				Navigation.PushModalAsync (new LoginPage ());
		}

		void DoAppearing (object sender, EventArgs e)
		{
			progBar.WidthRequest = this.Width;
			try {
				loadDataFromServer ();
			} catch (ProtocolViolationException ex) {
				Console.WriteLine ("loadDataFromServer: WRONG SERVER VERSION {0}", ex);
				Navigation.PushModalAsync (new LoginPage ());
			} catch (UnauthorizedAccessException) {
				Console.WriteLine ("WorkerCompleted: Offline");
				Navigation.PushModalAsync (new LoginPage ());
			}
		}

		public void SetMessage (string message, Double progress)
		{
			Device.BeginInvokeOnMainThread (() => {
				LoadingMessage.Text = message;
				Console.WriteLine ("Loading message: {0}", message);
				progBar.ProgressTo (progress, 250, Easing.Linear);
			});
		}

		public LoadingPage ()
		{
			LoadingMessage = new Label { 
				Text = "Loading...",
				TextColor = Color.White,
				HorizontalOptions = LayoutOptions.Center,
			};
			progBar = new ProgressBar (){ HorizontalOptions = LayoutOptions.CenterAndExpand };
			ServerMessage = new Label { 
				Text = "",
				TextColor = ColorUtil.Lighter (Color.Red),
				HorizontalOptions = LayoutOptions.Center,
			};
			string server = Persist.Instance.GetConfig (settings.SERVER);
			if (!server.Contains (settings.DEFAULT_SERVER)) {
				// we are not on prod
				ServerMessage.Text = server;
			}

			BackgroundColor = settings.BaseColor;
			Content = new StackLayout { 
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new ActivityIndicator { IsRunning = true, Color = Color.White, },
					LoadingMessage,
					progBar,
					ServerMessage,
				}
			};
			Appearing += DoAppearing;
		}
	}
}


