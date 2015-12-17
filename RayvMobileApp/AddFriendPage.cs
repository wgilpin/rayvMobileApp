using System;

using Xamarin.Forms;
using Xamarin;

namespace RayvMobileApp
{
	public class AddFriendPage : ContentPage
	{
		void DoSendInvite (object s, EventArgs e)
		{
			try {
				var resp = Persist.Instance.GetWebConnection ().get ("/api/invite");
				if (resp.ResponseStatus == RestSharp.ResponseStatus.Error) {
					DisplayAlert ("Error", "Unable to create invite", "OK");
					return;
				}
				string uri = resp.Content;
				var sharer = DependencyService.Get<IShareable> ();
				var shareBody = 
					"I'd like to invite you to use the Taste 5 app so we can share the restaurants and cafes we like.\n" +
					"Using your phone, click on this link to join me!\n\n" +
					uri;
				sharer.OpenShareIntent (shareBody);
			} catch (Exception ex) {
				DisplayAlert ("Error", "Invite Error", "OK");
				Insights.Report (ex);
			}
		}

		public AddFriendPage ()
		{
			Analytics.TrackPage ("AddWhatPage");
			Grid grid = new Grid {
				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				VerticalOptions = LayoutOptions.EndAndExpand,
				RowSpacing = 10,
				ColumnSpacing = 0,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (100, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (120, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (40, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (100, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (120, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (2, GridUnitType.Auto) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			Image findUserImg = new Image {
				Source = settings.DevicifyFilename ("Find_friend.png"),
				Aspect = Aspect.AspectFit,
				HeightRequest = 80,
			};
			var clickFind = new TapGestureRecognizer ();
			clickFind.Tapped += (s, e) => {
				Console.WriteLine ("MainMenu: Add button - push AddMenu");
				var addPage = new AddFindUserPage ();
				addPage.Failed += async (o, ev) => {
					await Navigation.PopAsync ();
					if (await DisplayAlert ("Failed", "Message could not be sent. Try with a phone app?", "Yes", "Cancel")) {
						DoSendInvite (this, null);
					}
				};
				addPage.Succeeded += (o2, e2) => {
					Navigation.PushModalAsync (new RayvNav (new MainMenu ()));
				};
				this.Navigation.PushAsync (addPage);
			};
			findUserImg.GestureRecognizers.Add (clickFind);

			// FRIENDS
			Image messageExternalImg = new Image {
				Source = settings.DevicifyFilename ("Invite_friend2.png"),
				Aspect = Aspect.AspectFit,
				HeightRequest = 80,
			};
			var clickMessage = new TapGestureRecognizer ();
			clickMessage.Tapped += DoSendInvite;
			messageExternalImg.GestureRecognizers.Add (clickMessage);


			var sendMessageTxt = new StackLayout { 
				VerticalOptions = LayoutOptions.End, 
				TranslationY = -40,
				Children = { 
					new Label {
						Text = "Send invite",
						FontSize = 35,
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Color.White,
					},
				}
			};
			sendMessageTxt.GestureRecognizers.Add (clickMessage);
			var addUserTxt = new StackLayout { 
				VerticalOptions = LayoutOptions.End, 
				TranslationY = -40,
				Children = { 
					new Label {
						Text = "Connect to a user",
						FontSize = 35,
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Color.White,
					},
				}
			};
			addUserTxt.GestureRecognizers.Add (clickFind);
			grid.Children.Add (findUserImg, 0, 0);
			grid.Children.Add (addUserTxt, 0, 1);
			grid.Children.Add (messageExternalImg, 0, 3);
			grid.Children.Add (sendMessageTxt, 0, 4);
			Content = grid;
			BackgroundColor = settings.BaseColor;
			Appearing += (sender, e) => {
				grid.RowDefinitions [2].Height = new GridLength ((Height - 250) / 6, GridUnitType.Absolute);
				Console.WriteLine (Height / 10);
			};
		}
	}
}


