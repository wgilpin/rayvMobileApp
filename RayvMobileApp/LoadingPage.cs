using System;

using Xamarin.Forms;
using System.ComponentModel;
using System.Net;

namespace RayvMobileApp
{
	public class LoadingPage : ContentPage
	{
		Label ServerMessage;
		Label LoadingMessage;
		ProgressBar progBar;

		private void WorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
//			Persist.Instance.Online = (e.Error != null);
//			try {
			//				Console.WriteLine ($"WorkerCompleted: Online: {Persist.Instance.Online}");
			Console.WriteLine ("WorkerCompleted");
//			} catch (UnauthorizedAccessException) {
//				Console.WriteLine ("WorkerCompleted: Offline");
//				Navigation.PushModalAsync (new LoginPage ());
//			}
		}

		void loadDataFromServer ()
		{
			Console.WriteLine ("loadDataFromServer");
			if (Persist.Instance.Online) {
				SetMessage ("Connecting", 0.5);
				Persist.Instance.GetUserData (
					onFail: () => {
						Console.WriteLine ("loadDataFromServer: Fail");
//						Device.BeginInvokeOnMainThread (() => {
//							Navigation.PushModalAsync (new MainMenu ());
//						});
					}, 
					onFailLogin: () => {
						Console.WriteLine ("loadDataFromServer: Fail Login");

						Device.BeginInvokeOnMainThread (() => {
							var login = new LoginPage ("Login Failed");
							Navigation.PushModalAsync (login);
						});
					}, 
					onSucceed: () => {
						Console.WriteLine ("loadDataFromServer: Success");
						Device.BeginInvokeOnMainThread (() => {
							Persist.Instance.Online = true;
//							Navigation.PushModalAsync (new MainMenu ());
							Persist.Instance.LoadCategories ();
						});
					},
					onFailVersion: () => {
						Console.WriteLine ("loadDataFromServer: Fail Version");
						Device.BeginInvokeOnMainThread (() => {
							var login = new LoginPage ("Wrong Server Version");
							Navigation.PushModalAsync (login);
							Persist.Instance.SetConfig (settings.SERVER, null);
						});
					}, 
					incremental: true, 
					setStatusMessage: SetMessage);
				
			} else
				Device.BeginInvokeOnMainThread (() => {
					if (string.IsNullOrEmpty (Persist.Instance.GetConfig (settings.PASSWORD))) {
						var login = new LoginPage ();
						Navigation.PushModalAsync (login);
					} else {
						DisplayAlert ("Offline", "Unable to contact server", "OK");
						Navigation.PushModalAsync (new RayvNav (new MainMenu ()));
					}
				});
		}

		void DoAppearing (object sender, EventArgs e)
		{
			progBar.WidthRequest = this.Width;
			Persist.Instance.LoadFromDb (loader: this);
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				loadDataFromServer ();
			})).Start ();
			Navigation.PushModalAsync (new RayvNav (new MainMenu ()));
//
//
//
//			try {
//				loadDataFromServer ();
//			} catch (ProtocolViolationException ex) {
//				Console.WriteLine ("loadDataFromServer: WRONG SERVER VERSION {0}", ex);
//				Navigation.PushModalAsync (new LoginPage ());
//			} catch (UnauthorizedAccessException) {
//				Console.WriteLine ("WorkerCompleted: Offline");
//				Navigation.PushModalAsync (new LoginPage ());
//			}
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
			progBar = new ProgressBar (){ HorizontalOptions = LayoutOptions.FillAndExpand };
			ServerMessage = new Label { 
				Text = "",
				TextColor = ColorUtil.Lighter (Color.Red),
				HorizontalOptions = LayoutOptions.Center,
			};
			string server = Persist.Instance.GetConfig (settings.SERVER);
			if (!server.Contains (settings.SERVER_DEFAULT)) {
				// we are not on prod
				ServerMessage.Text = server;
			}
			BackgroundColor = settings.BaseColor;
			Padding = 30;
			Appearing += DoAppearing;
		}
	}
}


