using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreLocation;
using Xamarin;
using Google.Maps;
using Xamarin.Forms;

//using HockeyApp;
using System.Threading.Tasks;


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
			global::Xamarin.FormsMaps.Init ();

			Insights.Initialize ("87e54cc1294cb314ce9f25d029a942aa7fc7dfd4");
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				MapServices.ProvideAPIKey ("AIzaSyBZ5j4RR4ymfrckCBKkgeNylfoWoRSD3yQ");
				IdentifyToAnalytics ();
			})).Start ();

//			//We MUST wrap our setup in this block to wire up
//			// Mono's SIGSEGV and SIGBUS signals
//			HockeyApp.Setup.EnableCustomCrashReporting (() => {
//
//				//Get the shared instance
//				var manager = BITHockeyManager.SharedHockeyManager;
//
//				//Configure it to use our APP_ID
//				manager.Configure ("97b718534f201b0baa7e6b9763cb875f");
//
//				//Start the manager
//				manager.StartManager ();
//
//				//Authenticate (there are other authentication options)
//				manager.Authenticator.AuthenticateInstallation ();
//
//				//Rethrow any unhandled .NET exceptions as native iOS 
//				// exceptions so the stack traces appear nicely in HockeyApp
//				AppDomain.CurrentDomain.UnhandledException += (sender, e) => 
//					Setup.ThrowExceptionAsNative (e.ExceptionObject);
//
//				TaskScheduler.UnobservedTaskException += (sender, e) => 
//					Setup.ThrowExceptionAsNative (e.Exception);
//			});


			//The rest of your code here
			// ...

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

