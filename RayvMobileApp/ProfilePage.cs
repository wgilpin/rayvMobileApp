using System;
using Xamarin.Forms;
using Xamarin;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class UserProfile
	{
		public string ScreenName { get; private set; }

		public string Email { get; private set; }

		public string Gender { get; private set; }

		public UserProfile ()
		{
			try {
				var restResult = Persist.Instance.GetWebConnection ().get ("api/profile");
				if (restResult != null) {
					String result = Persist.Instance.GetWebConnection ().get ("api/profile").Content;
					JObject obj = JObject.Parse (result);
					ScreenName = obj ["profile"] ["screen_name"].ToString ();
					Email = obj ["profile"] ["email"].ToString ();
					Gender = obj ["profile"] ["sex"].ToString ();
					Persist.Instance.SetConfig (settings.PROFILE_SCREENNAME, ScreenName);
					Persist.Instance.SetConfig (settings.PROFILE_EMAIL, Email);
					Persist.Instance.SetConfig (settings.PROFILE_GENDER, Gender);
				} else {
					// no response
					throw new ApplicationException ("No server response");
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DoGetProfile {0}", ex);
				try {
					ScreenName = Persist.Instance.GetConfig (settings.PROFILE_SCREENNAME);
					Email = Persist.Instance.GetConfig (settings.PROFILE_EMAIL);
					Gender = Persist.Instance.GetConfig (settings.PROFILE_GENDER);
				} catch (Exception innerEx) {
					Insights.Report (innerEx);
					throw new ApplicationException ("Bad Server Response");
				}
			}
		}
	}

	public class LineGrid3: Grid
	{
	
		public LineGrid3 () : base ()
		{
			Padding = new Thickness (0, 5, 0, 5);
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (22, GridUnitType.Absolute) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (50, GridUnitType.Absolute) });
			RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  });
			HorizontalOptions = LayoutOptions.FillAndExpand;
		}

		public LineGrid3 (Image leftImg, View rightView) : this ()
		{
			if (leftImg != null)
				this.Children.Add (leftImg, 0, 1, 0, 1); 
			this.Children.Add (rightView, 1, 3, 0, 1); 
		}

		public LineGrid3 (View leftView, Xamarin.Forms.Switch rightSwitch) : this ()
		{
			this.Children.Add (leftView, 1, 2, 0, 1); 
			this.Children.Add (rightSwitch, 2, 3, 0, 1); 
		}
	}

	public class ProfilePage: ContentPage
	{
		#region Fields

		Entry ScreenNameEd;
		Label EmailEd;
		Picker GenderEd;
		RayvButton PwdBtn;
		RayvButton SaveBtn;
		Xamarin.Forms.Switch PushSw;
		Xamarin.Forms.Switch EmailsSw;
		StackLayout stack;

		#endregion

		void DoGetProfile ()
		{
			try {
				var profile = new UserProfile ();

				ScreenNameEd.Text = profile.ScreenName;
				EmailEd.Text = profile.Email;
				try {
					GenderEd.SelectedIndex = GenderEd.Items.IndexOf (profile.Gender);
				} catch {
					GenderEd.SelectedIndex = 0;
				}
				SaveBtn.IsVisible = false;
				Persist.Instance.SetConfig (settings.PROFILE_SCREENNAME, profile.ScreenName);
				Persist.Instance.SetConfig (settings.PROFILE_EMAIL, profile.Email);
				Persist.Instance.SetConfig (settings.PROFILE_GENDER, profile.Gender);
			} catch (ApplicationException) {
				Content = new Label{ Text = "Could not load profile" };
			}
			
		}

		void DoSaveProfile (object s, EventArgs e)
		{
			ScreenNameEd.Unfocus ();
			Dictionary<string, string> ps = new Dictionary<string, string> ();
			ps ["screen_name"] = ScreenNameEd.Text;
			if (GenderEd.SelectedIndex > -1)
				ps ["gender"] = GenderEd.Items [GenderEd.SelectedIndex];
			String result = Persist.Instance.GetWebConnection ().post ("api/profile", ps);
			if (result != "OK") {
				DisplayAlert ("Error", "Couldn't Save: " + result, "OK");
				SaveBtn.IsEnabled = true;
			}
			SaveBtn.IsEnabled = false;
		}

		public ProfilePage ()
		{
			Title = "My Profile";
			BackgroundColor = Color.White;
			Insights.Track ("Profile Page");


			ScreenNameEd = new Entry {
				Placeholder = "Screen Name (what other users see)",
			};
			ScreenNameEd.TextChanged += (sender, e) => {
				SaveBtn.IsVisible = true;
			};
			EmailEd = new Label {
				TranslationX = 3,
				TranslationY = 4,
			};

			GenderEd = new Picker {
				Title = "Gender (optional)"
			};
			GenderEd.Items.Add ("Gender Not disclosed");
			GenderEd.Items.Add ("Male");
			GenderEd.Items.Add ("Female");
			GenderEd.Items.Add ("Other");
			GenderEd.SelectedIndex = 0;
			GenderEd.SelectedIndexChanged += (sender, e) => {
				SaveBtn.IsVisible = true;
			};

			PwdBtn = new RayvButton ("Change Password");
			PwdBtn.Clicked += (sender, e) => {
				
			};

			PushSw = new Xamarin.Forms.Switch{ IsToggled = true, };
			EmailsSw = new Xamarin.Forms.Switch{ IsToggled = true, };

			stack = new StackLayout {
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			stack.Children.Add (new LineGrid3 (
				new Image{ Source = settings.DevicifyFilename ("18-envelope@2x.png") }, 
				EmailEd));
			stack.Children.Add (new LineGrid3 (
				new Image{ Source = settings.DevicifyFilename ("111-user@2x.png") }, 
				ScreenNameEd));
			stack.Children.Add (new LineGrid3 (
				null, 
				GenderEd));

			SaveBtn = new RayvButton ("Save"){ IsVisible = false, };
			SaveBtn.Clicked += DoSaveProfile;
			stack.Children.Add (new LineGrid3 (
				null, 
				SaveBtn));
			stack.Children.Add (new LineGrid3 (
				new Image{ Source = settings.DevicifyFilename ("54-lock@2x.png") }, 
				PwdBtn));
			stack.Children.Add (new LabelWide ("Notifications") {
				FontSize = settings.FontSizeLabelLarge,
			});
			stack.Children.Add (new LineGrid3 (new LabelWide ("Push Notifications"), PushSw));
			stack.Children.Add (new LineGrid3 (new LabelWide ("Email Notifications"), EmailsSw));

			// intro slides
			Label ShowLbl = new Label { Text = "Show introduction every time" };
			var ShowSw = new Xamarin.Forms.Switch { 
				IsToggled = !Persist.Instance.GetConfigBool (settings.SKIP_INTRO) 
			};
			ShowSw.Toggled += (sender, e) => { 
				Persist.Instance.SetConfig (settings.SKIP_INTRO, !ShowSw.IsToggled);
			};
			stack.Children.Add (new LineGrid3 (ShowLbl, ShowSw));

			ToolbarItems.Add (new ToolbarItem {
				Text = "Settings",
				Icon = settings.DevicifyFilename ("19-gear.png"),
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("ListPage Toolbar Map: Push MapPage");
					Navigation.PushAsync (new SettingsPage ());
				})
			});
			Content = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					stack,
					new BottomToolbar (this, "profile")
				}
			};
			DoGetProfile ();
		}
	}
}