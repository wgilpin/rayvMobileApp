using System;

using Xamarin.Forms;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class ChangePwdPage : ContentPage
	{
		public event EventHandler Done;

		RayvButton ChangeBtn;
		Entry OldPwdEntry;
		Entry NewPwdEntry;
		Entry ConfirmPwnEntry;

		void DoChangePassword (Object sender, EventArgs e)
		{
			var oldPwd = OldPwdEntry.Text ?? "";
			if (oldPwd != Persist.Instance.GetConfig (settings.PASSWORD)) {
				DisplayAlert ("Error", "Old password is wrong", "OK");
				return;
			}
			var newPwd = NewPwdEntry.Text;
			if (!string.IsNullOrEmpty (newPwd) && newPwd != ConfirmPwnEntry.Text) {
				DisplayAlert ("Error", "New passwords don't match", "OK");
				return;
			}
			if (string.IsNullOrEmpty (newPwd) || newPwd.Length < settings.MIN_PWD_LENGTH) {
				DisplayAlert ("Error",$"Password must be at least {settings.MIN_PWD_LENGTH} characters long","OK"); 
				return;
			}
			var conn = Persist.Instance.GetWebConnection ();
			Dictionary<string,string> webParams = new Dictionary<string,string> { 
				{ "oldpwd", oldPwd },
				{ "newpwd", newPwd },
			};
			var result = conn.post ("/api/password", webParams);
			if (string.IsNullOrEmpty (result) || result != "OK") {
				DisplayAlert ("Error", "Password Change Failed", "OK");
				return;
			}
			Persist.Instance.SetConfig (settings.PASSWORD, newPwd);
			DisplayAlert ("Changed", "Password Changed", "OK");
		}

		public ChangePwdPage ()
		{
			this.Title = "Change Password";
			Padding = 10;
			ChangeBtn = new RayvButton ("Change");
			ChangeBtn.Clicked += DoChangePassword;
			OldPwdEntry = new Entry { Placeholder = "OldPassword" };
			NewPwdEntry = new Entry { Placeholder = "New Password" };
			ConfirmPwnEntry = new Entry { Placeholder = "Confirm New Password" };
			Content = new StackLayout { 
				Spacing = 5,
				Children = {
					OldPwdEntry,
					NewPwdEntry,
					ConfirmPwnEntry,
					ChangeBtn
				}
			};
		}
	}
}


