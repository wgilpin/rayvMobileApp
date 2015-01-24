using System;
using RestSharp;
using System.Collections.Generic;
using System.Text;

namespace RayvMobileApp.iOS
{
	public class restConnection
	{

		public Object Lock = new Object ();

		private static restConnection instance;
		RestClient client;

		private restConnection ()
		{
			Console.WriteLine ("restConnection()");
			client = new RestClient ();
			client.CookieContainer = new System.Net.CookieContainer ();
		}

		public void setBaseUrl (string url)
		{
			client.BaseUrl = new Uri (url);
		}

		public void setCredentials (string username, string pwd, string domain, string baseUrl = null)
		{
			if (baseUrl != null) {
				client.BaseUrl = new Uri (baseUrl);
			}
			client.Authenticator = new HttpBasicAuthenticator (username, pwd);
		}

		public bool loggedIn { 
			get { 
				lock (Lock) {
					string ping = this.get ("/ping").Content;
					return ping == "OK";
				}
			} 
		}

		public IRestResponse get (string url, Dictionary<string,string> parameters = null, Method method = Method.GET)
		{

			//TODO: retries
			try {
				var request = new RestRequest (url);
				request.Method = method;
				if (parameters != null) {
					foreach (KeyValuePair<string, string> kvp in parameters) {
						request.AddParameter (kvp.Key, kvp.Value, ParameterType.GetOrPost);
					}
				}
				Console.WriteLine (String.Format ("get: {0}{1}", client.BaseUrl, request.Resource));

				IRestResponse response = client.Execute (request);
				Console.WriteLine (String.Format (
					"get: response: {0}", 
					response.Content.Substring (0, Math.Min (100, response.Content.Length))));
				return response;
			} catch (Exception E) {
				Console.WriteLine (String.Format ("get: exception {0}", E));
				return null;
			}
		}

		public string post (string url, Dictionary<string,string> parameters = null)
		{
			return this.get (url, parameters, Method.POST).Content;
		}

		public static restConnection Instance {
			get {
				if (instance == null) {
					instance = new restConnection ();
				}
				return instance;
			}
		}
	}
}

