using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin;
using System.Net.Mail;

namespace RayvMobileApp
{
	public class RegisterPage : ContentPage
	{
		Entry FirstNameEd;
		Entry LastNameEd;
		//		Entry UserNameEd;
		Entry Pwd1Ed;
		Entry Pwd2Ed;
		Entry EmailEd;
		Entry ScreenNameEd;
		RayvButton GoBtn;
		ActivityIndicator Spinner;

		async void  DoRegister (object sender, EventArgs e)
		{
			//validate
			if (Pwd1Ed.Text.Length < 7) {
				await DisplayAlert ("Error", "Password must be 7 or more characters long", "OK");
				return;
			}
			if (Pwd1Ed.Text != Pwd2Ed.Text) {
				await DisplayAlert ("Passwords don't match", "Enter the same password in both boxes", "OK");
				return;
			}
//			if (UserNameEd.Text.Length == 0) {
//				await DisplayAlert ("User Name Missing ", "Please supply a User Name", "OK");
//				return;
//			}
			if (FirstNameEd.Text.Length == 0 || LastNameEd.Text.Length == 0) {
				await DisplayAlert ("Full Name Needed", "Please supply a first & a last name", "OK");
				return;
			}
			try {
				new MailAddress (EmailEd.Text);
				//good email
			} catch (FormatException) {
				await DisplayAlert ("Invalid Email", "Please supply a valid email address", "OK");
				return;
			}
			string[] keys = new string[6];
			string[] values = new string[6];
			keys [0] = "username";
			keys [1] = "password";
			keys [2] = "email";
			keys [3] = "fn";
			keys [4] = "ln";
			keys [5] = "screenname";
			values [0] = EmailEd.Text;
			values [1] = Pwd1Ed.Text;
			values [2] = EmailEd.Text;
			values [3] = FirstNameEd.Text;
			values [4] = LastNameEd.Text;
			values [5] = ScreenNameEd.Text;
			try {
				if (ScreenNameEd.Text == "") {
					string fn = FirstNameEd.Text;
					fn = fn [0].ToString ().ToUpper () [0] + fn.Substring (1);
					values [5] = String.Format (
						"{0} {1}.", fn, LastNameEd.Text.Remove (1).ToUpper ());
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				await DisplayAlert ("Invalid Name", "Please supply valid first & last names", "OK");
				return;
			}
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				String result = Persist.Instance.GetWebConnection ().post ("/api/register", keys, values);
				if (result == "BAD_USERNAME") {
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("New user Registration failed - Username in use");
						DisplayAlert (
							"Try Again",
							String.Format ("The user name {0} is already taken", EmailEd.Text),
							"OK");
						return;
					});
				}
				if (result == "OK") {
					Console.WriteLine ("New user Registered");
					Persist.Instance.SetConfig (settings.PASSWORD, Pwd1Ed.Text);
					Persist.Instance.SetConfig (settings.USERNAME, EmailEd.Text);
					restConnection.Instance.setCredentials (EmailEd.Text, Pwd1Ed.Text, "");
					Persist.Instance.Wipe ();
					try {
						Insights.Identify (EmailEd.Text, "server", Persist.Instance.GetConfig (settings.SERVER));
						Console.WriteLine ("AppDelegate Analytics ID: {0}", EmailEd.Text);
					} catch (Exception ex) {
						Insights.Report (ex);
					}
					Device.BeginInvokeOnMainThread (() => {
						Navigation.PushModalAsync (new RayvNav (new MainMenu ()));
					});
				} else {
					Device.BeginInvokeOnMainThread (() => {
						DisplayAlert ("Failed", "Got an error: " + result, "OK");
					});
				}
				Device.BeginInvokeOnMainThread (() => {
					Spinner.IsRunning = false;
				});
			})).Start ();


		}

		public RegisterPage ()
		{
			Analytics.TrackPage ("RegisterPage");
			Title = "Register";

			Spinner = new ActivityIndicator {
				IsRunning = false,
				Color = Color.Red,
			};

			FirstNameEd = new Entry {
				Placeholder = "First Name",
				Text = "",
			};
			LastNameEd = new Entry {
				Placeholder = "Last Name",
				Text = "",
			};
//			UserNameEd = new Entry {
//				Placeholder = "User Name (for logon)",
//				Text = "",
//			};
			ScreenNameEd = new Entry {
				Placeholder = "Screen Name (what other users see)",
				Text = "",
			};
			Pwd1Ed = new Entry {
				Placeholder = "Password",
				Text = "",
			};
			Pwd2Ed = new Entry {
				Placeholder = "Password (again)",
				Text = "",
			};
			EmailEd = new Entry {
				Placeholder = "E-mail Address",
				Text = "",
			};
			GoBtn = new RayvButton ("Register");
			GoBtn.OnClick = DoRegister;
			Content = new StackLayout { 
				VerticalOptions = LayoutOptions.Start,
				Padding = 20,
				Children = {
					new Label { Text = "Register for a new account" },
					FirstNameEd,
					LastNameEd,
					EmailEd,
					new LabelWide ("Login Details"),
//					UserNameEd,
					Pwd1Ed,
					Pwd2Ed,
					new ServerPicker (),
					Spinner,
					GoBtn,
				}
			};
			ToolbarItems.Add (new ToolbarItem {
				Text = "Back",
				Order = ToolbarItemOrder.Default,
				Command = new Command (() => {
					Debug.WriteLine ("RegisterPage - toolbar back to login");
					var login = new LoginPage ();
					this.Navigation.PushModalAsync (login);
				})
			});
		}
	}
}


