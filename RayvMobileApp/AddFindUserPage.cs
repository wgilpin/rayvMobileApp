using System;

using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
					} else {
						Failed?.Invoke (this, null);
					}
				} catch {
					Failed?.Invoke (this, null);
				}
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


