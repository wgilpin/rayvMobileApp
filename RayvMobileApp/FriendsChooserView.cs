using System;

using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class FriendsChooserView : ContentView
	{
		public event EventHandler Saved;

		ListView lv;
		const int IMAGE_SIZE = 25;


		void DoSelectItem (Object s, ItemTappedEventArgs ev)
		{
			KeyValuePair<string, Friend> kvp = (KeyValuePair<string, Friend>)ev.Item;
			var tappedName = kvp.Value.Name;
			Friend fr = Persist.Instance.Friends.Where (kv => kv.Value.Name == tappedName).First ().Value;
			fr.InFilter = !fr.InFilter;
			lv.ItemsSource = null;
			lv.ItemsSource = Persist.Instance.Friends;
		}

		public FriendsChooserView ()
		{
			lv = new ListView ();
			lv.ItemTemplate = new DataTemplate (() => {
				BackgroundColor = Color.White;

				Label nameLabel = new Label {
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
					FontAttributes = FontAttributes.Bold,
					LineBreakMode = LineBreakMode.TailTruncation,
					YAlign = TextAlignment.Center,
				};
				nameLabel.SetBinding (Label.TextProperty, "Value.Name");

				Image checkBox = new Image { 
					Aspect = Aspect.AspectFill,
					WidthRequest = IMAGE_SIZE, 
					HeightRequest = IMAGE_SIZE
				};
				checkBox.SetBinding (Image.SourceProperty, "Value.InFilterImage");

				Grid grid = new Grid {
					Padding = 5,
					VerticalOptions = LayoutOptions.FillAndExpand,
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (22)  },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength (IMAGE_SIZE, GridUnitType.Absolute) },
					}
				};
				grid.Children.Add (nameLabel, 0, 0);
				grid.Children.Add (checkBox, 1, 0);

				return new ViewCell {
					View = grid,
				};
				// Return an assembled ViewCell.

			});
			lv.ItemsSource = Persist.Instance.Friends;
			lv.ItemTapped += DoSelectItem;
			var DoneBtn = new RayvButton ("Done");
			DoneBtn.Clicked += (sender, e) => Saved?.Invoke (this, null);
			var stack = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					lv,
					DoneBtn
				}
			};
			Content = stack;
		}
	}
}


