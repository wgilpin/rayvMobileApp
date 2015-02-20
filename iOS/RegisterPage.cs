﻿using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

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
			Dictionary<String,String> parameters = new Dictionary<String,String> ();
			parameters ["username"] = UserNameEd.Text;
			parameters ["email"] = EmailEd.Text;
			parameters ["password"] = Pwd1Ed.Text;
			parameters ["screenname"] = ScreenNameEd.Text;
			String result = restConnection.Instance.post ("/api/register", parameters);
			if (result == "BAD_USERNAME") {
				DisplayAlert (
					"Try Again",
					String.Format ("The user name {0} is already taken", UserNameEd.Text),
					"OK");
				Console.WriteLine ("New user Registration failed - Username in use");
				return;
			}
			if (result == "OK") {
				Console.WriteLine ("New user Registered");
				Persist.Instance.SetConfig (settings.PASSWORD, Pwd1Ed.Text);
				Persist.Instance.SetConfig (settings.USERNAME, UserNameEd.Text);
				restConnection.Instance.setCredentials (UserNameEd.Text, Pwd1Ed.Text, "");
				this.Navigation.PushModalAsync (new ProfilePage ());
			} else
				DisplayAlert ("Failed", "Got an error: " + result, "OK");

		}

		public RegisterPage ()
		{
		
			Title = "Register";

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
				IsPassword = true,
				Text = "",
			};
			Pwd2Ed = new Entry {
				Placeholder = "Password (again)",
				IsPassword = true,
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


