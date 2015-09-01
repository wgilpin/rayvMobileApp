using System;
using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class ServerPicker: Picker
	{
		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();

		Dictionary<string, string> ServerChoices = new Dictionary<string, string> ();

		void DoSelect (object s, EventArgs e)
		{
			string server_url = "";
			server_url = ServerChoices [Items [SelectedIndex]];
			System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
			restConnection.Instance.setBaseUrl (server_url);
			Persist.Instance.SetConfig (settings.SERVER, server_url);
		}

		public ServerPicker () : base ()
		{
			Title = "Server";
			VerticalOptions = LayoutOptions.Start;
			ServerChoices.Add ("Local", "http://localhost:8080/");
			ServerChoices.Add ("Dev", "http://192.168.1.2:8080/");
			ServerChoices.Add ("Pre-Prod", "https://" + GetServerVersionForAppVersion () + "shout-about.appspot.com/");
			ServerChoices.Add ("Production", "https://" + GetServerVersionForAppVersion () + settings.DEFAULT_SERVER);
			foreach (var kvp in ServerChoices)
				Items.Add (kvp.Key);
			try {
				SelectedIndex = ServerChoices.Values.ToList ().IndexOf (Persist.Instance.GetConfig (settings.SERVER));
			} catch {
				SelectedIndex = ServerChoices.Count - 1;
			}
			SelectedIndexChanged += DoSelect;
			IsVisible = DEBUG_ON_SIMULATOR ? true : Persist.Instance.IsAdmin;
		}

		public static string GetServerVersionForAppVersion ()
		{
			switch (GetServerVersion ()) {
				case "0.2":
					return "";
				case "0.3":
					return "3-dot-";
				case "0.4":
					return "4-dot-";
				case "0.5":
					return "5-dot-";
				case "0.6":
					return "6-dot-";
			}
			return "";
		}

		public static string GetServerVersion ()
		{
			string[] version_parts = DependencyService.Get<IAppData> ().AppMajorVersion ().Split ('-');
			return version_parts [0];
		}
	}
}



