﻿using System;
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


			//The rest of your code here
			// ...

			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}

		public override void DidEnterBackground (UIApplication application)
		{
			
		}

		public override void WillEnterForeground (UIApplication application)
		{
			
		}
	}
}

