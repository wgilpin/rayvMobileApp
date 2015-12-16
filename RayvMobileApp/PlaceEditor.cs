using System;
using Xamarin.Forms;
using System.Diagnostics;
using Xamarin;
using System.Linq;

namespace RayvMobileApp
{

	public class PlaceEditor: ContentPage
	{
		Place EditPlace;
		bool EditAsDraft;
		EditVoteView VoteView;
		EditPlaceKindView KindView;
		EditCommentView CommentView;


		public event EventHandler Saved;
		public event EventHandler Cancelled;
		public event EventHandler Removed;

		#region Methods

		void ShowVoteView ()
		{
			VoteView = new EditVoteView (EditPlace.vote.vote, EditPlace.vote.untried);
			VoteView.Saved += DoVoteSaved;
			VoteView.Cancelled += (sender, e) => {
				Cancelled.Invoke (sender, null);
			};
			VoteView.Removed += DoVoteRemoved;
			VoteView.ShowMessage += (sender, e) => {
				DisplayAlert ("Error", e.Message, "OK");
			};
			Content = VoteView;
		}

		void ShowKindView ()
		{
			KindView = new EditPlaceKindView (EditPlace.vote.kind, EditPlace.vote.style);
			KindView.Saved += DoKindSaved;
			KindView.Cancelled += (s, e) => {
				ShowVoteView ();
			};
			KindView.ShowMessage += (object s, EventArgsMessage e) => {
				DisplayAlert ("Meal Kind", e.Message, "OK");
			};
			Content = KindView;
		}

		void ShowCommentView ()
		{
			CommentView = new EditCommentView (EditPlace.Comment (), vote: EditPlace.vote.vote, inFlow: true);
			CommentView.Saved += DoCommentSaved;
			CommentView.Cancelled += (s, e) => {
				ShowCuisineView (); 
			};
			CommentView.NoComment += (s, e) => {
				DisplayAlert ("No Comment", "You have to comment", "OK");
			};
			Content = CommentView;
		}



		void ShowCuisineView ()
		{
			var CuisineView = new EditCuisineView (EditPlace.vote.cuisineName);
			CuisineView.Saved += DoCuisineSaved;
			CuisineView.Cancelled += (sender, e) => {
				ShowKindView ();
			};
			CuisineView.NoCuisine += (s, e) => {
				DisplayAlert ("No Cuisine", "You have to choose a cusine", "OK");
			};
			Content = CuisineView;
		}

		#endregion

		#region events

		void DoVoteSaved (object sender, EventArgsVoteValues ev)
		{
			EditPlace.vote.vote = ev.Vote;
			EditPlace.vote.untried = ev.Untried;
			ShowKindView ();
			Debug.WriteLine ("PlaceEditor DoVoteSaved");
		}

		void DoCuisineSaved (object sender, CuisineSavedEventArgs ev)
		{
			EditPlace.vote.cuisine = ev.Cuisine;
			ShowCommentView ();
			Debug.WriteLine ("PlaceEditor DoKindSaved");
		}

		void DoKindSaved (object sender, KindSavedEventArgs ev)
		{
			try {
				EditPlace.vote.kind = ev.Kind;
				EditPlace.vote.style = ev.Style;
				Debug.Assert (ev.Style != PlaceStyle.None);
				Debug.Assert (ev.Kind != MealKind.None);
				if (ev.Kind == MealKind.Bar) {
					// #674 default cuisine for Bar is Bar
					EditPlace.vote.cuisine = new Cuisine{ Title = "Bar" };
					// #681 skip cuisine selection if its a bar
					ShowCommentView ();
					Debug.WriteLine ("PlaceEditor DoKindSaved");
				} else {
					// not a bar
					Debug.WriteLine ("PlaceEditor DoKindSaved2");
					ShowCuisineView ();
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				Navigation.PopToRootAsync ();
			}
		}

		void DoCommentSaved (object sender, CommentSavedEventArgs ev)
		{
			EditPlace.setComment (ev.Comment); 
			EditPlace.IsDraft = false;
			Saved?.Invoke (sender, null);
			Debug.WriteLine ("PlaceEditor DoCommentSaved");
		}

		void DoVoteRemoved (object sender, EventArgs e)
		{
			Persist.Instance.GetWebConnection ().post ($"/item/del/{EditPlace.key}");
			// remove the vote from the votes list
			Persist.Instance.Votes.Remove (EditPlace.vote);
			//TODO: delete & free an object?
			// was that the only vote? If so, remove the place
			var votesLeft = Persist.Instance.Votes.Where (v => v.key == EditPlace.key).Count ();
			if (votesLeft == 0) {
				// no votes left, remove from place list
				Persist.Instance.DeletePlace (EditPlace);
			} else {
				// update the place to remove the vote details
				EditPlace.vote = null;
				Persist.Instance.UpdateVoteForPlace (EditPlace);
			}
			Removed?.Invoke (this, null);
		}

		#endregion


		public PlaceEditor (Place place, bool isDraft = false)
		{
			Console.WriteLine ("PlaceEditor ctor");
			Padding = Device.OnPlatform (new Thickness (2, 20, 2, 2), 2, 2);
			EditPlace = place;
			ShowVoteView ();
//			};
		}
	}
}

