using System;

using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class FriendsChooserView : ContentView
	{
		public event EventHandler Saved;

		public string SelectedKey = "";

		ListView lv;
		const int IMAGE_SIZE = 25;


		void DoSelectItem (Object s, ItemTappedEventArgs ev)
		{
			KeyValuePair<string, Friend> kvp = (KeyValuePair<string, Friend>)ev.Item;
			var tappedName = kvp.Value.Name;
			Friend fr = Persist.Instance.Friends.Where (kv => kv.Value.Name == tappedName).First ().Value;
			SelectedKey = fr.Key;
			lv.ItemsSource = null;
			lv.ItemsSource = Persist.Instance.Friends;
			Saved?.Invoke (this, null);
		}



		public FriendsChooserView ()
		{
			lv = new ListView ();
			lv.ItemTemplate = new DataTemplate (() => {
				Padding = 5;
				BackgroundColor = Color.White;

				Label nameLabel = new Label {
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
					FontAttributes = FontAttributes.Bold,
					LineBreakMode = LineBreakMode.TailTruncation,
					YAlign = TextAlignment.Center,
				};
				nameLabel.SetBinding (Label.TextProperty, "Value.Name");


				return new ViewCell {
					View = nameLabel,
				};
				// Return an assembled ViewCell.

			});
			lv.ItemsSource = Persist.Instance.Friends;
			lv.ItemTapped += DoSelectItem;

			Content = lv;
		}
	}
}


