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
			if (TextEditor.Text?.Length == 0)
				DisplayAlert ("No Comment", "Please add a comment", "OK");
			if (Saved != null)
				Saved (this, new CommentSavedEventArgs (TextEditor.Text));
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
						Text = "Next", 
						Source = settings.DevicifyFilename ("Add Select right button.png"),
						BackgroundColor = ColorUtil.Darker (settings.BaseColor),
						TextColor = Color.White,
						OnClick = DoSave
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


