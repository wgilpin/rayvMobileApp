using System;

namespace RayvMobileApp
{
	public interface IDeviceSpecific
	{
		bool MakeCall (string phoneNumber);

		bool RunningOnIosSimulator ();
	}
}

