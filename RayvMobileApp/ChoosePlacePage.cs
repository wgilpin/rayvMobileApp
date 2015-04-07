using System;
using Xamarin.Forms;
using System.Linq;
using Xamarin.Forms.Maps;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{
	public class ChoosePlacePage : ContentPage
	{
		AddMenu _caller;
		Entry locationName;

		public ChoosePlacePage (AddMenu caller)
		{
			_caller = caller;
			locationName = new Entry { 
				Placeholder = "Location",
			};
			RayvButton hereBtn = new RayvButton {
				Text = " Search Here ",
			};
			hereBtn.Clicked += SearchHere;

			StackLayout history = new StackLayout ();
			if (Persist.Instance.SearchHistory.Length == 0) {
				history.Children.Add (new LabelWide {
					Text = "No History",
				});
			} else {
				for (int i = 0; i <= Persist.Instance.SearchHistory.Length; i++) {
					Button clickItem = new Button {
						Text = Persist.Instance.SearchHistory.GetItem (i),
						HorizontalOptions = LayoutOptions.Center
					};
					clickItem.Clicked += (object sender, EventArgs e) => {
						locationName.Text = (sender as Button).Text;
						SearchHere (null, null);
					};
					history.Children.Add (clickItem);
				}
			}
			Frame historyFrame = new Frame {
				OutlineColor = Color.Silver,
				Content = history,
			};
			Content = new StackLayout {
				Children = {
					locationName,
					historyFrame,
					hereBtn,
				}	
			};
		}

		async void SearchHere (object sender, EventArgs e)
		{
			// geocode place name
			var positions = (await (new Geocoder ()).GetPositionsForAddressAsync (locationName.Text)).ToList ();
			Console.WriteLine ("SearchHere: Got");
			Persist.Instance.SearchHistory.Add (locationName.Text);
			// save and return
			_caller.searchPosition = positions.First ();
			Debug.WriteLine ("ChoosePlacePage.SearchHere: Pop");
			await this.Navigation.PopAsync ();
		}
	}
}

