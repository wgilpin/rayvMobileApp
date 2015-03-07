using System;

using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class AddWhatPage : ContentPage
	{
		public AddWhatPage ()
		{
			Image addImg = new Image {
				Source = "big-btn-add-place.png"
			};
			var clickAdd = new TapGestureRecognizer ();
			clickAdd.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				this.Navigation.PushModalAsync (new NavigationPage (new AddMenu ()));
			};
			addImg.GestureRecognizers.Add (clickAdd);

			// FRIENDS
			Image friendsImg = new Image {
				Source = "big-btn-add-friend.png"
			};
			var clickFriends = new TapGestureRecognizer ();
			clickFriends.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: friends button - not implemented");
				DisplayAlert ("Friends", "Not Implemented (yet)", "Shame");
			};
			friendsImg.GestureRecognizers.Add (clickFriends);


			StackLayout buttons = new StackLayout { 
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					addImg,
					friendsImg,
				}
			};
			StackLayout tools = new BottomToolbar (this, "add");
			Content = new StackLayout {
				Children = {
					buttons,
					tools
				}
			};
		}
	}
}


