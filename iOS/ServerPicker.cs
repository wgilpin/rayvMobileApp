using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class ServerPicker: Picker
	{
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
				server_url = "http://192.168.1.9:8080/";
				System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
				restConnection.Instance.setBaseUrl (server_url);
				break;
			case 2:
				server_url = "https://shout-about.appspot.com/";
				System.Diagnostics.Debug.WriteLine ("Server: " + server_url);
				restConnection.Instance.setBaseUrl (server_url);
				break;
			case 3:
				server_url = "https://rayv-app.appspot.com/";
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
			IsVisible = Persist.Instance.IsAdmin;
		}
	}
}



