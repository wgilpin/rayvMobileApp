using System;

using Xamarin.Forms;
using System.Diagnostics;

namespace RayvMobileApp
{
	class VoteLabel : LabelClickable
	{
		public VoteLabel (bool selected)
		{			
			XAlign = TextAlignment.Start;
			FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label));
			FontAttributes = FontAttributes.Bold;
			YAlign = TextAlignment.Center;
			TextColor = selected ? Color.White : Color.Black;
			BackgroundColor = selected ? settings.BaseColor : Color.White;
		}
	}

	public class VoteSavedEventArgs : EventArgs
	{
		public VoteValue Vote;

		public VoteSavedEventArgs (VoteValue vote)
		{
			Vote = vote;
		}
	}

	public class EditVotePage : ContentPage
	{
		VoteValue _vote;
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
						Saved (this, new VoteSavedEventArgs (_vote));
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

		public void VoteLiked (object s, EventArgs e)
		{
			if (Spinner.IsRunning) {
				Debug.WriteLine ("Spinner running - abort");
				return;
			}
			_vote = VoteValue.Liked; 
			OnSaved ();
		}

		public void VoteDisliked (object s, EventArgs e)
		{
			if (Spinner.IsRunning) {
				Debug.WriteLine ("Spinner running - abort");
				return;
			}
			_vote = VoteValue.Disliked; 
			OnSaved ();
		}

		public void VoteUntried (object s, EventArgs e)
		{
			if (Spinner.IsRunning) {
				Debug.WriteLine ("Spinner running - abort");
				return;
			}
			_vote = VoteValue.Untried; 
			OnSaved ();
		}

		public void VoteNone (object s, EventArgs e)
		{
			if (Spinner.IsRunning) {
				Debug.WriteLine ("Spinner running - abort");
				return;
			}
			_vote = VoteValue.None; 
			OnSaved ();
		}

		public EditVotePage (VoteValue vote, bool inFlow = true)
		{
			InFlow = inFlow;
			_vote = vote;
			var grid = new Grid { 
				RowSpacing = 20,
				ColumnSpacing = 30,
				VerticalOptions = LayoutOptions.Start,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (40) },
					new RowDefinition (),
					new RowDefinition (),
					new RowDefinition (),
					new RowDefinition (),
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (10) },
					new ColumnDefinition { Width = new GridLength (30) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (15) },
					new ColumnDefinition { Width = new GridLength (5) },
				}
			};
			var selectionFrame = new Frame { BackgroundColor = settings.BaseColor, HasShadow = false, };
			if (vote == VoteValue.Liked) {
				grid.Children.Add (selectionFrame, 0, 5, 1, 2); 
			}
			grid.Children.Add (new ImageButton ("Like.png", VoteLiked), 1, 2, 1, 2);
			grid.Children.Add (new VoteLabel (vote == VoteValue.Liked) { 
				Text = "Like", 
				OnClick = VoteLiked, 
			}, 2, 3, 1, 2);
			grid.Children.Add (new ImageButton (
				settings.DevicifyFilename ("arrow.png"), VoteLiked), 3, 4, 1, 2);
			if (vote == VoteValue.Disliked) {
				grid.Children.Add (selectionFrame, 0, 5, 2, 3); 
			}
			grid.Children.Add (new ImageButton ("Dislike.png", VoteDisliked), 1, 2, 2, 3);
			grid.Children.Add (new VoteLabel (vote == VoteValue.Disliked) {
				Text = "Dislike", 
				OnClick = VoteDisliked, 
			}, 2, 3, 2, 3);
			grid.Children.Add (new ImageButton (
				settings.DevicifyFilename ("arrow.png"), VoteDisliked), 3, 4, 2, 3);
			if (vote == VoteValue.Untried) {
				grid.Children.Add (selectionFrame, 0, 5, 3, 4); 
			}

			grid.Children.Add (new ImageButton ("Wish1.png", VoteUntried), 1, 2, 3, 4);
			grid.Children.Add (new VoteLabel (vote == VoteValue.Untried) { 
				Text = "Wish", 
				OnClick = VoteUntried,
			}, 2, 3, 3, 4);
			grid.Children.Add (new ImageButton (
				settings.DevicifyFilename ("arrow.png"), VoteUntried), 3, 4, 3, 4);

			if (vote != VoteValue.None) {
				grid.Children.Add (new ImageButton ("remove_vote.png", VoteNone), 1, 2, 4, 5);
				grid.Children.Add (new VoteLabel (selected: false) { 
					Text = "Remove", 
					OnClick = VoteNone,
				}, 2, 3, 4, 5);
				grid.Children.Add (new ImageButton (
					settings.DevicifyFilename ("arrow.png"), VoteUntried), 3, 4, 4, 5);
			}
			Spinner = new ActivityIndicator{ Color = Color.Red, IsRunning = false };
			grid.Children.Add (Spinner, 0, 5, 0, 1);
			Content = grid;
			if (vote != VoteValue.None) {
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


