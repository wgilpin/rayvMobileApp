using System;
using PushNotification.Plugin;
using System.Collections.Generic;
using PushNotification.Plugin.Abstractions;

//using Foundation;

//using Foundation;

namespace RayvMobileApp
{
	public class PushNotificationsListener: IPushNotificationListener
	{
		public void OnMessage (Newtonsoft.Json.Linq.JObject values, DeviceType deviceType)
		{
			Console.Write ("APNS ALERT: ");
			Console.WriteLine (values ["alert"].ToString ());
			Persist.Instance.NotificationsReceived = true;
		}

		#region IPushNotificationListener implementation

		//		public void OnMessage (IDictionary<string, object> Parameters, DeviceType deviceType)
		//		{
		//			var data = (Parameters ["aps"] as NSMutableDictionary);
		//			Console.Write ("APNS ALERT: ");
		//			Console.WriteLine (data ["alert"]);
		//			Persist.Instance.NotificationsReceived = true;
		//		}

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

