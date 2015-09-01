using System;
using Xamarin.Forms;

namespace RayvMobileApp
{

	public class PlacesListView : ListView
	{
		private const int IMAGE_SIZE = 78;
		private bool ShowVotes;
		bool showingDistance = true;

		public bool IsShowingDistance {
			get { return showingDistance; }
			set {
				showingDistance = value;
				ItemTemplate = GetTemplate ();
			}
		}

		DataTemplate GetTemplate ()
		{
			return new DataTemplate (() => {
				// Create views with bindings for displaying each property.
				BackgroundColor = Color.White;
				Label nameLabel = new Label {
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
					FontAttributes = FontAttributes.Bold,
					LineBreakMode = LineBreakMode.TailTruncation,
				};
				nameLabel.SetBinding (Label.TextProperty, "place_name");

				Label catLabel = new Label ();
				catLabel.SetBinding (Label.TextProperty, "vote.cuisineName");
				catLabel.TextColor = Color.Black;
				catLabel.BackgroundColor = Color.Transparent;
				catLabel.FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label));

				Label distLabel = new Label {
					FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					TextColor = Color.Gray,
					BackgroundColor = Color.Transparent,
				};
				if (IsShowingDistance)
					distLabel.SetBinding (Label.TextProperty, "distance");

				Label addressLabel = new Label {
					FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					TranslationX = 85,
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
				};
				addressLabel.SetBinding (
					Label.TextProperty, 
					new Binding ("address", converter: new AddressToShortAddressConverter ()));
				addressLabel.TextColor = Color.Black;

				Image webImage = new Image { 
					Aspect = Aspect.AspectFill,
					WidthRequest = IMAGE_SIZE, 
					HeightRequest = IMAGE_SIZE
				};
				webImage.SetBinding (Image.SourceProperty, "thumb_url");

				StackLayout draftSign = new StackLayout {
					BackgroundColor = Color.White,
					VerticalOptions = LayoutOptions.CenterAndExpand,
					Children = {
						new Label { Text = "Draft", TextColor = Color.Red, BackgroundColor = Color.White },
					}
				};
				draftSign.SetBinding (
					StackLayout.IsVisibleProperty, "IsDraft");

				var distCuisineLine = new StackLayout { Orientation = StackOrientation.Horizontal };
				distCuisineLine.Children.Add (distLabel);
				distCuisineLine.Children.Add (new Label{ Text = " " });
				distCuisineLine.Children.Add (catLabel);
				Grid grid = new Grid {
					VerticalOptions = LayoutOptions.FillAndExpand,
					Padding = 2,
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						//						new RowDefinition { Height = new GridLength (19, GridUnitType.Absolute)  },
						//						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
						//						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength (70, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (IMAGE_SIZE, GridUnitType.Absolute) },
					}
				};
				grid.Children.Add (webImage, 2, 3, 0, 3);
				grid.Children.Add (draftSign, 2, 3, 0, 3);
				grid.Children.Add (nameLabel, 0, 2, 0, 1);
				grid.Children.Add (distCuisineLine, 0, 2, 1, 2);
				if (!ShowVotes)
					grid.Children.Add (addressLabel, 0, 2, 2, 3);

				// votes are shown on the main list, not on the Add lists
				if (ShowVotes) {
					var Stars = new StarEditor (false) { Height = 15, ReadOnly = true };
					Stars.SetBinding (StarEditor.VoteProperty, "vote.vote");
					Stars.SetBinding (StackLayout.IsVisibleProperty, "iVoted");
					grid.Children.Add (Stars, 0, 2, 2, 3);
					var ratingLine = new StackLayout { Orientation = StackOrientation.Horizontal };
					var rating = new Label { TextColor = settings.BaseColor, FontSize = settings.FontSizeLabelSmall };
					rating.SetBinding (Label.FormattedTextProperty, "Rating", stringFormat: "{0:F1}");
					ratingLine.SetBinding (Label.IsVisibleProperty, "noVote");
					ratingLine.Children.Add (rating);
					ratingLine.Children.Add (new Image { 
						Source = "star-empty.png", 
						WidthRequest = 15, 
						Aspect = Aspect.AspectFit,
						TranslationY = -5
					});
					grid.Children.Add (ratingLine, 0, 2, 2, 3);
					var untriedImg = new Image { 
						Source = "wish_blue.png", 
						Aspect = Aspect.AspectFit, 
						HeightRequest = 20,
						HorizontalOptions = LayoutOptions.End,
					};
					untriedImg.SetBinding (Image.IsVisibleProperty, "vote.untried");
					grid.Children.Add (untriedImg, 1, 2, 0, 1);
				}
				return new ViewCell {
					View = grid,
				};
				// Return an assembled ViewCell.
			});
		}

		public PlacesListView (bool showVotes = true, bool showDistance = true) : base ()
		{
			Console.WriteLine ("PlacesListView()");
			ShowVotes = showVotes;
			RowHeight = 80;

			//isShowingDistance setter also sets the data template
			IsShowingDistance = showDistance;
			VerticalOptions = LayoutOptions.FillAndExpand;
		}


	}
}

