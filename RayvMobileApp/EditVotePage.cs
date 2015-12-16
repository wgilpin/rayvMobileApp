using System;

using Xamarin.Forms;
using System.Diagnostics;

namespace RayvMobileApp
{
	class VoteLabel : LabelClickable
	{
		public VoteLabel (bool selected)
		{			
			Label.XAlign = TextAlignment.Start;
			Label.FontSize = settings.FontSizeLabelLarge;
			Label.FontAttributes = FontAttributes.Bold;
			Label.YAlign = TextAlignment.Center;
			Label.TextColor = selected ? Color.White : Color.Black;
			BackgroundColor = selected ? settings.BaseColor : Color.White;
		}
	}

	public class EventArgsVoteValues : EventArgs
	{
		public int Vote;
		public bool Untried;

		public EventArgsVoteValues (int vote, bool untried)
		{
			Vote = vote;
			Untried = untried;
		}
	}

	public class EditVoteView : StackLayout
	{
		int _vote;
		bool _untried;
		ActivityIndicator Spinner;
		bool InFlow;

		public event EventHandler<EventArgsVoteValues> Saved;
		public event EventHandler<EventArgsMessage> ShowMessage;
		public event EventHandler Cancelled;
		public event EventHandler Removed;

		protected virtual void OnSaved ()
		{
			if (Saved != null) {
				Debug.WriteLine ("Vote OnSaved Spinner On");
				Spinner.IsRunning = true;
				new System.Threading.Thread (new System.Threading.ThreadStart (() => {
					Device.BeginInvokeOnMainThread (() => {
						if (_vote == 0 && _untried == false)
							ShowMessage?.Invoke (this, new EventArgsMessage ("You must vote"));
						else {
							Debug.WriteLine ("EditVotePage OnSaved");
							Saved (this, new EventArgsVoteValues (_vote, _untried));
						}
						Spinner.IsRunning = false;
					});
				})).Start ();
			}
		}

		protected virtual void OnCancelled ()
		{
			if (Cancelled != null)
				Cancelled (this.Cancelled, null);
		}

		public void SetStar (int value)
		{
			if (Spinner.IsRunning) {
				Debug.WriteLine ("Spinner running - abort");
				return;
			}
			_vote = value; 
			_untried = false;
			OnSaved ();
		}

		public void VoteUntried (object s, EventArgs e)
		{
			if (Spinner.IsRunning) {
				Debug.WriteLine ("Spinner running - abort");
				return;
			}
			_vote = 0;
			_untried = true;
			OnSaved ();
		}

		public void VoteNone (object s, EventArgs e)
		{
			if (Spinner.IsRunning) {
				Debug.WriteLine ("Spinner running - abort");
				return;
			}
			_vote = 0; 
			_untried = false;
			Removed?.Invoke (this, null);
		}

		public EditVoteView (int vote, bool untried, bool inFlow = true)
		{
			InFlow = inFlow;
			_vote = vote;
			_untried = untried;
			BackgroundColor = Color.White;
			var innerStack = new StackLayout {
				Padding = 5,
				Spacing = 20,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			innerStack.Children.Add (new Label {
				HorizontalOptions = LayoutOptions.Center,
				FontSize = settings.FontSizeLabelLarge,
				Text = "Set Vote",
				FontAttributes = FontAttributes.Bold,
			});
			var stars = new StarEditor (false) { Vote = vote, HorizontalOptions = LayoutOptions.CenterAndExpand };
			stars.ChangedNotUI += (o, e) => {
				Device.BeginInvokeOnMainThread (() => {
					SetStar ((e as StarEditorEventArgs).Vote);
				});
			};
			innerStack.Children.Add (stars);
			innerStack.Children.Add (new Label{ Text = "or", XAlign = TextAlignment.Center });
			var untriedVoteBtn = new ButtonWithImage {
				BackgroundColor = settings.ColorDarkGray,
				Text = "Want to try",
				FontSize = settings.FontSizeButtonLarge,
				ImageSource = "Wish_white.png", 
				OnClick = VoteUntried,
				Padding = 10
			};
			untriedVoteBtn.ButtonLeft.HorizontalOptions = LayoutOptions.Center;
			innerStack.Children.Add (untriedVoteBtn);
			if (vote != Vote.VoteNotSetValue || untried) {
				// if there's a vote or untried is set, show the remove vote button
				var removeBtn = new ButtonWithImage () {
					Text = "Remove my vote",
					ImageSource = "Delete_white.png",
					FontSize = settings.FontSizeButtonLarge,
					OnClick = VoteNone,
					Padding = 10,
				};
				removeBtn.ButtonLeft.HorizontalOptions = LayoutOptions.Center; 
				// vote is set, add a remove option
				innerStack.Children.Add (removeBtn);
			}
			Spinner = new ActivityIndicator{ Color = Color.Red, IsRunning = false };
			innerStack.Children.Add (Spinner);

			var buttons = new DoubleImageButton { 
				LeftText = "Back", 
				RightText = "Next",
				LeftSource = "back_1.png",
				RightSource = "forward_1.png"
			};
			buttons.LeftClick = (s, e) => OnCancelled ();
			buttons.RightClick = (s, e) => OnSaved ();
			Children.Add (innerStack);
			Children.Add (buttons);
//			if (vote != Vote.VoteNotSetValue) {
//				// vote is set, so can navigate
//				ToolbarItems.Add (new ToolbarItem {
//					Text = inFlow ? " Next" : "  Cancel  ",
//					//				Icon = "187-pencil@2x.png",
//					Order = ToolbarItemOrder.Primary,
//					Command = new Command (() => { 
//						if (InFlow)
//							OnSaved ();
//						else
//							OnCancelled ();
//					})
//				});
//			}
		}
	}
}


