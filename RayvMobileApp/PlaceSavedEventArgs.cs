using System;

namespace RayvMobileApp
{
	public class PlaceSavedEventArgs : EventArgs
	{
		private readonly Place _place;

		public PlaceSavedEventArgs (Place place)
		{
			_place = place;
		}

		public Place EditedPlace {
			get { return _place; }
		}
	}
}

