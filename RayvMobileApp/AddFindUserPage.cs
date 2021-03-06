﻿using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin;

namespace RayvMobileApp
{
	public class AddFindUserPage : ContentPage
	{
		public EventHandler Succeeded;
		public EventHandler Failed;

		public AddFindUserPage ()
		{
			Title = "Find a User";
			BackgroundColor = settings.BaseColor;
			var emailEd = new Entry { Placeholder = "Email" };
			var searchBtn = new ColouredButton ("Invite as Friend");
			string result = null;
			searchBtn.Clicked += async (sender, e) => {
				var param = new Dictionary<string, string> () {
					{ "email",emailEd.Text }
				};
				try {
					result = Persist.Instance.GetWebConnection ().get ("api/friend/invite", param).Content;
				} catch (Exception ex) {
					Insights.Report (ex, "Email", emailEd.Text);
					Failed?.Invoke (this, null);
					return;
				}
				try {
					switch (result) {
						case "FOUND":
							await DisplayAlert ("Sent", "Invite Sent", "OK");
							Succeeded?.Invoke (this, null);
							return;
						case "EMAIL TO SELF":
							DisplayAlert ("Errr..", "You have invited yourself. Does not compute!", "OK");
							break;
						case "NOT FOUND":
							if (await DisplayAlert ("Not Found", "That user could not be found. Send them an invite email?", "Send", "Cancel")) {
								String emailResult = Persist.Instance.GetWebConnection ().post ("api/email_friend", "email", emailEd.Text);
								if (emailResult == "OK") {
									await DisplayAlert ("Sent", $"Email sent to {emailEd.Text}", "OK"); 
									Succeeded?.Invoke (this, null);
									return;
								}
							}
							break;
						default:
							Failed?.Invoke (this, null);
							break;
					}
				} catch (Exception ex) {
					Insights.Report (ex);
				}
				Failed?.Invoke (this, null);
			};
			Content = new StackLayout () {
				Spacing = 15,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label {
						Text = "Find someone who uses Sprout", 
						TextColor = Color.White,
						FontSize = settings.FontSizeLabelLarge
					},
					new Label { Text = "Enter their email address", TextColor = Color.White },
					emailEd,
					searchBtn
				}
			};
			Padding = 5;

		}
	}
}


