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
using PushNotification.Plugin;


namespace RayvMobileApp.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		


		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
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
			// Handle when your app starts
			global::Xamarin.Forms.Forms.Init ();
			global::Xamarin.FormsMaps.Init ();

			//The rest of your code here
			// ...
			Insights.Initialize ("87e54cc1294cb314ce9f25d029a942aa7fc7dfd4");
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				//				MapServices.ProvideAPIKey ("AIzaSyBZ5j4RR4ymfrckCBKkgeNylfoWoRSD3yQ");
				App.IdentifyToAnalytics ();
			})).Start ();

			// no crash reporting if on the ios simulator
			bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();
			Insights.DisableCollection = DEBUG_ON_SIMULATOR;

			//Consider inizializing before application initialization, if using any CrossPushNotification method during application initialization.
			CrossPushNotification.Initialize<PushNotificationsListener> ();
			CrossPushNotification.Current.Register ();
			//...
			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}

		public override void DidEnterBackground (UIApplication application)
		{
			
		}

		public override void WillEnterForeground (UIApplication application)
		{
			
		}

		public override void FailedToRegisterForRemoteNotifications (UIApplication application, NSError error)
		{
			if (CrossPushNotification.Current is IPushNotificationHandler) {
				((IPushNotificationHandler)CrossPushNotification.Current).OnErrorReceived (error);
			}
		}

		public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
		{
			if (CrossPushNotification.Current is IPushNotificationHandler) {
				((IPushNotificationHandler)CrossPushNotification.Current).OnRegisteredSuccess (deviceToken);
			}
		}

		public override void DidRegisterUserNotificationSettings (UIApplication application,
		                                                          UIUserNotificationSettings notificationSettings)
		{
			application.RegisterForRemoteNotifications ();
		}

		public  void DidReceiveRemoteNotification (UIApplication application,
		                                           NSDictionary userInfo,
		                                           Action completionHandler)
		{
			if (CrossPushNotification.Current is IPushNotificationHandler) {
				((IPushNotificationHandler)CrossPushNotification.Current).OnMessageReceived (userInfo);
			}
		}

		public override void ReceivedRemoteNotification (UIApplication application, NSDictionary userInfo)
		{ 
			if (CrossPushNotification.Current is IPushNotificationHandler) {
				((IPushNotificationHandler)CrossPushNotification.Current).OnMessageReceived (userInfo);
			}
		}
	}
}

