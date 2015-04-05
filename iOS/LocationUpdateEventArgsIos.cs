using System;
using CoreLocation;

namespace RayvMobileApp.iOS
{
	public class LocationUpdateEventArgsIos
	{
		CLLocation location;

		public LocationUpdateEventArgsIos (CLLocation location)
		{
			this.location = location;
		}

		public CLLocation Location {
			get { return location; }
		}
	}
}
	