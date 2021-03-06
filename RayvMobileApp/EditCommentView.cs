﻿using System;

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


	public class EditCommentView : StackLayout
	{
		public event EventHandler<CommentSavedEventArgs> Saved;
		public event EventHandler NoComment;
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
				Saved?.Invoke (this, new CommentSavedEventArgs (TextEditor.Text));
			} else {
				Device.BeginInvokeOnMainThread (() => {
					NoComment?.Invoke (this, null);
					Spinner.IsRunning = false;
				});
			}
		}

		protected virtual void DoCancel (object sender, EventArgs e)
		{
			if (Cancelled != null)
				Cancelled (sender, e);
		}

		public EditCommentView (string initialText, int vote, bool inFlow = true)
		{
			InFlow = inFlow;
			IsMandatory = vote > 0;
			TextEditor = new Editor { 
//				HorizontalOptions = LayoutOptions.Fil, 
				Text = initialText,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Editor)),
			};
			TextEditor.Keyboard = Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions);
			Spinner = new ActivityIndicator{ Color = Color.Red, };
			var innerStack = new StackLayout {
				Padding = 2,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			innerStack.Children.Add (new Label {
				HorizontalOptions = LayoutOptions.Center,
				FontSize = settings.FontSizeLabelLarge,
				Text = "Comment",
				FontAttributes = FontAttributes.Bold,
			});
			innerStack.Children.Add (new Frame { OutlineColor = Color.Gray,
				HasShadow = false,
				Content = TextEditor,
				VerticalOptions = LayoutOptions.FillAndExpand,
			});
			innerStack.Children.Add (Spinner);					
			Children.Add (innerStack);
			if (inFlow) {
				var buttons = new DoubleImageButton { 
					LeftText = "Back", 
					RightText = "Save",
					LeftSource = "back_1.png",
					RightSource = "forward_1.png"
				};
				buttons.LeftClick = (s, e) => Cancelled?.Invoke (this, null);
				buttons.RightClick = (s, e) => {
					if (InFlow)
						DoSave (null, null);
					else
						Cancelled?.Invoke (this, null);
				};
				Children.Add (buttons);
			} else {
				// have a Done button if not in a flow (ie not showing the 2-button)
				Children.Add (new LabelWithImageButton { 
					Text = "Done", 
					Source = settings.DevicifyFilename ("arrow.png"),
					BackgroundColor = ColorUtil.Darker (settings.BaseColor),
					TextColor = Color.White,
					OnClick = DoSave,
					XAlign = TextAlignment.Center,
				});
			}
//			if (!string.IsNullOrEmpty (initialText)) {
//				ToolbarItems.Add (new ToolbarItem {
//					Text = inFlow ? " Next" : "  Cancel  ",
//					//				Icon = "187-pencil@2x.png",
//					Order = ToolbarItemOrder.Primary,
//					Command = new Command (() => { 
//						if (InFlow)
//							DoSave (null, null);
//						else
//							DoCancel (null, null);
//					})
//				});
//			}
		}
	}
}


