using System;
using Xamarin.Forms;
using Xamarin;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
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
		RayvButton ActivityBtn;
		RayvButton PwdBtn;
		RayvButton SaveBtn;
		Xamarin.Forms.Switch PushSw;
		Xamarin.Forms.Switch EmailsSw;

		#endregion

		void DoGetProfile ()
		{
			try {
				String result = restConnection.Instance.get ("api/profile").Content;
				restConnection.LogToServer (LogLevel.INFO, "DoGetProfile" + result);
				JObject obj = JObject.Parse (result);
				ScreenNameEd.Text = obj ["profile"] ["screen_name"].ToString ();
				EmailEd.Text = obj ["profile"] ["email"].ToString ();
				try {
					GenderEd.SelectedIndex = GenderEd.Items.IndexOf (obj ["profile"] ["sex"].ToString ());
				} catch {
					GenderEd.SelectedIndex = 0;
				}
				SaveBtn.IsVisible = false;
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DoGetProfile {0}", ex);
			}
		}

		void DoSaveProfile (object s, EventArgs e)
		{
			ScreenNameEd.Unfocus ();
			Dictionary<string, string> ps = new Dictionary<string, string> ();
			ps ["screen_name"] = ScreenNameEd.Text;
			if (GenderEd.SelectedIndex > -1)
				ps ["gender"] = GenderEd.Items [GenderEd.SelectedIndex];
			String result = restConnection.Instance.post ("api/profile", ps);
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

			ActivityBtn = new RayvButton ("My Activity");
			PwdBtn = new RayvButton ("Change Password");
			PushSw = new Xamarin.Forms.Switch{ IsToggled = true, };
			EmailsSw = new Xamarin.Forms.Switch{ IsToggled = true, };

			StackLayout stack = new StackLayout {
				Orientation = StackOrientation.Vertical,
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
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
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
				Children = {
					stack,
					new BottomToolbar (this, "profile")
				}
			};
			DoGetProfile ();
		}
	}
}

