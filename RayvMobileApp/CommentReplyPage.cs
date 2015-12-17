using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin;
using System.Linq;
using System.Threading.Tasks;

namespace RayvMobileApp
{
	using CommentList = List<VoteComment>;

	public class CommentReplyPage : ContentPage
	{
		ColouredButton ReplyBtn;
		DoubleButton SaveBtn;
		Vote CurrentVote;
		Editor textEditor;
		ScrollView repliesScrollView;
		StackLayout innerRepliesStack;

		public EventHandler Finished;

		void DoSave (Object sender, EventArgs ev)
		{
			try {
				string text = textEditor.Text.Trim ();
				var parms = new Dictionary<string,string> {
					{ "author",Persist.Instance.MyId.ToString () },
					{ "comment",text },
					{ "vote",CurrentVote.voteId.ToString () },
					{ "when",DateTime.UtcNow.ToString ("s") }
				};
				var res = Persist.Instance.GetWebConnection ().post ("/api/comment", parms);
				if (res == "OK") {
					Navigation.PopAsync ();
					Finished.Invoke (this, null);
				} else {
					DisplayAlert ("Error", "Save failed", "OK");
				}
			} catch (Exception ex) {
				Console.WriteLine ($"CommentReplyPage.DoSave ERROR {ex}");
				Insights.Report (ex);
			}
		}

		void DoStartReply (Object sender, EventArgs ev)
		{
			// set edit view	
			textEditor = new Editor { 
				VerticalOptions = LayoutOptions.FillAndExpand,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Editor))
			};
			textEditor.Completed += (a, b) => textEditor.Unfocus ();
			textEditor.Keyboard = Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions);
			Content = new StackLayout {
				Children = {
					new CommentViewGrid (CurrentVote, onReply: null, showReplyBtn: false),
					new Frame { Padding = 5,  VerticalOptions = LayoutOptions.FillAndExpand, Content = textEditor, HasShadow = true },
					SaveBtn
				}
			};
			textEditor.Focus ();
		}

		void DoCancelSave (object sender, EventArgs e)
		{
			SetListView (CurrentVote);
			ScrollToEnd (true);
		}

		StackLayout GetCommentReplies ()
		{
			var stack = new StackLayout { VerticalOptions = LayoutOptions.FillAndExpand };
			stack.Children.Add (new LabelWide ("Replies"));
			CommentList comments = null;
			if (Persist.Instance.CommentCache.ContainsKey (CurrentVote.voteId))
				comments = Persist.Instance.CommentCache [CurrentVote.voteId];
			if (comments != null)
				comments.OrderBy (c => c.When).ToList ().ForEach (co => {
					if (!string.IsNullOrEmpty (co.Comment)) {
						var cell = new CommentReplyGrid (co);
						stack.Children.Add (cell);
					}
				});
			return stack;
		}

		private Task ScrollToEnd (bool animate = false)
		{
			return repliesScrollView.ScrollToAsync (innerRepliesStack, ScrollToPosition.End, animate);
		}

		void SetListView (Vote vote)
		{
			innerRepliesStack = GetCommentReplies ();
			repliesScrollView = new ScrollView { Content = innerRepliesStack };
			Content = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new CommentViewGrid (vote, onReply: null, showReplyBtn: false, showUntried: vote.untried),
					repliesScrollView,
					ReplyBtn
				}
			};
		}



		public CommentReplyPage (Vote vote)
		{
			Padding = 5;
			CurrentVote = vote;
			ReplyBtn = new ColouredButton ("Reply"){ BackgroundColor = settings.BaseDarkColor };
			ReplyBtn.Clicked += DoStartReply;
			SaveBtn = new DoubleButton {
				LeftText = "Save",
				RightText = "Cancel",
				LeftClick = DoSave,
				RightClick = DoCancelSave,
				ButtonBackgroundColor = settings.BaseDarkColor
			};
			SetListView (vote);
			Appearing += (sender, e) => {
				ScrollToEnd (true);
			};
		}
	}
}


