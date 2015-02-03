using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{

	public class PlacesListView : ListView
	{
		private const int IMAGE_SIZE = 63;

		public PlacesListView () : base ()
		{
			Console.WriteLine ("PlacesListView()");
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

				Image webImage = new Image { 
					Aspect = Aspect.AspectFill,
					WidthRequest = IMAGE_SIZE, 
					HeightRequest = IMAGE_SIZE
				};
//				Binding x = new Binding ("img");
				webImage.SetBinding (Image.SourceProperty, "thumb_url");


				Label upVotes = new Label ();
				upVotes.SetBinding (Label.IsVisibleProperty, "untried");
				upVotes.SetBinding (Label.TextProperty, "up");

				Label downVotes = new Label ();
				downVotes.TextColor = Color.Red;
				downVotes.SetBinding (Label.IsVisibleProperty, "untried");
				downVotes.SetBinding (Label.TextProperty, "down");


				Label myVote = new Label ();
				myVote.TextColor = Color.Blue;
				myVote.SetBinding (Label.IsVisibleProperty, "iVoted");
				myVote.SetBinding (Label.TextProperty, "voteImage");

				Image myVoteImg = new Image { 
					Aspect = Aspect.AspectFit,
					WidthRequest = 28, 
					HeightRequest = 28
				};
				myVoteImg.SetBinding (Image.SourceProperty, "voteImage");

				Grid grid = new Grid {
					VerticalOptions = LayoutOptions.FillAndExpand,
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (16, GridUnitType.Absolute)  },
						new RowDefinition { Height = new GridLength (16, GridUnitType.Absolute)  },
						new RowDefinition { Height = new GridLength (16, GridUnitType.Absolute)  },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (IMAGE_SIZE, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength (14, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (20, GridUnitType.Absolute) }
					}
				};
				grid.Children.Add (webImage, 0, 1, 0, 3);
				grid.Children.Add (nameLabel, 1, 2, 0, 1);
				grid.Children.Add (catLabel, 1, 2, 1, 2);
				grid.Children.Add (distLabel, 1, 2, 2, 3);
				grid.Children.Add (upVotes, 2, 3, 0, 1);
				grid.Children.Add (downVotes, 2, 3, 2, 3);
				grid.Children.Add (myVoteImg, 3, 4, 0, 3);



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
	}
}

