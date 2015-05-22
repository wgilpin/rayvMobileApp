using System;
using Xamarin.Forms;

namespace RayvMobileApp
{

	public class PlaceEditor
	{
		Place EditPlace;
		ContentPage CallingPage;
		bool EditAsDraft;
		EditVotePage VotePage;

		void DoVoteSaved (object sender, VoteSavedEventArgs ev)
		{
			EditPlace.voteValue = ev.Vote;
			var cuisinePage = new EditCuisinePage (EditPlace.vote.cuisineName);
			cuisinePage.Saved += DoCuisineSaved;
			CallingPage.Navigation.PushAsync (cuisinePage);
		}

		void DoCuisineSaved (object sender, CuisineSavedEventArgs ev)
		{
			EditPlace.vote.cuisine = ev.Cuisine;
			var kindPage = new EditPlaceKindPage (EditPlace.vote.kind, EditPlace.vote.style);
			kindPage.Saved += DoKindSaved;
			CallingPage.Navigation.PushAsync (kindPage);
		}

		void DoCommentSaved (object sender, CommentSavedEventArgs ev)
		{
			EditPlace.setComment (ev.Comment); 
			CallingPage.Navigation.PushModalAsync (new DetailPage (
				EditPlace, showToolbar: true, showSave: true, isDraft: EditAsDraft));
		}

		void DoKindSaved (object sender, KindSavedEventArgs ev)
		{
			EditPlace.vote.kind = ev.Kind;
			EditPlace.vote.style = ev.Style;
			var commentPage = new EditCommentPage (EditPlace.Comment ());
			commentPage.Saved += DoCommentSaved;
			CallingPage.Navigation.PushAsync (commentPage);
		}

		public void Edit ()
		{
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

