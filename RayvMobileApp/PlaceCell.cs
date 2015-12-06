using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class PlaceCell: ViewCell
	{
		private const int IMAGE_SIZE = 78;
		public string ShowFriend = "";
		EventHandler OnTapped;

		//views
		Label distLabel;

		#region Properties

		Place _place;

		public Place Place { get { return _place; } }

		bool _isShowingDistance;



		#endregion

		public PlaceCell (Place p, bool isShowingDistance, bool isShowingVotes, EventHandler onTapped) : base ()
		{
			_place = p;
			_isShowingDistance = isShowingDistance;
			Height = isShowingVotes ? 100 : 90;
			// Create views  for displaying each property.
			Label nameLabel = new Label {
				TextColor = Color.Black,
				BackgroundColor = Color.Transparent,
				FontAttributes = FontAttributes.Bold,
				LineBreakMode = LineBreakMode.TailTruncation,
			};

			Label catLabel = new Label ();
			catLabel.TextColor = Color.Black;
			catLabel.BackgroundColor = Color.Transparent;
			catLabel.FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label));

			distLabel = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
				FontAttributes = FontAttributes.Italic,
				TextColor = Color.Gray,
				BackgroundColor = Color.Transparent,
			};

			Label addressLabel = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
				FontAttributes = FontAttributes.Italic,
				TextColor = Color.Black,
				BackgroundColor = Color.Transparent,
			};
			addressLabel.LineBreakMode = LineBreakMode.TailTruncation;
			addressLabel.TextColor = Color.Black;

			Image webImage = new Image { 
				Aspect = Aspect.AspectFill,
				WidthRequest = IMAGE_SIZE, 
				HeightRequest = IMAGE_SIZE,
				TranslationY = 5,
				TranslationX = -3
			};

			StackLayout draftSign = new StackLayout {
				BackgroundColor = Color.White,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Children = {
					new Label { Text = "Draft", TextColor = Color.Red, BackgroundColor = Color.White },
				}
			};

			var distCuisineLine = new StackLayout { Orientation = StackOrientation.Horizontal };
			distCuisineLine.Children.Add (distLabel);

			Label priceLabel = new Label { 
				Text = $"• {p.vote.ShortPrice} •",
				TextColor = ColorUtil.Darker (settings.BaseColor),
				FontSize = settings.FontSizeLabelSmall,
				FontFamily = "Arial"
			}; 
			if (p.vote.style == PlaceStyle.None)
				priceLabel.Text = "";
			distCuisineLine.Children.Add (priceLabel);
			distCuisineLine.Children.Add (catLabel);
			Grid grid = new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Padding = 5,
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
			if (!isShowingVotes)
				grid.Children.Add (addressLabel, 0, 1, 2, 3);

			// votes are shown on the main list, not on the Add lists
			StarEditor Stars = new StarEditor (showUntried: true) {
				Height = 12,
				ReadOnly = true,
				IsInFriendMode = ShowFriend != ""
			};
			Label RatingLbl;
			StackLayout RatingLine;
			RatingLine = new StackLayout { Orientation = StackOrientation.Horizontal };
			RatingLbl = new Label { TextColor = settings.ColorDarkGray, FontSize = settings.FontSizeLabelMicro };
			RatingLine.Children.Add (Stars);
			RatingLine.Children.Add (RatingLbl);
			if (isShowingVotes) {
				grid.Children.Add (RatingLine, 0, 2, 2, 3);
			}
			View = grid;

			//LOAD VALUES
			nameLabel.Text = p.place_name;
			catLabel.Text = p.vote.cuisineName;
			distLabel.Text = p.distance;
			addressLabel.Text = AddressToShortAddressConverter.AddressToShortAddress (p.address);
			webImage.Source = p.thumb_url;
			draftSign.IsVisible = p.IsDraft;

			// votes are shown on the main list, not on the Add lists
			if (isShowingVotes) {
				int vote = PlaceKeyToCorrectVoteScoreForList.KeyToVote (p.key) ?? 0;
				if (vote > 0)
					Stars.Vote = vote;
				else
					Stars.Untried = PlaceKeyToCorrectUntriedForList.KeyToUntriedValue (p.key);
				RatingLbl.FormattedText = $"({p.Rating:F1})";
				RatingLine.Children.Add (Stars);
				RatingLine.Children.Add (RatingLbl);
				grid.Children.Add (RatingLine, 0, 2, 2, 3);
			}

			//TAPPED
			OnTapped = onTapped;
			this.Tapped += OnTapped;
		}
	}
}

