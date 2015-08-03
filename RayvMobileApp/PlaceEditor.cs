using System;
using Xamarin.Forms;
using System.Diagnostics;

namespace RayvMobileApp
{

	public class PlaceEditor
	{
		Place EditPlace;
		ContentPage CallingPage;
		bool EditAsDraft;
		EditVotePage VotePage;

		public event EventHandler Saved;

		void DoVoteSaved (object sender, VoteSavedEventArgs ev)
		{
			EditPlace.voteValue = ev.Vote;
			var cuisinePage = new EditCuisinePage (EditPlace.vote.cuisineName);
			cuisinePage.Saved += DoCuisineSaved;
			Debug.WriteLine ("PlaceEditor DoVoteSaved");
			CallingPage.Navigation.PushAsync (cuisinePage);
		}

		void DoCuisineSaved (object sender, CuisineSavedEventArgs ev)
		{
			EditPlace.vote.cuisine = ev.Cuisine;
			var kindPage = new EditPlaceKindPage (EditPlace.vote.kind, EditPlace.vote.style);
			kindPage.Saved += DoKindSaved;
			Debug.WriteLine ("PlaceEditor DoCuisineSaved");
			CallingPage.Navigation.PushAsync (kindPage);
		}

		void DoKindSaved (object sender, KindSavedEventArgs ev)
		{
			EditPlace.vote.kind = ev.Kind;
			EditPlace.vote.style = ev.Style;
			if (ev.Kind == MealKind.Bar)
				// #674 default cuisine for Bar is Bar
				EditPlace.vote.cuisine = new Cuisine{ Title = "Bar" };
			Debug.Assert (ev.Style != PlaceStyle.None);
			Debug.Assert (ev.Kind != MealKind.None);
			var commentPage = new EditCommentPage (
				                  EditPlace.Comment (), 
				                  vote: EditPlace.vote.vote);
			commentPage.Saved += DoCommentSaved;
			Debug.WriteLine ("PlaceEditor DoKindSaved");
			CallingPage.Navigation.PushAsync (commentPage);
		}

		void DoCommentSaved (object sender, CommentSavedEventArgs ev)
		{
			EditPlace.setComment (ev.Comment); 
			var detailPage = new DetailPage (
				                 EditPlace, showToolbar: false, showSave: true, isDraft: EditAsDraft);
			if (Saved != null)
				detailPage.Closed += Saved;
			Debug.WriteLine ("PlaceEditor DoCommentSaved");
			CallingPage.Navigation.PushModalAsync (new RayvNav (detailPage));
		}

		public void Edit ()
		{
			Debug.WriteLine ("EditVotePage Edit");
			CallingPage.Navigation.PushAsync (VotePage);
		}

		public PlaceEditor (Place place, ContentPage caller, bool isDraft = false)
		{
			EditPlace = place;
			CallingPage = caller;
			VotePage = new EditVotePage (EditPlace.voteValue);
			VotePage.Saved += DoVoteSaved;
		}
	}
}

