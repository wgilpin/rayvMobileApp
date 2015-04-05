using System;

using Xamarin.Forms;

namespace RayvMobileApp
{
	public class App : Xamarin.Forms.Application
	{
		public static ILocationManager locationMgr;

		public App ()
		{
			locationMgr = DependencyService.Get<ILocationManager> ();
			// The root page of your application
			if (Persist.Instance.GetConfig (settings.PASSWORD).Length *
			    Persist.Instance.GetConfig (settings.USERNAME).Length *
			    Persist.Instance.GetConfig (settings.SERVER).Length > 0) {

				MainPage = new LoadingPage ();
				return;
			}
			MainPage = new LoginPage ();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

