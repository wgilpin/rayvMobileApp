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

		Editor TextEditor;

		protected virtual void DoSave (object sender, EventArgs e)
		{
			if (TextEditor.Text?.Length > 0) {
				if (Saved != null)
					Saved (this, new CommentSavedEventArgs (TextEditor.Text));
			} else {
				DisplayAlert ("No Comment", "Please add a comment", "OK");
				return;
			}
		}

		public EditCommentPage (string initialText)
		{
			this.Title = "Comment";
			TextEditor = new Editor { 
//				HorizontalOptions = LayoutOptions.Fil, 
				Text = initialText,
			};
			TextEditor.Keyboard = Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions);

			Content = new StackLayout { 
				Padding = 2,
				Children = {
					new Frame { OutlineColor = Color.Gray,
						HasShadow = false,
						Content = TextEditor,
					},
					new LabelWithImageButton { 
						Text = "Done", 
						Source = settings.DevicifyFilename ("arrow.png"),
						BackgroundColor = ColorUtil.Darker (settings.BaseColor),
						TextColor = Color.White,
						OnClick = DoSave,
					},
				}
			};
			if (!string.IsNullOrEmpty (initialText)) {
				ToolbarItems.Add (new ToolbarItem {
					Text = " Next",
					//				Icon = "187-pencil@2x.png",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						DoSave (null, null);
					})
				});
			}
		}
	}
}


