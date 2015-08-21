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

		public bool ShowAll {
			get;
			set;
		}

		public CuisineSavedEventArgs (Cuisine category, bool showAll)
		{
			_cuisine = category;
			ShowAll = showAll;
		}
	}

	public class EditCuisinePage : ContentPage
	{
		bool InFlow;

		public event EventHandler<CuisineSavedEventArgs> Saved;
		public event EventHandler Cancelled;

		protected virtual void OnSaved (Cuisine cuisine, bool showAll)
		{
			if (Saved != null)
				Saved (this, new CuisineSavedEventArgs (cuisine, showAll));
		}

		void SaveSelected (string item, bool showAll)
		{
			Cuisine cuisine = Persist.Instance.Cuisines.Where (c => c.Title == item).SingleOrDefault ();
			if (showAll || cuisine != null)
				OnSaved (cuisine, showAll);
		}

		void DoListChoice (object s, ItemTappedEventArgs e)
		{
			string item = e.Item.ToString ();
			SaveSelected (item, false);
		}

		public EditCuisinePage (string cuisine, bool inFlow = true, Page caller = null, bool showAllButton = false)
		{
			InFlow = inFlow;
			Title = "Type of food";
			ListView list = new ListView ();
			list.ItemsSource = Persist.Instance.Cuisines;
			list.ItemTapped += DoListChoice;
			list.SelectedItem = Persist.Instance.Cuisines.Where (c => c.Title == cuisine).FirstOrDefault ();
			list.ScrollTo (list.SelectedItem, ScrollToPosition.Center, true);
			StackLayout tools = new BottomToolbar (this, "add");
			RayvButton AllBtn = new RayvButton ("All Kinds") {
				IsVisible = showAllButton
			};
			AllBtn.OnClick += (s, e) => {
				SaveSelected (null, true);
			};
			Content = new StackLayout {
				Children = {
					list,
					AllBtn,
					tools
				}
			};
			if (!string.IsNullOrEmpty (cuisine)) {
				ToolbarItems.Add (new ToolbarItem {
					Text = InFlow ? " Next " : "  Cancel  ",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						if (InFlow)
							SaveSelected ((list.SelectedItem as Cuisine).Title, false);
						else
							Cancelled?.Invoke (this, null);
					})
				});
			}
			if (caller != null) {
				ToolbarItems.Add (new ToolbarItem {
					Text = "  Back  ",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						Navigation.PopModalAsync ();
						Cancelled?.Invoke (this, null);
					})
				});
			}
		}
	}
}


