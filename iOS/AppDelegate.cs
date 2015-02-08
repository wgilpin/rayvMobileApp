using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreLocation;
using Xamarin;


namespace RayvMobileApp.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public static LocationManager locationMgr;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();
			Insights.Initialize ("87e54cc1294cb314ce9f25d029a942aa7fc7dfd4");
			try {
				String user = Persist.Instance.GetConfig ("username");
				Insights.Identify (user, "email", "user");
			} catch (Exception ex) {
				Insights.Report (ex);
			}

			LoadApplication (new App ());


			return base.FinishedLaunching (app, options);
		}

		public override void DidEnterBackground (UIApplication application)
		{
			Console.WriteLine ("App entering background state.");
			locationMgr.StopUpdatingLocation ();
			Insights.Track ("AppDelegate.DidEnterBackground");
		}

		public override void WillEnterForeground (UIApplication application)
		{
			Console.WriteLine ("App will enter foreground");
			locationMgr.StartLocationUpdates ();
			Insights.Track ("AppDelegate.WillEnterForeground");
		}
	}
}

