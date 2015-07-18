using System;
using RayvMobileApp.iOS;
using UIKit;
using Foundation;

[assembly: Xamarin.Forms.Dependency (typeof(ShareIntent))]
namespace RayvMobileApp.iOS
{
	public class ShareIntent: IShareable
	{
		public ShareIntent ()
		{
		}

		public void OpenShareIntent (string textToShare)
		{
			try {
				var window = UIApplication.SharedApplication.KeyWindow;
				var vc = window.RootViewController;
				while (vc.PresentedViewController != null) {
					vc = vc.PresentedViewController;
				}
				var activityController = new UIActivityViewController (new NSObject[] { 
					UIActivity.FromObject (textToShare) 
				}, null);

				vc.PresentViewController (
					activityController, true, null);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
	}
}

