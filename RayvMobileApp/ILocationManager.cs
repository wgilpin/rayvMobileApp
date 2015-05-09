using System;

namespace RayvMobileApp
{
	public interface ILocationManager
	{
		// event for the location changing
		void AddLocationUpdateHandler (EventHandler<LocationUpdatedEventArgs> handler);

		void RemoveLocationUpdateHandler (EventHandler<LocationUpdatedEventArgs> handler);

		void StopUpdatingLocation ();

		void StartLocationUpdates ();

	}
}

