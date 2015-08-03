using System;

using Xamarin.Forms;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class FriendsPage : ContentPage
	{
		ListView listView;
		const int BUTTON_SIZE = 100;

		DataTemplate GetDataTemplate ()
		{
			return new DataTemplate (() => {
				Button delBtn = new RayvButton {
					Text = "  Unfriend  ",
					IsVisible = false,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Button))
				};
				delBtn.SetBinding (Button.CommandParameterProperty, "Key");
				delBtn.Clicked += async (object sender, EventArgs e) => {
					var friendKey = ((sender as Button).CommandParameter as string);
					if (!string.IsNullOrEmpty (friendKey)) {
						var friend = Persist.Instance.Friends [friendKey];

						if (await DisplayAlert ("Unfriend", 
					                                    $"Unfriend {friend.Name}",
						                        "Yes",
						                        "Cancel")) { 
							if (Persist.Instance.Unfriend (friend.Key))
								GetContent ();
							else
								DisplayAlert ("Fail", $"Couldn't unfriend {friend.Name} - Try Later", "OK"); 
						} else {
							delBtn.IsVisible = false;
						}
					}
				};
				Button nameBtn = new Button {
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
					FontAttributes = FontAttributes.Bold,
					CommandParameter = delBtn,
					FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button))
				};
				nameBtn.SetBinding (Button.TextProperty, "Value.Name");
				nameBtn.Clicked += (object sender, EventArgs e) => {
					((sender as Button).CommandParameter as Button).IsVisible = true;
					// Return an assembled ViewCell.
				};
				return new ViewCell {
					View = new StackLayout {
						Orientation = StackOrientation.Horizontal,
//						HorizontalOptions = LayoutOptions.FillAndExpand,
						Children = { nameBtn, delBtn },
						Spacing = 20,
						Padding = new Thickness (20, 5, 20, 5),
					}
				};
			});
		}

		View GetContent ()
		{
			if (Persist.Instance.Friends.Count == 0) {
				return new Label { Text = "No Friends (Yet)" };
			} else {
				listView = new ListView {
					// Source of data items.
					ItemsSource = Persist.Instance.Friends,
					SeparatorColor = settings.ColorDarkGray,
					SeparatorVisibility = SeparatorVisibility.Default,

					// Define template for displaying each item.
					ItemTemplate = GetDataTemplate (),
				};
				return listView;
			}
		}

		public FriendsPage ()
		{
			Title = "Friends";
			BackgroundColor = Color.White;

			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					GetContent (),
					tools
				}
			};
		}
	}
}


