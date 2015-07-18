using System;

using Xamarin.Forms;
using System.ComponentModel;
using System.Net;

namespace RayvMobileApp
{
	public class LoadingPage : ContentPage
	{
		BackgroundWorker worker;
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

		void loadDataFromServer (object sender, DoWorkEventArgs e)
		{
			Console.WriteLine ("loadDataFromServer");
			Persist.Instance.LoadFromDb (loader: this);
			Console.WriteLine ("loadDataFromServer");
			if (Persist.Instance.Online) {
				SetMessage ("Connecting", 0.5);
				Persist.Instance.GetUserData (
					onFail: () => {
						Device.BeginInvokeOnMainThread (() => {
							if (string.IsNullOrEmpty (Persist.Instance.GetConfig (settings.PASSWORD)))
								Navigation.PushModalAsync (new LoginPage ());
							else {
								//DisplayAlert ("Offline", "Unable to contact server", "OK");
								Navigation.PushModalAsync (new MainMenu ());
							}
						});
					}, 
					onSucceed: () => {
						Device.BeginInvokeOnMainThread (() => {
							Persist.Instance.Online = true;
							Navigation.PushModalAsync (new MainMenu ());
							Persist.Instance.LoadCategories ();
						});
					},
					incremental: true, 
					statusMessage: SetMessage);
				
			} else
				Device.BeginInvokeOnMainThread (() => {
					if (string.IsNullOrEmpty (Persist.Instance.GetConfig (settings.PASSWORD)))
						Navigation.PushModalAsync (new LoginPage ());
					else {
						DisplayAlert ("Offline", "Unable to contact server", "OK");
						Navigation.PushModalAsync (new MainMenu ());
					}
				});
		}

		void DoAppearing (object sender, EventArgs e)
		{
			progBar.WidthRequest = this.Width;
			worker.DoWork += 
				new DoWorkEventHandler (loadDataFromServer);
			worker.RunWorkerCompleted += 
				new RunWorkerCompletedEventHandler (WorkerCompleted);
			worker.RunWorkerAsync ();
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
			worker = new BackgroundWorker ();
			Appearing += DoAppearing;
		}
	}
}


