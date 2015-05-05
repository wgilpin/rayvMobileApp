using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class ServerPicker: Picker
	{
		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();

		void DoSelect (object s, EventArgs e)
		{
			string server_url = "";
			switch (SelectedIndex) {
			case 0:
				server_url = "http://localhost:8080/";
				System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
				restConnection.Instance.setBaseUrl (server_url);
				break;
			case 1:
				server_url = "http://192.168.1.7:8080/";
				System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
				restConnection.Instance.setBaseUrl (server_url);
				break;
			case 2:
				server_url = "https://" + GetServerVersionForAppVersion () + "shout-about.appspot.com/";
				System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
				restConnection.Instance.setBaseUrl (server_url);
				break;
			case 3:
				server_url = "https://" + GetServerVersionForAppVersion () + settings.DEFAULT_SERVER;
				System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
				restConnection.Instance.setBaseUrl (server_url);
				break;
			}
			Persist.Instance.SetConfig (settings.SERVER, server_url);

		}

		public ServerPicker () : base ()
		{
			Title = "Server";
			VerticalOptions = LayoutOptions.Start;
			Items.Add ("Local");
			Items.Add ("Dev");
			Items.Add ("Pre-Prod");
			Items.Add ("Production");
			SelectedIndexChanged += DoSelect;
			SelectedIndex = 3;
			IsVisible = DEBUG_ON_SIMULATOR ? true : Persist.Instance.IsAdmin;
		}

		public string GetServerVersionForAppVersion ()
		{
			string[] version_parts = DependencyService.Get<IAppData> ().AppMajorVersion ().Split ('-');
			switch (version_parts [0]) {
			case "0.2":
				return "";
				break;
			case "0.3":
				return "3.";
				break;
			}
			return "";
		}
	}
}



