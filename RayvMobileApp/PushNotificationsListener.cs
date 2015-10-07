using System;
using PushNotification.Plugin;
using System.Collections.Generic;
using PushNotification.Plugin.Abstractions;

namespace RayvMobileApp
{
	public class PushNotificationsListener: IPushNotificationListener
	{
		#region IPushNotificationListener implementation

		public void OnMessage (IDictionary<string, object> Parameters, DeviceType deviceType)
		{
			throw new NotImplementedException ();
		}

		public void OnRegistered (string Token, DeviceType deviceType)
		{
			Console.WriteLine ($"Notifications registered {Token}");
		}

		public void OnUnregistered (DeviceType deviceType)
		{
			throw new NotImplementedException ();
		}

		public void OnError (string message, DeviceType deviceType)
		{
			Console.WriteLine ($"ERROR PushNotificationsListener {message}");
		}

		public bool ShouldShowNotification ()
		{
			return true;
		}

		#endregion

		public PushNotificationsListener ()
		{
			Console.WriteLine ("PushNotificationsListener ctor");
		}

	}
}

