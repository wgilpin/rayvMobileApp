using System;

namespace RayvMobileApp
{
	public class EventArgsMessage : EventArgs
	{
		public string Message;

		public EventArgsMessage (string message)
		{
			Message = message;
		}
	}
}

