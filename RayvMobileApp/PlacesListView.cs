﻿using System;
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

				Grid grid = new Grid {
					VerticalOptions = LayoutOptions.FillAndExpand,
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						//						new RowDefinition { Height = new GridLength (19, GridUnitType.Absolute)  },
						//						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
						//						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (IMAGE_SIZE, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength (70, GridUnitType.Absolute) },
					}
				};
				grid.Children.Add (webImage, 0, 1, 0, 3);
				grid.Children.Add (draftSign, 0, 1, 0, 3);
				grid.Children.Add (nameLabel, 1, 2, 0, 1);
				grid.Children.Add (catLabel, 1, 2, 1, 2);
				grid.Children.Add (distLabel, 1, 2, 2, 3);
				if (!ShowVotes)
					grid.Children.Add (addressLabel, 1, 3, 2, 3);

				// votes are shown on the main list, not on the Add lists
				if (ShowVotes) {

					Label upVotes = new Label {
						TextColor = ColorUtil.Darker (settings.BaseColor),
						BackgroundColor = Color.Transparent,
						XAlign = TextAlignment.End,
						HorizontalOptions = LayoutOptions.End,
						TranslationX = 2,
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					};
					upVotes.SetBinding (Label.IsVisibleProperty, "noVote");
					upVotes.SetBinding (
						Label.TextProperty, 
						new Binding ("key", converter: new KeyToUpVotersConverter ()));
					Label downVotes = new Label {
						HorizontalOptions = LayoutOptions.End,
						TextColor = Color.FromHex ("A22"),
						BackgroundColor = Color.Transparent,
						XAlign = TextAlignment.End,
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
						TranslationY = 2,
						TranslationX = 2,
					};

					downVotes.SetBinding (
						Label.TextProperty, 
						new Binding ("key", converter: new KeyToDownVotersConverter ()));

					downVotes.SetBinding (
						Label.IsVisibleProperty, 
						new Binding ("key", converter: new KeyToShowDownBoolConverter ()));


					Label myVote = new Label {
						BackgroundColor = Color.Transparent,
						TextColor = ColorUtil.Darker (settings.BaseColor),
						TranslationY = -12,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						XAlign = TextAlignment.End,
					};
					myVote.SetBinding (
						Label.IsVisibleProperty, 
						new Binding ("key", converter: new KeyToShowMyVoteConverter ()));
					myVote.SetBinding (
						Label.TextProperty, 
						new Binding ("key", converter: new KeyToMyVoteTextConverter ()));
					myVote.SetBinding (
						Label.TextColorProperty, 
						new Binding ("vote", converter: new VoteToColorConverter ()));
					myVote.SetBinding (
						Label.FontSizeProperty,
						new Binding ("key", converter: new KeyToMyVoteSizeConverter ()));


					Label noVoteLiked = new Label {
						Text = "Liked",
						BackgroundColor = Color.Transparent,
						TextColor = Color.FromHex ("888"),
						FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					};
					Label noVoteDisliked = new Label {
						BackgroundColor = Color.Transparent,
						TextColor = Color.FromHex ("822"),
						Text = "Disliked",
						FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
						TranslationY = -2,
					};
					noVoteLiked.SetBinding (Label.IsVisibleProperty, "noVote");
					noVoteDisliked.SetBinding (
						Label.IsVisibleProperty, 
						new Binding ("down", converter: new KeyToShowDownBoolConverter ()));
					grid.Children.Add (new Frame { 
						HasShadow = false, 
						Content = upVotes, 
						Padding = new Thickness (3, 0), 
					}, 2, 3, 0, 3);
					grid.Children.Add (downVotes, 2, 3, 1, 3);
					grid.Children.Add (myVote, 2, 3, 1, 4);
					//					grid.Children.Add (noVoteLiked, 3, 4, 1, 2);
					//					grid.Children.Add (noVoteDisliked, 3, 4, 2, 3);
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

