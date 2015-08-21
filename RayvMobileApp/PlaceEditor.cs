using System;
using Xamarin.Forms;
using System.Diagnostics;
using Xamarin;

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
			EditPlace.vote.vote = ev.Vote;
			var kindPage = new EditPlaceKindPage (EditPlace.vote.kind, EditPlace.vote.style);
			kindPage.Saved += DoKindSaved;
			Debug.WriteLine ("PlaceEditor DoCuisineSaved");
			CallingPage.Navigation.PushAsync (kindPage);
		}

		void DoCuisineSaved (object sender, CuisineSavedEventArgs ev)
		{
			EditPlace.vote.cuisine = ev.Cuisine;
			var commentPage = new EditCommentPage (
				                  EditPlace.Comment (), 
				                  vote: EditPlace.vote.vote);
			commentPage.Saved += DoCommentSaved;
			Debug.WriteLine ("PlaceEditor DoKindSaved");
			CallingPage.Navigation.PushAsync (commentPage);
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
					var commentPage = new EditCommentPage (
						                  EditPlace.Comment (), 
						                  vote: EditPlace.vote.vote);
					commentPage.Saved += DoCommentSaved;
					Debug.WriteLine ("PlaceEditor DoKindSaved");
					CallingPage.Navigation.PushAsync (commentPage);
				} else {
					// not a bar
					Debug.WriteLine ("PlaceEditor DoKindSaved2");
					var cuisinePage = new EditCuisinePage (EditPlace.vote.cuisineName);
					cuisinePage.Saved += DoCuisineSaved;
					CallingPage.Navigation.PushAsync (cuisinePage);
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				CallingPage.Navigation.PopToRootAsync ();
			}
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
			VotePage = new EditVotePage (EditPlace.vote.vote);
			VotePage.Saved += DoVoteSaved;
		}
	}
}

