using System;
using RestSharp;
using System.Collections.Generic;
using System.Text;
using Xamarin;

namespace RayvMobileApp.iOS
{
	public enum LogLevel
	{
		CRITICAL = 50,
		ERROR = 40,
		WARNING = 30,
		INFO = 20,
		DEBUG = 10
	}


	public class restConnection
	{



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
				lock (Persist.Instance.Lock) {
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
				Insights.Report (E);
				restConnection.LogErrorToServer (String.Format ("get: exception {0}", E));
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

		public static void LogToServer (LogLevel level, String message)
		{
			try {

				Console.Error.WriteLine ("{0}: {1}", level.ToString (), message);
				Dictionary<string, string> parameters = new Dictionary<string, string> ();
				
				parameters ["level"] = Convert.ToString ((int)level);
				parameters ["message"] = message;
				try {
					restConnection.Instance.post ("/api/log", parameters);
				} catch (Exception ex) {
					Console.Error.WriteLine ("LogToServer Exception {0}", ex);
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				Console.WriteLine ("LogToError Exception 1", ex);
			}
		}

		public static void LogToServer (LogLevel level, string format, params object[] args)
		{
			try {
				String msg = String.Format (format, args);
				LogToServer (level, msg);
			} catch (Exception ex) {
				restConnection.LogErrorToServer ("LogToServer Exception 2", ex);
			}
		}

		public static void LogErrorToServer (string format, params object[] args)
		{
			try {
				String msg = String.Format (format, args);
				LogToServer (LogLevel.ERROR, msg);
			} catch (Exception ex) {
				restConnection.LogErrorToServer ("LogErrorToServer Exception 1", ex);
			}
		}

		public static void LogErrorToServer (string message)
		{
			try {
				LogToServer (LogLevel.ERROR, message);
			} catch (Exception ex) {
				restConnection.LogErrorToServer ("LogErrorToServer Exception 2", ex);
			}
		}

	}
}

