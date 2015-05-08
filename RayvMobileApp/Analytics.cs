using System;
using Xamarin;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class Analytics
	{
		public Analytics ()
		{
		}

		static public void TrackPage (string trackEvent)
		{
			try {
				Insights.Track (trackEvent);
				Console.WriteLine (trackEvent);
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}
	}
}

