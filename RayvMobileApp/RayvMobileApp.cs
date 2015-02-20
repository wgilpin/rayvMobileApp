using System;

using Xamarin.Forms;
using RayvMobileApp.iOS;

namespace RayvMobileApp
{
	public class App : Xamarin.Forms.Application
	{
		public App ()
		{
			// The root page of your application
			if (Persist.Instance.GetConfig (settings.PASSWORD).Length *
			    Persist.Instance.GetConfig (settings.USERNAME).Length *
			    Persist.Instance.GetConfig (settings.SERVER).Length > 0) {
				MainPage = MainMenu.Instance;
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

