using System;
using Foundation;

using RayvMobileApp.iOS;

[assembly: Xamarin.Forms.Dependency (typeof(AppDataIos))]

namespace RayvMobileApp.iOS
{
	public class AppDataIos: IAppData
	{
		public string AppVersion ()
		{
			return string.Format (
				"{0} (Build {1})", 
				NSBundle.MainBundle.InfoDictionary ["CFBundleShortVersionString"],
				NSBundle.MainBundle.InfoDictionary ["CFBundleVersion"]);
		}

		public string AppMajorVersion ()
		{
			return NSBundle.MainBundle.InfoDictionary ["CFBundleShortVersionString"].ToString ();
		}

		public AppDataIos ()
		{
		}
	}
}

