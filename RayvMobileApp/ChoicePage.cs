using System;

using Xamarin.Forms;
using System.Collections.Generic;

namespace RayvMobileApp.iOS
{
	public class ChoicePage : ContentPage
	{
		string ANY_PLACE = "Anything";
		PersistantQueue history;

		void DoListChoice (object s, ItemTappedEventArgs e)
		{
			string item = (e.Item as string);
			if (item == ANY_PLACE) {
				this.Navigation.PushModalAsync (new NavigationPage (new ListPage ()));
			} else {
				history.Add (item);
				this.Navigation.PushModalAsync (new NavigationPage (new ListPage (item)));
			}
		}

		public ChoicePage ()
		{
			
			Title = "Type of food";
			ListView list = new ListView ();
			List<string> data = new List<string> ();
			data.Add (ANY_PLACE);
			for (int i = 0; i < history.Length; i++) {
				data.Add (history.GetItem (i));
			}
			foreach (Category cat in Persist.Instance.Categories) {
				data.Add (cat.Title);
			}
			list.ItemsSource = data;
			list.ItemTapped += DoListChoice;
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					list,
					tools
				}
			};
		}
	}
}


