using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin;
using System.Linq;

namespace RayvMobileApp
{
	public class CommentReplyPage : ContentPage
	{
		RayvButton ReplyBtn;
		RayvButton SaveBtn;
		Vote CurrentVote;
		Editor textEditor;

		void DoSave (Object sender, EventArgs ev)
		{
			try {
				var parms = new Dictionary<string,string> {
					{ "author",Persist.Instance.MyId.ToString () },
					{ "comment",textEditor.Text },
					{ "vote",CurrentVote.voteId.ToString () },
					{ "when",DateTime.UtcNow.ToString ("s") }
				};
				var res = Persist.Instance.GetWebConnection ().post ("/api/comment", parms);
				if (res == "OK") {
					Navigation.PopAsync ();
				} else {
					DisplayAlert ("Error", "Save failed", "OK");
				}
			} catch (Exception ex) {
				Console.WriteLine ($"CommentReplyPage.DoSave ERROR {ex}");
				Insights.Report (ex);
			}
		}

		void DoReply (Object sender, EventArgs ev)
		{
			// set edit view	
			textEditor = new Editor { 
				VerticalOptions = LayoutOptions.FillAndExpand,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Editor))
			};
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

		StackLayout GetCommentReplies ()
		{
			var stack = new StackLayout { VerticalOptions = LayoutOptions.FillAndExpand };
			stack.Children.Add (new LabelWide ("Replies"));
			var comments = Persist.Instance.GetComments (CurrentVote);
			if (comments != null)
				comments.OrderBy (c => c.When).ToList ().ForEach (co => {
					if (!string.IsNullOrEmpty (co.Comment)) {
						var cell = new CommentReplyGrid (co);
						stack.Children.Add (cell);
					}
				});
			return stack;
		}

		void SetListView (Vote vote)
		{
			StackLayout inner = GetCommentReplies ();
			Content = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new CommentViewGrid (vote, onReply: null, showReplyBtn: false),
					new ScrollView { Content = inner },
					ReplyBtn
				}
			};
		}

		public CommentReplyPage (Vote vote)
		{
			Padding = 5;
			CurrentVote = vote;
			ReplyBtn = new RayvButton ("Reply"){ BackgroundColor = settings.BaseDarkColor };
			ReplyBtn.Clicked += DoReply;
			SaveBtn = new RayvButton ("Save"){ BackgroundColor = settings.BaseDarkColor };
			SaveBtn.Clicked += DoSave;
			SetListView (vote);
		}
	}
}


