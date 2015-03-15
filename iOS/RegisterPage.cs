using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Xamarin;
using System.Net.Mail;

namespace RayvMobileApp.iOS
{
	public class RegisterPage : ContentPage
	{
		Entry FirstNameEd;
		Entry LastNameEd;
		Entry UserNameEd;
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
			if (UserNameEd.Text.Length == 0) {
				await DisplayAlert ("User Name Missing ", "Please supply a User Name", "OK");
				return;
			}
			if (FirstNameEd.Text.Length == 0 || LastNameEd.Text.Length == 0) {
				await DisplayAlert ("Full Name Needed", "Please supply a first & a last name", "OK");
				return;
			}
			try {
				MailAddress m = new MailAddress (EmailEd.Text);
				//good email
			} catch (FormatException) {
				await DisplayAlert ("Invalid Email", "Please supply a valid email address", "OK");
				return;
			}
			Dictionary<String,String> parameters = new Dictionary<String,String> ();
			parameters ["username"] = UserNameEd.Text;
			parameters ["password"] = Pwd1Ed.Text;
			parameters ["email"] = EmailEd.Text;
			parameters ["fn"] = FirstNameEd.Text;
			parameters ["ln"] = LastNameEd.Text;
			parameters ["screenname"] = ScreenNameEd.Text;
			try {
				if (ScreenNameEd.Text == "") {
					string fn = FirstNameEd.Text;
					fn = fn [0].ToString ().ToUpper () [0] + fn.Substring (1);
					parameters ["screenname"] = String.Format (
						"{0} {1}.", fn, LastNameEd.Text.Remove (1).ToUpper ());
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				await DisplayAlert ("Invalid Name", "Please supply valid first & last names", "OK");
				return;
			}
			Spinner.IsRunning = true;
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				String result = restConnection.Instance.post ("/api/register", parameters);
				if (result == "BAD_USERNAME") {
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("New user Registration failed - Username in use");
						DisplayAlert (
							"Try Again",
							String.Format ("The user name {0} is already taken", UserNameEd.Text),
							"OK");
						return;
					});
				}
				if (result == "OK") {
					Console.WriteLine ("New user Registered");
					Persist.Instance.SetConfig (settings.PASSWORD, Pwd1Ed.Text);
					Persist.Instance.SetConfig (settings.USERNAME, UserNameEd.Text);
					restConnection.Instance.setCredentials (UserNameEd.Text, Pwd1Ed.Text, "");
					Persist.Instance.Wipe ();
					try {
						Insights.Identify (UserNameEd.Text, "server", Persist.Instance.GetConfig (settings.SERVER));
						Console.WriteLine ("AppDelegate Analytics ID: {0}", UserNameEd.Text);
					} catch (Exception ex) {
						Insights.Report (ex);
					}
					Device.BeginInvokeOnMainThread (() => {
						this.Navigation.PushModalAsync (new MainMenu ());
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
			UserNameEd = new Entry {
				Placeholder = "User Name (for logon)",
				Text = "",
			};
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
					UserNameEd,
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
					this.Navigation.PushModalAsync (new LoginPage ());
				})
			});
		}
	}
}


