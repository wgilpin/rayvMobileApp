using System;

using Xamarin.Forms;

namespace RayvMobileApp
{
	public class CommentSavedEventArgs : EventArgs
	{
		public string Comment;

		public CommentSavedEventArgs (string comment)
		{
			Comment = comment;
		}
	}


	public class EditCommentPage : ContentPage
	{
		public event EventHandler<CommentSavedEventArgs> Saved;
		public event EventHandler Cancelled;

		Editor TextEditor;
		bool InFlow;
		bool IsMandatory;
		ActivityIndicator Spinner;

		protected virtual void DoSave (object sender, EventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				Spinner.IsRunning = true;
			});
			if ((!IsMandatory) || (TextEditor.Text?.Length > 0)) {
				if (Saved != null)
					Saved (this, new CommentSavedEventArgs (TextEditor.Text));
			} else {
				Device.BeginInvokeOnMainThread (() => {
					DisplayAlert ("No Comment", "Please add a comment", "OK");
					Spinner.IsRunning = false;
				});
			}
		}

		protected virtual void DoCancel (object sender, EventArgs e)
		{
			if (Cancelled != null)
				Cancelled (sender, e);
		}

		public EditCommentPage (string initialText, VoteValue vote, bool inFlow = true)
		{
			InFlow = inFlow;
			IsMandatory = (vote == VoteValue.Liked) || (vote == VoteValue.Disliked);
			this.Title = "Comment";
			TextEditor = new Editor { 
//				HorizontalOptions = LayoutOptions.Fil, 
				Text = initialText,
			};
			TextEditor.Keyboard = Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions);
			Spinner = new ActivityIndicator{ Color = Color.Red, };
			Content = new StackLayout { 
				Padding = 2,
				Children = {
					new Frame { OutlineColor = Color.Gray,
						HasShadow = false,
						Content = TextEditor,
					},
					Spinner,
					new LabelWithImageButton { 
						Text = "Done", 
						Source = settings.DevicifyFilename ("arrow.png"),
						BackgroundColor = ColorUtil.Darker (settings.BaseColor),
						TextColor = Color.White,
						OnClick = DoSave,
						XAlign = TextAlignment.Center,
					},
				}
			};
			if (!string.IsNullOrEmpty (initialText)) {
				ToolbarItems.Add (new ToolbarItem {
					Text = inFlow ? " Next" : "  Cancel  ",
					//				Icon = "187-pencil@2x.png",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						if (InFlow)
							DoSave (null, null);
						else
							DoCancel (null, null);
					})
				});
			}
		}
	}
}


