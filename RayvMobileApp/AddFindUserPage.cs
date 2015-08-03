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
					String result = restConnection.Instance.get ("api/friend/invite", param).Content;
					switch (result) {
						case "FOUND":
							await DisplayAlert ("Sent", "Invite Sent", "OK");
							Succeeded?.Invoke (this, null);
							return;
							break; 
						case "EMAIL TO SELF":
							DisplayAlert ("Errr..", "You have invited yourself. Does not compute!", "OK");
							break;
						case "NOT FOUND":
							if (await DisplayAlert ("Not Found", "That user could not be found. Send them an invite email?", "Send", "Cancel")) {
								String emailResult = restConnection.Instance.post ("api/email_friend", "email", emailEd.Text);
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


