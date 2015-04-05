using System;
using Xamarin.Forms;
using Xamarin;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class ProfilePage: ContentPage
	{
		#region Fields

		Entry ScreenNameEd;
		Label EmailEd;
		Picker GenderEd;
		RayvButton ActivityBtn;
		RayvButton PwdBtn;
		RayvButton LogoutBtn;
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
				SaveBtn.IsEnabled = false;
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

			Insights.Track ("Profile Page");
			Grid grid = new Grid {
				Padding = 5,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (22, GridUnitType.Absolute) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (50, GridUnitType.Absolute) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },

					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },

					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star)  },
				},
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};

			ScreenNameEd = new Entry {
				Placeholder = "Screen Name (what other users see)",
			};
			ScreenNameEd.TextChanged += (sender, e) => {
				SaveBtn.IsEnabled = true;
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
				SaveBtn.IsEnabled = true;
			};

			ActivityBtn = new RayvButton ("My Activity");
			PwdBtn = new RayvButton ("Change Password");
			PushSw = new Xamarin.Forms.Switch{ IsToggled = true, };
			EmailsSw = new Xamarin.Forms.Switch{ IsToggled = true, };

			grid.Children.Add (EmailEd, 1, 3, 1, 2);
			grid.Children.Add (new Image{ Source = "18-envelope@2x.png" }, 0, 1);
			grid.Children.Add (ScreenNameEd, 1, 3, 2, 3);
			grid.Children.Add (new Image{ Source = "111-user@2x.png" }, 0, 2);
			grid.Children.Add (GenderEd, 1, 3, 3, 4);
			SaveBtn = new RayvButton ("Save"){ IsEnabled = false, };
			SaveBtn.Clicked += DoSaveProfile;

			grid.Children.Add (SaveBtn, 1, 3, 4, 5);
			grid.Children.Add (ActivityBtn, 1, 3, 5, 6);
			grid.Children.Add (new Image{ Source = "259-list@2x.png" }, 0, 5);
			grid.Children.Add (PwdBtn, 1, 3, 6, 7);
			grid.Children.Add (new Image{ Source = "54-lock@2x.png" }, 0, 6);

			grid.Children.Add (new LabelWide ("Notifications") {
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
			}, 1, 7);
			grid.Children.Add (new LabelWide ("Push Notifications"), 1, 2, 8, 9);
			grid.Children.Add (PushSw, 2, 3, 8, 9);
			grid.Children.Add (new LabelWide ("Email Notifications"), 1, 2, 9, 10);
			grid.Children.Add (EmailsSw, 2, 3, 9, 10);
			ToolbarItems.Add (new ToolbarItem {
				Text = "Settings",
				Icon = "19-gear.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("ListPage Toolbar Map: Push MapPage");
					Navigation.PushAsync (new SettingsPage ());
				})
			});
			Content = new StackLayout {
				Children = {
					grid,
					new BottomToolbar (this, "profile")
				}
			};
			DoGetProfile ();
		}
	}
}

