using System;

using CoreLocation;
using UIKit;
using Foundation;
using Xamarin.Forms.Maps;
using RayvMobileApp.iOS;
using Xamarin;

[assembly: Xamarin.Forms.Dependency (typeof(LocationManagerIos))]

namespace RayvMobileApp.iOS
{
	public class LocationManagerIos : ILocationManager
	{
		CLLocationManager locMgr;

		// event for the location changing
		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated = delegate {};

		public void AddLocationUpdateHandler (EventHandler<LocationUpdatedEventArgs> handler)
		{
			try {
				LocationUpdated += handler;
			} catch (Exception ex) {
				Console.WriteLine ("AddLocationUpdateHandler: {0}", ex);
			}
		}

		public void RemoveLocationUpdateHandler (EventHandler<LocationUpdatedEventArgs> handler)
		{
			try {
				LocationUpdated -= handler;
			} catch (Exception ex) {
				Console.WriteLine ("RemoveLocationUpdateHandler: {0}", ex);
			}
		}


		public LocationManagerIos ()
		{
			if (locMgr == null) {
				locMgr = new CLLocationManager ();
				locMgr.RequestWhenInUseAuthorization ();
				Console.WriteLine ("LocationManager RequestWhenInUseAuthorization");
			}
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
			Position pos = new Position (
				               e.Locations [e.Locations.Length - 1].Coordinate.Latitude, 
				               e.Locations [e.Locations.Length - 1].Coordinate.Longitude); 
			this.LocationUpdated (this, new LocationUpdatedEventArgs (pos));
		}

		public void DoLocationUpdateIos7Plus (object sender, CLLocationUpdatedEventArgs e)
		{
			Position pos = new Position (
				               e.NewLocation.Coordinate.Latitude, 
				               e.NewLocation.Coordinate.Longitude); 
			this.LocationUpdated (this, new LocationUpdatedEventArgs (pos));
		}

		public void StopUpdatingLocation ()
		{
			if (locMgr != null) {
				locMgr.StopUpdatingLocation ();

			}
		}

		public void StartLocationUpdates ()
		{
			// We need the user's permission for our app to use the GPS in iOS. This is done either by the user accepting
			// the popover when the app is first launched, or by changing the permissions for the app in Settings

			if (CLLocationManager.LocationServicesEnabled) {
				if (locMgr == null) {
					Insights.Track ("Location Start Update FAIL");
					return;
				}
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
		public void PrintLocation (object sender, LocationUpdateEventArgsIos e)
		{
			CLLocation location = e.Location;
		}

	}
}