using System;
using Foundation;
using UIKit;

using RayvMobileApp.iOS;

[assembly: Xamarin.Forms.Dependency (typeof(DeviceSpecificIos))]

namespace RayvMobileApp.iOS
{
	public class DeviceSpecificIos: IDeviceSpecific
	{
		public bool MakeCall (string phoneNumber)
		{
			var urlToSend = new NSUrl ("tel:" + phoneNumber); // phonenum is in the format 1231231234
			
			if (UIApplication.SharedApplication.CanOpenUrl (urlToSend)) {
				Console.WriteLine ("DoMakeCall: calling {0}", phoneNumber);
				UIApplication.SharedApplication.OpenUrl (urlToSend);
				return true;
			} else {
				// Url is not able to be opened.
				return false;
			}
		}

		public bool RunningOnIosSimulator ()
		{
			return ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR;
		}

		public DeviceSpecificIos ()
		{
		}
	}
}

