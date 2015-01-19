using System;

using CoreLocation;
using UIKit;
using Foundation;


namespace RayvMobileApp.iOS
{
	public class LocationManager
	{
		CLLocationManager locMgr;

		// event for the location changing
		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated = delegate {};

		public LocationManager ()
		{
			if (locMgr == null) {
				locMgr = new CLLocationManager ();
				locMgr.RequestWhenInUseAuthorization ();
				Console.WriteLine ("LocationManager RequestWhenInUseAuthorization");
			}
			LocationUpdated += PrintLocation;
		}

		//		// create a location manager to get system location updates to the application
		//		public CLLocationManager LocMgr
		//		{
		//			get {
		//				return this.locMgr;
		//			}
		//		} protected CLLocationManager locMgr;

		public void DoLocationUpdateIos6 (object sender, CLLocationsUpdatedEventArgs e)
		{
			// fire our custom Location Updated event
			this.LocationUpdated (this, new LocationUpdatedEventArgs (e.Locations [e.Locations.Length - 1]));
		}

		public void DoLocationUpdateIos7Plus (object sender, CLLocationUpdatedEventArgs e)
		{
			this.LocationUpdated (this, new LocationUpdatedEventArgs (e.NewLocation));
		}

		public void StopUpdatingLocation ()
		{
			if (locMgr != null)
				locMgr.StopUpdatingLocation ();
		}

		public void StartLocationUpdates ()
		{
			// We need the user's permission for our app to use the GPS in iOS. This is done either by the user accepting
			// the popover when the app is first launched, or by changing the permissions for the app in Settings

			if (CLLocationManager.LocationServicesEnabled) {
				if (locMgr == null)
					return;

				locMgr.DesiredAccuracy = 1; // sets the accuracy that we want in meters

				// Location updates are handled differently pre-iOS 6. If we want to support older versions of iOS,
				// we want to do perform this check and let our LocationManager know how to handle location updates.

				if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) {
					locMgr.LocationsUpdated += DoLocationUpdateIos6;
				} else {
					// this won't be called on iOS 6 (deprecated). We will get a warning here when we build.
					locMgr.UpdatedLocation += DoLocationUpdateIos7Plus;
				}
				// Start our location updates
				locMgr.StartUpdatingLocation ();

				// Get some output from our manager in case of failure
				locMgr.Failed += (object sender, NSErrorEventArgs e) => {
					Console.WriteLine (e.Error);
				}; 
			} else {
				//Let the user know that they need to enable LocationServices
				Console.WriteLine ("Location services not enabled, please enable this in your Settings");
			}
		}

		//This will keep going in the background and the foreground
		public void PrintLocation (object sender, LocationUpdatedEventArgs e)
		{
			CLLocation location = e.Location;
		}

	}
}