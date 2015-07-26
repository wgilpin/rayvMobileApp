using System;

using Xamarin.Forms;
using Newtonsoft.Json.Linq;
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
			var searchBtn = new RayvButton ("Invite as Friend");
			searchBtn.Clicked += async (sender, e) => {
				var param = new Dictionary<string, string> () {
					{ "email",emailEd.Text }
				};
				try {
					String result = restConnection.Instance.get ("api/find_friend", param).Content;
					if (result == "FOUND") {
						await DisplayAlert ("Sent", "Invite Sent", "OK");
						Succeeded?.Invoke (this, null);
						return;
					} else {
						if (await DisplayAlert ("Not Found", "User could not be found. Send a direct email?", "Send", "Cancel")) {
							String emailResult = restConnection.Instance.post ("api/email_friend", "email", emailEd.Text);
							if (emailResult == "OK") {
								await DisplayAlert ("Sent", $"Email sent to {emailEd.Text}", "OK"); 
								Succeeded?.Invoke (this, null);
								return;
							} else {
								Failed?.Invoke (this, null);
							}
						} else
							Failed?.Invoke (this, null);
					}
				} catch (Exception ex) {
					Insights.Report (ex);
				}
				Failed?.Invoke (this, null);
			};
			var top = new StackLayout () {
				Spacing = 15,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label {
						Text = "Find someone who uses Taste5", 
						TextColor = Color.White,
						FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label))
					},
					new Label { Text = "Enter their email address", TextColor = Color.White },
					emailEd,
					searchBtn
				}
			};
			Padding = 5;
			Content = new StackLayout { 
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					top,
					new BottomToolbar (this, "add")
				}
			};
		}
	}
}


