using System;

using Xamarin.Forms;
using Xamarin;

namespace RayvMobileApp
{
	public class App : Xamarin.Forms.Application
	{
		public static ILocationManager locationMgr;

		public static void IdentifyToAnalytics ()
		{
			try {
				String user = Persist.Instance.GetConfig (settings.USERNAME);
				Insights.Identify (user, "server", Persist.Instance.GetConfig (settings.SERVER));
				Console.WriteLine ("AppDelegate Analytics ID: {0}", user);
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

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
			Persist.Instance.UpdateSchema ();
			MainPage = GetFirstPage ();
		}

		protected override void OnStart ()
		{
			App.locationMgr.StartLocationUpdates ();
			App.locationMgr.AddLocationUpdateHandler (MainMenu.HandleLocationChanged);
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
			Console.WriteLine ("App entering background state.");
			App.locationMgr.StopUpdatingLocation ();
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
			Console.WriteLine ("App Resumed");
			App.locationMgr.StartLocationUpdates ();
			IdentifyToAnalytics ();
			Resumed (this, null);
		}

		public static event EventHandler Resumed = delegate {};
	}
}

