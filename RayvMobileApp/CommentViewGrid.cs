using System;
using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin;
using System.Linq;

namespace RayvMobileApp
{
	public class CommentViewGrid:Grid
	{
		
		static int StarSize = 15;

		void ReplyButtonClicked (Object o, EventArgs e)
		{
			ImageButton btn = o as ImageButton;
			Vote vote = Persist.Instance.Votes.Where (v => v.Id == Convert.ToInt32 (btn.StyleId)).FirstOrDefault ();
			if (vote != null)
				DoReply?.Invoke (o, null);
		}

		EventHandler DoReply;

		public CommentViewGrid (Vote vote, EventHandler onReply, bool showReplyBtn, bool showUntried = false) : base ()
		{
			DoReply = onReply;
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (RoundButton.letterButtonSize + 1) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Auto) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (RoundButton.letterButtonSize + 1) });
			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
//			DebugUtils.AddLinesToGrid (this);
			var letterFontSize = Device.OnPlatform (
				                     settings.FontSizeButtonLarge,
				                     settings.FontSizeButtonMedium,
				                     settings.FontSizeButtonLarge);
			try {

				RoundButton LetterBtn = new RoundButton {
					BackgroundColor = Vote.RandomColor (vote.VoterName),
					Text = Vote.FirstLetter (vote.VoterName),
				};


				LetterBtn.Text = Vote.FirstLetter (vote.VoterName);
				LetterBtn.BackgroundColor = Vote.RandomColor (vote.VoterName);
				Children.Add (LetterBtn, 0, 0);
				var FriendLine = new FormattedString ();

				string voter = "";
				if (vote.voter == Persist.Instance.MyId.ToString ()) {
					LetterBtn.BackgroundColor = Color.Black;
					LetterBtn.FontSize = settings.FontSizeButtonMedium;
					LetterBtn.Roundness = Roundness.Rectangular;
					LetterBtn.Text = "Me";
				} else
					try {
						voter = Persist.Instance.Friends [vote.voter].Name;
					} catch (Exception ex) {
						var data = new Dictionary<string,string> { 
							{ "Friend", $"{vote.voter}" },
							{ "Vote",$"{vote.Id}" }
						};
						Insights.Report (ex, data);
						throw new KeyNotFoundException ();
					}

				var friendStars = new StarEditor (showUntried) {
					ReadOnly = true,
					Height = StarSize,
					Vote = vote.vote,
					Untried = vote.untried
				};
				Children.Add (friendStars, 2, 4, 0, 1);
				FriendLine.Spans.Add (new Span{ Text = voter, });
				FriendLine.Spans.Add (new Span{ Text = " " });
				FriendLine.Spans.Add (new Span {
					Text = vote.PrettyHowLongAgo,
					FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					ForegroundColor = Color.FromHex ("#606060"),
				});
				Children.Add (new Label { FormattedText = FriendLine }, 1, 0);
				String comment_text = vote.PrettyComment;
				if (!String.IsNullOrEmpty (comment_text)) {
					var commentLbl = new Label {
						Text = comment_text,
						TextColor = settings.ColorDarkGray,
						FontAttributes = FontAttributes.Italic,

					};
					commentLbl.LineBreakMode = showReplyBtn ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
					Children.Add (commentLbl, left: 0, right: showReplyBtn ? 3 : 4, top: 1, bottom: 2);
				}
				if (showReplyBtn) {
					var replyBtn = new RoundButton (vote.replies == 0) {
						StyleId = vote.voteId.ToString (), 
						Text = vote.replies.ToString (),
						BackgroundColor = settings.BaseDarkColor
					};
					Children.Add (replyBtn, left: 3, top: 1);
				}
				//styleId is theSQLlite ID of the vote, for use in the event handler
				StyleId = vote.voteId.ToString ();
				var click = new TapGestureRecognizer ();
				click.Tapped += onReply;
				GestureRecognizers.Add (click);
			} catch (KeyNotFoundException) {
				// already handled
			} catch (Exception ex) {
				Console.WriteLine ("CommentViewGrid {0}", ex);
				Insights.Report (ex);
			}

		}
	}
}

