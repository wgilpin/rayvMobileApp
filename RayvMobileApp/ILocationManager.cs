using System;

namespace RayvMobileApp
{
	public interface ILocationManager
	{
		// event for the location changing
		void SetLocationUpdateHandler (EventHandler<LocationUpdatedEventArgs> handler);

		void StopUpdatingLocation ();

		void StartLocationUpdates ();

	}
}

