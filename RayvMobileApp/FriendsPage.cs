using System;

using Xamarin.Forms;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class FriendsPage : ContentPage
	{
		StackLayout stack;
		const int BUTTON_SIZE = 100;

		async void SetupList ()
		{
			stack.Children.Clear ();
			if (Persist.Instance.Friends.Count == 0) {
				stack.Children.Add (new Label { Text = "No Friends (Yet)" });
			} else
				foreach (var kvp in Persist.Instance.Friends) {
					Button delBtn = new RayvButton {
						Text = "Unfriend",
						CommandParameter = kvp,
						IsVisible = false,
						FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Button))
					};
					delBtn.Clicked += async (object sender, EventArgs e) => {
						var kv = ((sender as Button).CommandParameter as KeyValuePair<string, Friend>?);
						if (kv != null) {
							KeyValuePair<string, Friend> kv2 = (KeyValuePair<string, Friend>)kv;
					
							if (await DisplayAlert ("Unfriend", 
						                        $"Unfriend {kv2.Value.Name}",
							                       "Yes",
							                       "Cancel")) { 
								if (Persist.Instance.Unfriend (kv2.Key))
									SetupList ();
								else
									DisplayAlert ("Fail", $"Couldn't unfriend {kv2.Value.Name} - Try Later", "OK");
							}
						}
					};
					Button nameBtn = new Button {
						TextColor = Color.Black,
						BackgroundColor = Color.Transparent,
						FontAttributes = FontAttributes.Bold,
						Text = kvp.Value.Name,
						CommandParameter = delBtn,
						FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button))
					};
					nameBtn.Clicked += (object sender, EventArgs e) => {
						((sender as Button).CommandParameter as Button).IsVisible = true;
					};
					Grid grid = new Grid {
						Padding = 5,
						VerticalOptions = LayoutOptions.FillAndExpand,
						RowDefinitions = {
							new RowDefinition {
								Height = new GridLength (30)
							},
						},
						ColumnDefinitions = {
							new ColumnDefinition {
								Width = new GridLength (1, GridUnitType.Star)
							},
							new ColumnDefinition {
								Width = new GridLength (BUTTON_SIZE, GridUnitType.Absolute)
							},
						}
					};
					grid.Children.Add (nameBtn, 0, 0);
					grid.Children.Add (delBtn, 1, 0);
					stack.Children.Add (grid);
				}
		}

		public FriendsPage ()
		{
			Title = "Friends";
			stack = new StackLayout ();
			BackgroundColor = Color.White;
			SetupList ();
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					stack,
					tools
				}
			};
		}
	}
}


