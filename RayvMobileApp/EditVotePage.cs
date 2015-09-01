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

	public class VoteSavedEventArgs : EventArgs
	{
		public int Vote;
		public bool Untried;

		public VoteSavedEventArgs (int vote, bool untried)
		{
			Vote = vote;
			Untried = untried;
		}
	}

	public class EditVotePage : ContentPage
	{
		int _vote;
		bool _untried;
		ActivityIndicator Spinner;
		bool InFlow;

		public event EventHandler<VoteSavedEventArgs> Saved;
		public event EventHandler Cancelled;

		protected virtual void OnSaved ()
		{
			if (Saved != null) {
				Debug.WriteLine ("Vote OnSaved Spinner On");
				Spinner.IsRunning = true;
				new System.Threading.Thread (new System.Threading.ThreadStart (() => {
					Device.BeginInvokeOnMainThread (() => {
						Debug.WriteLine ("EditVotePage OnSaved");
						Saved (this, new VoteSavedEventArgs (_vote, _untried));
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
			_untried = true;
			OnSaved ();
		}

		public EditVotePage (int vote, bool untried, bool inFlow = true)
		{
			InFlow = inFlow;
			_vote = vote;
			_untried = untried;
			BackgroundColor = Color.White;
			Padding = 5;
			var stack = new StackLayout { Spacing = 20 };
			var stars = new StarEditor (false) { Vote = vote, HorizontalOptions = LayoutOptions.CenterAndExpand };
			stack.Children.Add (stars);
			var untriedVoteBtn = new RayvButton ("I want to try this place") { 
				OnClick = VoteUntried,
			};
			stack.Children.Add (untriedVoteBtn);
			if (vote != Vote.VoteNotSetValue) {
				var removeBtn = new ButtonWithImage () {
					Text = "Remove my vote",
					ImageSource = "remove_vote.png",
					FontSize = settings.FontSizeButtonLarge,
					OnClick = VoteNone,
					Padding = 10,
				};
				// vote is set, add a remove option
				stack.Children.Add (removeBtn);
			}
			Spinner = new ActivityIndicator{ Color = Color.Red, IsRunning = false };
			stack.Children.Add (Spinner);
			Content = stack;
			if (vote != Vote.VoteNotSetValue) {
				// vote is set, so can navigate
				ToolbarItems.Add (new ToolbarItem {
					Text = inFlow ? " Next" : "  Cancel  ",
					//				Icon = "187-pencil@2x.png",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						if (InFlow)
							OnSaved ();
						else
							OnCancelled ();
					})
				});
			}
		}
	}
}


