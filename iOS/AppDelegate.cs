using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreLocation;
using Xamarin;
using Google.Maps;
using Xamarin.Forms;


namespace RayvMobileApp.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		
		private void IdentifyToAnalytics ()
		{
			try {
				String user = Persist.Instance.GetConfig (settings.USERNAME);
				Insights.Identify (user, "server", Persist.Instance.GetConfig (settings.SERVER));
				Console.WriteLine ("AppDelegate Analytics ID: {0}", user);
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

			Insights.Initialize ("87e54cc1294cb314ce9f25d029a942aa7fc7dfd4");
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				MapServices.ProvideAPIKey ("AIzaSyBZ5j4RR4ymfrckCBKkgeNylfoWoRSD3yQ");
				IdentifyToAnalytics ();
			})).Start ();

			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}

		public override void DidEnterBackground (UIApplication application)
		{
			Console.WriteLine ("App entering background state.");
			App.locationMgr.StopUpdatingLocation ();
//			Insights.Track ("AppDelegate.DidEnterBackground");
		}

		public override void WillEnterForeground (UIApplication application)
		{
			Console.WriteLine ("App will enter foreground");
			App.locationMgr.StartLocationUpdates ();
			IdentifyToAnalytics ();
//			Insights.Track ("AppDelegate.WillEnterForeground");
		}
	}
}

