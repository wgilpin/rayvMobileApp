using System;

using Xamarin.Forms;

namespace RayvMobileApp
{
	public class App : Xamarin.Forms.Application
	{
		public static ILocationManager locationMgr;

		public static Page GetFirstPage (bool SkipIntro = false)
		{
			// The root page of your application
			if (SkipIntro || Persist.Instance.GetConfigBool (settings.SKIP_INTRO)) {
				if (Persist.Instance.GetConfig (settings.PASSWORD).Length * Persist.Instance.GetConfig (settings.USERNAME).Length * Persist.Instance.GetConfig (settings.SERVER).Length > 0) {
					return new LoadingPage ();
				}
				return new LoginPage ();
			} else
				return new IntroPage ();
		}

		public App ()
		{
			locationMgr = DependencyService.Get<ILocationManager> ();
			MainPage = GetFirstPage ();
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

