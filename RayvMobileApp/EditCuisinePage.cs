using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;

namespace RayvMobileApp
{
	public class CuisineSavedEventArgs : EventArgs
	{
		Cuisine _cuisine;

		public Cuisine Cuisine {
			get { return _cuisine; }
		}

		public CuisineSavedEventArgs (Cuisine category)
		{
			_cuisine = category;
		}
	}

	public class EditCuisinePage : ContentPage
	{
		public event EventHandler<CuisineSavedEventArgs> Saved;

		protected virtual void OnSaved (Cuisine cuisine)
		{
			if (Saved != null)
				Saved (this, new CuisineSavedEventArgs (cuisine));
		}

		void SaveSelected (string item)
		{
			Cuisine cuisine = Persist.Instance.Cuisines.Where (c => c.Title == item).SingleOrDefault ();
			if (cuisine != null)
				OnSaved (cuisine);
		}

		void DoListChoice (object s, ItemTappedEventArgs e)
		{
			string item = e.Item.ToString ();
			SaveSelected (item);
		}

		public EditCuisinePage (string cuisine)
		{
			Title = "Type of food";
			ListView list = new ListView ();
			list.ItemsSource = Persist.Instance.Cuisines;
			list.ItemTapped += DoListChoice;
			list.SelectedItem = Persist.Instance.Cuisines.Where (c => c.Title == cuisine).FirstOrDefault ();
			list.ScrollTo (list.SelectedItem, ScrollToPosition.Center, true);
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					list,
					tools
				}
			};
			if (!string.IsNullOrEmpty (cuisine)) {
				ToolbarItems.Add (new ToolbarItem {
					Text = " Next",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						SaveSelected ((list.SelectedItem as Cuisine).Title);
					})
				});
			}
		}
	}
}


