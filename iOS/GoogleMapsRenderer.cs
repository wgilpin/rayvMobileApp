using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Google.Maps;
using System.Drawing;
using Xamarin;
using CoreGraphics;


[assembly:ExportRenderer (typeof(RayvMobileApp.iOS.MapGooglePage), typeof(RayvMobileApp.iOS.GoogleMapsRenderer))]
namespace RayvMobileApp.iOS
{
	public class GoogleMapsRenderer: PageRenderer
	{
		MapView mapView;

		public GoogleMapsRenderer ()
		{
		}

		public override void LoadView ()
		{
			try {
				base.LoadView ();
				
				var camera = CameraPosition.FromCamera (-33.868, 151.2086, 6);
				mapView = MapView.FromCamera (CGRect.Empty, camera);
				mapView.Settings.CompassButton = true;
				mapView.Settings.MyLocationButton = true;
				mapView.MyLocationEnabled = true;
				View = mapView;
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			mapView.StartRendering ();
		}

		public override void ViewWillDisappear (bool animated)
		{   
			mapView.StopRendering ();
			base.ViewWillDisappear (animated);
		}
	}
}