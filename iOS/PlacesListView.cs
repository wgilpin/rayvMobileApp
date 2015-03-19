using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{

	public class PlacesListView : ListView
	{
		private const int IMAGE_SIZE = 63;
		private bool ShowVotes;

		public PlacesListView () : base ()
		{
			Console.WriteLine ("PlacesListView()");
			ShowVotes = true;
			RowHeight = 65;

			ItemTemplate = new DataTemplate (() => {
				// Create views with bindings for displaying each property.
				Label nameLabel = new Label ();
				nameLabel.FontAttributes = FontAttributes.Bold;
				nameLabel.LineBreakMode = LineBreakMode.TailTruncation;
				nameLabel.SetBinding (Label.TextProperty, "place_name");

				Label catLabel = new Label ();
				catLabel.SetBinding (Label.TextProperty, "category");
				catLabel.FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label));

				Label distLabel = new Label ();
				distLabel.FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label));
				distLabel.SetBinding (Label.TextProperty, "distance");

				Label addressLabel = new Label {
					FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					TranslationX = 85,
				};
				//addressLabel.SetBinding (Label.TextProperty, "ShortAddress");
				addressLabel.SetBinding (
					Label.TextProperty, 
					new Binding ("address", converter: new AddressToShortAddressConverter ()));

				Image webImage = new Image { 
					Aspect = Aspect.AspectFill,
					WidthRequest = IMAGE_SIZE, 
					HeightRequest = IMAGE_SIZE
				};
//				Binding x = new Binding ("img");
				webImage.SetBinding (Image.SourceProperty, "thumb_url");

				Grid grid = new Grid {
					VerticalOptions = LayoutOptions.FillAndExpand,
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (19, GridUnitType.Absolute)  },
						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (IMAGE_SIZE, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength (70, GridUnitType.Absolute) },
					}
				};
				grid.Children.Add (webImage, 0, 1, 0, 3);
				grid.Children.Add (nameLabel, 1, 3, 0, 1);
				grid.Children.Add (catLabel, 1, 2, 1, 2);
				grid.Children.Add (distLabel, 1, 2, 2, 3);
				if (!ShowVotes)
					grid.Children.Add (addressLabel, 1, 3, 2, 3);


				// votes are shown on the main list, not on the Add lists
				if (ShowVotes) {

					Label upVotes = new Label {
						TextColor = settings.ColorLight,
						XAlign = TextAlignment.End,
						HorizontalOptions = LayoutOptions.End,
						TranslationX = 2,
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					};
					upVotes.SetBinding (Label.IsVisibleProperty, "noVote");
//					upVotes.SetBinding (Label.TextProperty, "up");
					upVotes.SetBinding (
						Label.TextProperty, 
						new Binding ("key", converter: new KeyToUpVotersConverter ()));

					Label downVotes = new Label {
						HorizontalOptions = LayoutOptions.End,
						TextColor = Color.FromRgb (255, 128, 128),
						XAlign = TextAlignment.End,
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
						TranslationY = 2,
					};
//					downVotes.SetBinding (Label.IsVisibleProperty, "noVote");
//					downVotes.SetBinding (Label.TextProperty, "down");
					downVotes.SetBinding (
						Label.TextProperty, 
						new Binding ("key", converter: new KeyToDownVotersConverter ()));
					
					downVotes.SetBinding (
						Label.IsVisibleProperty, 
						new Binding ("key", converter: new KeyToShowDownBoolConverter ()));


					Label myVote = new Label {
						TextColor = settings.ColorDark,
						TranslationY = -5,
					};
					myVote.SetBinding (Label.IsVisibleProperty, "iVoted");
					myVote.SetBinding (Label.TextProperty, "voteImage");
					myVote.SetBinding (
						Label.TextColorProperty, 
						new Binding ("vote", converter: new VoteToColorConverter ()));


					Image myVoteImg = new Image { 
						Aspect = Aspect.AspectFit,
						WidthRequest = 28, 
						HeightRequest = 28,
						TranslationX = 0,
					};
					myVoteImg.SetBinding (Image.SourceProperty, "voteImage");
					myVoteImg.SetBinding (Label.IsVisibleProperty, "iVoted");


					Image smileysImg = new Image { 
						Aspect = Aspect.AspectFit,
						WidthRequest = 20, 
						HeightRequest = 32,
						Source = "two-smileys-lg.png",
						TranslationX = 0,
					};
					Label noVoteLiked = new Label {
						Text = "Liked",
						TextColor = Color.FromHex ("888"),
						FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					};
					Label noVoteDisliked = new Label {
						TextColor = Color.FromHex ("822"),
						Text = "Disliked",
						FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
						TranslationY = -2,
					};
					noVoteLiked.SetBinding (Label.IsVisibleProperty, "noVote");
					noVoteDisliked.SetBinding (
						Label.IsVisibleProperty, 
						new Binding ("down", converter: new KeyToShowDownBoolConverter ()));
					smileysImg.SetBinding (Label.IsVisibleProperty, "noVote");
					grid.Children.Add (new Frame { HasShadow = false, Content = upVotes, Padding = new Thickness (3, 0), }, 2, 3, 0, 3);
					grid.Children.Add (downVotes, 2, 3, 1, 3);
					grid.Children.Add (myVote, 2, 3, 1, 3);
//					grid.Children.Add (noVoteLiked, 3, 4, 1, 2);
//					grid.Children.Add (noVoteDisliked, 3, 4, 2, 3);
				}



				return new ViewCell {
					View = grid,
				};
				// Return an assembled ViewCell.
			});
			VerticalOptions = LayoutOptions.FillAndExpand;
		}

		public PlacesListView (bool showVotes) : this ()
		{
			ShowVotes = showVotes;
		}
	}
}

