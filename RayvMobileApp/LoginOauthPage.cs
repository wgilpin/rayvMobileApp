using System;

using Xamarin.Forms;

namespace RayvMobileApp
{
	public class LoginOauthPage : BaseContentPage
	{
		public EventHandler Close;

		public LoginOauthPage ()
		{
			Content = new StackLayout { 
				Children = {
					new Label { Text = "Oauth Login" }
				}
			};
		}
	}
}


