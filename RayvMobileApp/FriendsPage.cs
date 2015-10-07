using System;

using Xamarin.Forms;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class FriendsPage : ContentPage
	{
		ListView listView;
		const int BUTTON_SIZE = 80;
		int roundButtonSize = Device.OnPlatform (30, 50, 30);
		StackLayout innerContent;

		DataTemplate GetDataTemplate ()
		{
			return new DataTemplate (() => {
				var letterFontSize = Device.OnPlatform (
					                     settings.FontSizeButtonLarge,
					                     settings.FontSizeButtonMedium,
					                     settings.FontSizeButtonLarge);
				Button LetterBtn = new Button {
					WidthRequest = roundButtonSize,
					HeightRequest = roundButtonSize,
					FontSize = letterFontSize,
					BorderRadius = roundButtonSize / 2,
//					BackgroundColor = Vote.RandomColor (name),
					//Text = Vote.FirstLetter (name),
					TextColor = Color.White,
					VerticalOptions = LayoutOptions.Center,
				};
				LetterBtn.SetBinding (
					Button.TextProperty, 
					new Binding ("Value.Name", converter: new FriendToFirstCharConverter ()));
//				LetterBtn.SetBinding (Button.TextProperty, "Value.Name.FirstChar");
				LetterBtn.SetBinding (
					Button.BackgroundColorProperty, 
					new Binding ("Value.Name", converter: new FriendToRandomColorConverter ()));
//				LetterBtn.SetBinding (Button.BackgroundColorProperty, "Value.Name.RandomColor");


				Button delBtn = new RayvButton {
					Text = "  Unfriend  ",
					IsVisible = false,
					FontSize = settings.FontSizeButtonMedium
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
							if (Persist.Instance.Unfriend (friendKey)) {
								Persist.Instance.Friends.Remove (friendKey);
								SetInnerContent ();
							} else
								DisplayAlert ("Fail", $"Couldn't unfriend {friend.Name} - Try Later", "OK"); 
						} else {
							delBtn.IsVisible = false;
						}
					}
				};
				NamedSize fontSize = Device.OnPlatform (NamedSize.Large, NamedSize.Medium, NamedSize.Medium);
				Button nameBtn = new Button {
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
					FontAttributes = FontAttributes.Bold,
					CommandParameter = delBtn,
					FontSize = Device.GetNamedSize (fontSize, typeof(Button))
				};
				nameBtn.SetBinding (Button.TextProperty, "Value.Name");
				nameBtn.Clicked += (object sender, EventArgs e) => {
					((sender as Button).CommandParameter as Button).IsVisible = true;
					// Return an assembled ViewCell.
				};
				var cell = new ViewCell {
					View = new StackLayout {
						Orientation = StackOrientation.Horizontal,
//						HorizontalOptions = LayoutOptions.FillAndExpand,
						Children = { LetterBtn, nameBtn, delBtn },
						Spacing = 10,
						Padding = new Thickness (10, 5, 10, 5),
					}
				};
				return cell;
			});
		}

		void SetInnerContent ()
		{
			innerContent.Children.Clear ();
			if (Persist.Instance.Friends.Count == 0) {
				innerContent.Children.Add (
					new Label { 
						Text = "No Friends (Yet)", 
						VerticalOptions = LayoutOptions.CenterAndExpand,
						HorizontalOptions = LayoutOptions.Center,
					}
				);
			} else {
				if (listView == null) {
					listView = new ListView {
						// Source of data items.
						ItemsSource = Persist.Instance.Friends,
						SeparatorColor = settings.ColorDarkGray,
						SeparatorVisibility = SeparatorVisibility.Default,
						RowHeight = Device.OnPlatform (100, 120, 120),
						// Define template for displaying each item.
						ItemTemplate = GetDataTemplate (),
						VerticalOptions = LayoutOptions.FillAndExpand,
					};
					listView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
					};	
				} else {
					// already exists, reload source
					listView.ItemsSource = null;
					listView.ItemsSource = Persist.Instance.Friends;
				}
				innerContent.Children.Add (listView);
			}
		}

		public FriendsPage ()
		{
			Title = "Friends";
			BackgroundColor = Color.White;

			var addFriendBtn = new RayvButton ("Add New Friend");
			addFriendBtn.OnClick = (s, e) => {
				Navigation.PushAsync (new AddFriendPage ());
			};
			StackLayout tools = new BottomToolbar (this, "add");
			innerContent = new StackLayout { VerticalOptions = LayoutOptions.FillAndExpand };
			SetInnerContent ();
			Content = new StackLayout {
				Children = {
					addFriendBtn,
					innerContent,
					tools
				}
			};
		}
	}
}


