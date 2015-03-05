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
				catLabel.Font = Font.SystemFontOfSize (NamedSize.Small);

				Label distLabel = new Label ();
				distLabel.Font = Font.SystemFontOfSize (NamedSize.Small);
				distLabel.SetBinding (Label.TextProperty, "distance");

				Label addressLabel = new Label {
					Font = Font.SystemFontOfSize (NamedSize.Micro),
					FontAttributes = FontAttributes.Italic,
					TranslationX = 65,
				};
				addressLabel.SetBinding (Label.TextProperty, "address");


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
						new ColumnDefinition { Width = new GridLength (12, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (50, GridUnitType.Absolute) }
					}
				};
				grid.Children.Add (webImage, 0, 1, 0, 3);
				grid.Children.Add (nameLabel, 1, 4, 0, 1);
				grid.Children.Add (catLabel, 1, 2, 1, 2);
				grid.Children.Add (distLabel, 1, 2, 2, 3);
				if (!ShowVotes)
					grid.Children.Add (addressLabel, 1, 4, 2, 3);


				// votes are shown on the main list, not on the Add lists
				if (ShowVotes) {

					Label upVotes = new Label {
						TextColor = settings.ColorLight,
						XAlign = TextAlignment.End,
						TranslationX = -6,
						HorizontalOptions = LayoutOptions.End,
					};
					upVotes.SetBinding (Label.IsVisibleProperty, "noVote");
					upVotes.SetBinding (Label.TextProperty, "up");

					Label downVotes = new Label {
						HorizontalOptions = LayoutOptions.End,
						TextColor = Color.FromRgb (255, 128, 128),
						XAlign = TextAlignment.End,
						TranslationX = -6,
					};
					downVotes.SetBinding (Label.IsVisibleProperty, "noVote");
					downVotes.SetBinding (Label.TextProperty, "down");


					Label myVote = new Label {
						TextColor = settings.ColorDark,
					};
					myVote.SetBinding (Label.IsVisibleProperty, "iVoted");
					myVote.SetBinding (Label.TextProperty, "voteImage");


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
						TextColor = Color.FromHex ("888"),
						Text = "Disliked",
						FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					};
					noVoteLiked.SetBinding (Label.IsVisibleProperty, "noVote");
					noVoteDisliked.SetBinding (Label.IsVisibleProperty, "noVote");
					smileysImg.SetBinding (Label.IsVisibleProperty, "noVote");
					grid.Children.Add (upVotes, 2, 3, 1, 2);
					grid.Children.Add (downVotes, 2, 3, 2, 3);
					grid.Children.Add (myVote, 2, 4, 1, 2);
					grid.Children.Add (noVoteLiked, 3, 4, 1, 2);
					grid.Children.Add (noVoteDisliked, 3, 4, 2, 3);
				}



				return new ViewCell {
					View = grid,
				};
				// Return an assembled ViewCell.
				return new ViewCell {
					View = new StackLayout {
						Padding = new Thickness (1, 1),
						Spacing = 1,
						Orientation = StackOrientation.Horizontal,
						Children = {
							webImage,
							new StackLayout {
								Spacing = 0,
								Orientation = StackOrientation.Vertical,
								Children = {
									nameLabel,
									catLabel,
									distLabel,
								}
							}
						}

					}
				};
			});
			VerticalOptions = LayoutOptions.FillAndExpand;
		}

		public PlacesListView (bool showVotes) : this ()
		{
			ShowVotes = showVotes;
		}
	}
}

