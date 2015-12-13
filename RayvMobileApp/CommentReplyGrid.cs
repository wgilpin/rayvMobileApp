using System;
using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin;

namespace RayvMobileApp
{
	public class CommentReplyGrid: Grid
	{

		public CommentReplyGrid (VoteComment reply) : base ()
		{
			Padding = 2;
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (RoundButton.letterButtonSize + 1) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (RoundButton.letterButtonSize + 1) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Auto) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (RoundButton.letterButtonSize + 1) });

			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			var bg = new BoxView (){ BackgroundColor = settings.ColorLightGray, TranslationX = -2, TranslationY = -2 };
			Children.Add (bg, left: 1, right: 5, top: 0, bottom: 2);

			try {
				string author = reply.GetVoterName ();
				RoundButton LetterBtn = new RoundButton {
					BackgroundColor = Vote.RandomColor (author),
					Text = Vote.FirstLetter (author),
				};


				var FriendLine = new FormattedString ();
				LetterBtn.Text = Vote.FirstLetter (author);
				LetterBtn.BackgroundColor = Vote.RandomColor (author);
				if (author == "Me") {
					LetterBtn.BackgroundColor = Color.Black;
					LetterBtn.Text = "Me";
					LetterBtn.FontSize = settings.FontSizeButtonMedium;
					LetterBtn.Roundness = Roundness.Rectangular;
					Children.Add (LetterBtn, 1, 0);
				} else {
					Children.Add (LetterBtn, 1, 0);

					
					string voter = "";
					try {
					} catch (Exception ex) {
						var data = new Dictionary<string,string> { 
							{ "Friend", $"{author}" },
							{ "Vote",$"{reply.CommentId}" }
						};
						Insights.Report (ex, data);
						throw new KeyNotFoundException ();
					}
					FriendLine.Spans.Add (new Span{ Text = voter, });
				}
				FriendLine.Spans.Add (new Span{ Text = " " });
				FriendLine.Spans.Add (new Span {
					Text = reply.PrettyHowLongAgo (),
					FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					ForegroundColor = Color.FromHex ("#606060"),
				});
				Children.Add (new Label { FormattedText = FriendLine }, 2, 0);
					
				String comment_text = reply.PrettyComment;
				if (!String.IsNullOrEmpty (comment_text)) {
					var commentLbl = new Label {
						Text = comment_text,
						TextColor = settings.ColorDarkGray,
						FontAttributes = FontAttributes.Italic,

					};
					Children.Add (commentLbl, left: 1, right: 5, top: 1, bottom: 2);
				}

			} catch (KeyNotFoundException) {
				// already handled
			} catch (Exception ex) {
				Console.WriteLine ("CommentViewGrid {0}", ex);
				Insights.Report (ex);
			}
		}
	}
}

