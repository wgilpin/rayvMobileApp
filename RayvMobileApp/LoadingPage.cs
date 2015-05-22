using System;

using Xamarin.Forms;
using System.ComponentModel;
using System.Net;

namespace RayvMobileApp
{
	public class LoadingPage : ContentPage
	{
		BackgroundWorker worker;
		Label LoadingMessage;
		Label ServerMessage;
		ProgressBar progBar;

		private void WorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			Persist.Instance.Online = (e.Error != null);
			Console.WriteLine ("Online: {0}", Persist.Instance.Online);
			Navigation.PushModalAsync (new MainMenu ());
		}

		void loadDataFromServer (object sender, DoWorkEventArgs e)
		{
			Console.WriteLine ("loadDataFromServer");
			Persist.Instance.LoadFromDb (loader: this);
			Console.WriteLine ("loadDataFromServer");
			try {
				Persist.Instance.GetUserData (this, incremental: true, loader: this);
			} catch (ProtocolViolationException ex) {
				Console.WriteLine ("loadDataFromServer: WRONG SERVER VERSION {0}", ex);
			}
		}

		void DoAppearing (object sender, EventArgs e)
		{
			progBar.WidthRequest = this.Width;
			worker.DoWork += 
				new DoWorkEventHandler (loadDataFromServer);
			worker.RunWorkerCompleted += 
				new RunWorkerCompletedEventHandler (WorkerCompleted);
			worker.RunWorkerAsync ();
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
			worker = new BackgroundWorker ();
			Appearing += DoAppearing;
		}
	}
}


