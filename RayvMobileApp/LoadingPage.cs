using System;

using Xamarin.Forms;
using System.ComponentModel;

namespace RayvMobileApp
{
	public class LoadingPage : ContentPage
	{
		BackgroundWorker worker;
		Label LoadingMessage;
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
			Persist.Instance.GetUserData (this, incremental: true, loader: this);
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

			BackgroundColor = settings.BaseColor;
			Content = new StackLayout { 
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new ActivityIndicator { IsRunning = true, Color = Color.White, },
					LoadingMessage,
					progBar,
				}
			};
			worker = new BackgroundWorker ();
			Appearing += DoAppearing;
		}
	}
}


