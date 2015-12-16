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

	public class EditCuisineView : StackLayout
	{
		bool InFlow;

		public event EventHandler<CuisineSavedEventArgs> Saved;
		public event EventHandler Cancelled;
		public event EventHandler NoCuisine;

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

		public EditCuisineView (string cuisine, bool inFlow = true, bool showAllButton = false)
		{
			InFlow = inFlow;
			ListView list = new ListView ();
			list.ItemsSource = Persist.Instance.Cuisines;
			list.ItemTapped += DoListChoice;
			list.SelectedItem = Persist.Instance.Cuisines.Where (c => c.Title == cuisine).FirstOrDefault ();
			list.ScrollTo (list.SelectedItem, ScrollToPosition.Center, true);
			RayvButton AllBtn = new RayvButton ("All Kinds") {
				IsVisible = showAllButton
			};
			AllBtn.OnClick += (s, e) => {
				SaveSelected (null, true);
			};
			Children.Add (list);
			Children.Add (AllBtn);
			if (inFlow) {
				var buttons = new DoubleImageButton { 
					LeftText = "Back", 
					RightText = "Next",
					LeftSource = "back_1.png",
					RightSource = "forward_1.png"
				};
				buttons.LeftClick = (s, e) => Cancelled?.Invoke (this, null);
				buttons.RightClick = (s, e) => {
					if (InFlow) {
						if (list.SelectedItem != null)
							SaveSelected ((list.SelectedItem as Cuisine).Title, false);
						else {
							NoCuisine?.Invoke (this, null);
						}
					} else
						Cancelled?.Invoke (this, null);
				};
				Children.Add (buttons);
			}

		}
	}
}


