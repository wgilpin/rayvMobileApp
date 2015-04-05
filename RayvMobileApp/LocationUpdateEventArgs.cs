using System;
using Xamarin.Forms.Maps;


namespace RayvMobileApp
{
	public class LocationUpdatedEventArgs : EventArgs
	{
		Position location;

		public LocationUpdatedEventArgs (Position location)
		{
			this.location = location;
		}

		public Position Location {
			get { return location; }
		}
	}
}

