using System;

using Xamarin.Forms;
using System.Collections.Generic;

namespace RayvMobileApp.iOS
{
	public class ChoicePage : ContentPage
	{
		public ChoicePage ()
		{
			Title = "Find me a...";
			ListView list = new ListView ();
			List<string> data = new List<string> ();
			data.Add ("Anything");
			foreach (string cat in Persist.Instance.Categories) {
				data.Add (cat);
			}
			list.ItemsSource = data;
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


