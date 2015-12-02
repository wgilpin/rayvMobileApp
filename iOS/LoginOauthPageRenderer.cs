using Xamarin.Forms;
using System;
using RayvMobileApp.iOS;
using RayvMobileApp;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Auth;
using Newtonsoft.Json.Linq;
using Xamarin;

[assembly: ExportRenderer (typeof(LoginOauthPage), typeof(LoginOauthPageRenderer))]
namespace RayvMobileApp.iOS
{
	public class LoginOauthPageRenderer: PageRenderer
	{
		public override void ViewDidAppear (bool animated)
		{
			Console.WriteLine ("LoginOauthPageRenderer.ViewDidAppear iOS");
			base.ViewDidAppear (animated);

			var auth = new OAuth2Authenticator (
				           clientId: "490952327738861", // your OAuth2 client id
				           scope: "email", // the scopes for the particular API you're accessing, delimited by "+" symbols
				           authorizeUrl: new Uri ("https://m.facebook.com/dialog/oauth/"), // the auth URL for the service
				           redirectUrl: new Uri ("https://rayv-app.appspot.com/oauth/fb")); // the redirect URL for the service
			auth.AllowCancel = true;

			auth.Completed += (sender, eventArgs) => {
				

				if (eventArgs.IsAuthenticated) {
					// Use eventArgs.Account to do wonderful things
					Persist.Instance.OauthToken = eventArgs.Account.Properties ["access_token"];
					var tok = eventArgs.Account.Properties ["access_token"];
					Console.WriteLine ($"OAUTH TOKEN STORED {tok}");
					var request = new OAuth2Request ("GET", new Uri ("https://graph.facebook.com/me?fields=email"), null, eventArgs.Account);
					request.GetResponseAsync ().ContinueWith (t => {
						Console.WriteLine ("LoginOauthPageRenderer.Completed.ContinueWith ");
						try {
							JObject obj = JObject.Parse (t.Result.GetResponseText ());
							var id = obj ["email"].ToString ();
							Persist.Instance.SetConfig (settings.FACEBOOK_EMAIL, id);
							Persist.Instance.OauthNavPage.Navigation.PopModalAsync ();
							Persist.Instance.OauthNavPage.Navigation.PopModalAsync ();
						} catch (Exception ex) {
							Console.WriteLine ($"LoginOauthPageRenderer.Completed.ContinueWith Error {ex}");
							Insights.Report (ex);
						}

					});
				} else {
					// The user cancelled
				}
				// We presented the UI, so it's up to us to dimiss it on iOS.
			};
			Console.WriteLine ("LoginOauthPageRenderer.ViewDidAppear iOS Presenting");
			PresentViewController (auth.GetUI (), true, null);
			Console.WriteLine ("LoginOauthPageRenderer.ViewDidAppear iOS Presented");
		}
	}
}
