using System;

using Xamarin.Forms;
using System.ComponentModel;

namespace RayvMobileApp
{
	public class LoadingPage : ContentPage
	{
		BackgroundWorker worker;

		private void WorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			Persist.Instance.Online = (e.Error != null);
			Console.WriteLine ("Online: {0}", Persist.Instance.Online);
			Navigation.PushModalAsync (new MainMenu ());
		}

		void loadDataFromServer (object sender, DoWorkEventArgs e)
		{
			Console.WriteLine ("loadDataFromServer");
			Persist.Instance.LoadFromDb ();
			Console.WriteLine ("loadDataFromServer");
			Persist.Instance.GetUserData (this, incremental: true);
		}

		void DoAppearing (object sender, EventArgs e)
		{
			worker.DoWork += 
				new DoWorkEventHandler (loadDataFromServer);
			worker.RunWorkerCompleted += 
				new RunWorkerCompletedEventHandler (WorkerCompleted);
			worker.RunWorkerAsync ();
		}


		public LoadingPage ()
		{
			BackgroundColor = settings.ColorLight;
			Content = new StackLayout { 
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new ActivityIndicator { IsRunning = true, Color = Color.Red, }
				}
			};
			worker = new BackgroundWorker ();
			Appearing += DoAppearing;
		}
	}
}


