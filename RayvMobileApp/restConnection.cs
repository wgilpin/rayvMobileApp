using System;
using RestSharp;
using System.Collections.Generic;
using System.Text;
using Xamarin;
using System.Net;
using Xamarin.Forms;
using System.Threading;

namespace RayvMobileApp
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

		IRestResponse innerGet (string url, Dictionary<string, string> parameters, Method method)
		{
			var request = new RestRequest (url);
			request.Method = method;
			if (parameters != null) {
				foreach (KeyValuePair<string, string> kvp in parameters) {
					request.AddParameter (kvp.Key, kvp.Value, ParameterType.GetOrPost);
				}
			}
			Console.WriteLine (String.Format ("innerGet: {0}{1}", client.BaseUrl, request.Resource));

			client.Timeout = 30000;
			IRestResponse response = client.Execute (request);
			Console.WriteLine (String.Format ("innerGet: response: {0}", response.Content.Substring (0, Math.Min (100, response.Content.Length))));
			if (response.StatusCode == HttpStatusCode.Unauthorized)
				throw new UnauthorizedAccessException ("Bad Login");
			try {
				int code = (int)response.StatusCode.GetTypeCode ();
				if (code > 400 || response.Content.IndexOf ("<html") > -1)
					throw new InvalidOperationException (
						String.Format (
							"Status {0} {1}", 
							response.StatusCode, 
							response.StatusDescription));
			} catch {
				return null;
			}
			return response;
		}

		public IRestResponse get (string url,
		                          string param_name,
		                          string param_value,
		                          Method method = Method.GET,
		                          int getRetries = settings.MAX_SERVER_TRIES)
		{
			var parms = new Dictionary<string,string> {
				{ param_name, param_value }
			};
			return get (url, parms, method, getRetries);
		}


		public IRestResponse get (string url,
		                          Dictionary<string,string> parameters = null,
		                          Method method = Method.GET,
		                          int getRetries = settings.MAX_SERVER_TRIES)
		{
			// only retry a Get
			int MaxRetries = method == Method.GET ? getRetries : 1;
			for (int try_number = 0; try_number < MaxRetries; try_number++) {
				try {
					if (try_number > 0)
						Thread.Sleep (1000);
					var response = innerGet (url, parameters, method);
					if (response == null) {
						// try again soon
						continue;
					}
					if (response.ResponseStatus == ResponseStatus.Error || response.ResponseStatus == ResponseStatus.TimedOut) {
						Persist.Instance.Online = false;
						continue;
					}
					return response;
				} catch (UnauthorizedAccessException) {
					throw;
				} catch (Exception E) {
					Insights.Report (E);
					restConnection.LogErrorToServer (String.Format ("get: exception {0}", E));
					continue;
				}
			}
			return null;
		}

		public string post (string url, string param_name, string param_value)
		{
			var parms = new Dictionary<string,string> {
				{ param_name, param_value }
			};
			var res = this.get (url, parms, Method.POST);
			return res?.Content;
		}

		public string post (string url, Dictionary<string,string> parameters = null)
		{
			var res = this.get (url, parameters, Method.POST);
			return res?.Content;
		}

		public static restConnection Instance {
			get {
				if (instance == null) {
					instance = new restConnection ();
				}
				return instance;
			}
		}

		public void ClearCredentials ()
		{
			instance = null;
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
				Insights.Report (ex);
			}
		}

		public static void LogErrorToServer (string format, params object[] args)
		{
			try {
				String msg = String.Format (format, args);
				Console.WriteLine ("LOG ERROR: {0}", msg);
				LogToServer (LogLevel.ERROR, msg);
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		public static void LogErrorToServer (string message)
		{
			try {
				LogToServer (LogLevel.ERROR, message);
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

	}
}

