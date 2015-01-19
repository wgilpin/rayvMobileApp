using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreLocation;


namespace RayvMobileApp.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public static LocationManager locationMgr;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

			LoadApplication (new App ());


			return base.FinishedLaunching (app, options);
		}

		public override void DidEnterBackground (UIApplication application)
		{
			Console.WriteLine ("App entering background state.");
			locationMgr.StopUpdatingLocation ();
		}

		public override void WillEnterForeground (UIApplication application)
		{
			Console.WriteLine ("App will enter foreground");
			locationMgr.StartLocationUpdates ();
		}
	}
}

