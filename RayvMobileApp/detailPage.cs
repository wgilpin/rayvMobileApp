﻿using System;
using Xamarin.Forms;

//using Foundation;
//using UIKit;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Xamarin;
using System.Collections.Generic;
using System.Linq;

//using Xamarin.Forms.Labs;

namespace RayvMobileApp
{
	
	class DetailLabel : Label
	{
		public DetailLabel (string text) : base ()
		{
			VerticalOptions = LayoutOptions.CenterAndExpand;
			HorizontalOptions = LayoutOptions.CenterAndExpand;
			Text = text;
		}
	}

	public class DetailPage : ContentPage
	{
		// these are consts as they are used in the switch in SetVote
		const string LIKE_TEXT = "Like";
		const string DISLIKE_TEXT = "Dislike";
		const string WISH_TEXT = "Wishlist";

		public event EventHandler Closed;

		protected virtual void OnClose (EventArgs e)
		{
			if (Closed != null)
				Closed (this, e);
		}

		#region Fields

		Place DisplayPlace;
		Label Place_name;
		Image Img;
		Label Category;
		ButtonWide VoteLike;
		ButtonWide VoteDislike;
		ButtonWide VoteWishlist;
		ButtonWide CallBtn;
		ButtonWide WebBtn;
		Label distance;
		LabelWithImageButton Address;
		LabelWithImageButton Comment;
		EntryWithButton CommentEditor;
		private bool ShowToolbar;
		public bool Dirty;

		#endregion

		#region Logic

		Grid GetFriendsComments ()
		{
			Grid grid = new Grid {
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (31) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (70) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			try {
				string MyStringId = Persist.Instance.MyId.ToString ();
				List<Vote> voteList = (from v in Persist.Instance.Votes
				                       where v.key == DisplayPlace.key
				                           && v.voter != MyStringId
				                           && v.VoterName.Length > 0
				                       select v).OrderBy (x => x.comment).ToList ();

				int whichRow = 0;
				//TODO: This should be a listview binding
				for (int row = 0; row < voteList.Count (); row++) {
					Vote vote = voteList [row];
					if (vote.voter != MyStringId) {
						try {

							grid.RowDefinitions.Add (new RowDefinition (){ Height = GridLength.Auto });
							grid.RowDefinitions.Add (new RowDefinition (){ Height = GridLength.Auto });
							string FriendName = Persist.Instance.Friends [voteList [whichRow].voter].Name;
							Button LetterBtn = new Button {
								WidthRequest = 30,
								HeightRequest = 30,
								FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button)),
								BorderRadius = 15,
								BackgroundColor = Color.Red,
								Text = "X",
								TextColor = Color.White,
								VerticalOptions = LayoutOptions.Start,
							};
							LetterBtn.Text = vote.FirstLetter;
							LetterBtn.BackgroundColor = vote.RandomColor;
							grid.Children.Add (LetterBtn, 0, 1, whichRow * 2, whichRow * 2 + 1);
							grid.Children.Add (new Label { Text = FriendName }, 1, 2, whichRow * 2, whichRow * 2 + 1);
							grid.Children.Add (new Label { 
								Text = vote.PrettyHowLongAgo,
								FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
								FontAttributes = FontAttributes.Italic,
								TextColor = Color.FromHex ("#606060"),
							}, 3, 5, whichRow * 2, whichRow * 2 + 1);
							String comment_text = vote.PrettyComment;
							if (!String.IsNullOrEmpty (comment_text)) {
								grid.Children.Add (new Label { 
									Text = comment_text,
									FontAttributes = FontAttributes.Italic,
								}, 0, 5, whichRow * 2 + 1, whichRow * 2 + 2);
							}
							Label voteLbl = new Label {
								Text = vote.GetVoteAsString,
							};
							grid.Children.Add (voteLbl, 2, 3, whichRow * 2, whichRow * 2 + 1);
							whichRow++;
						} catch (Exception ex) {
							Console.WriteLine ("detailPage.GetFriendsComments {0}", ex);
							Insights.Report (ex);
						}
					}
				}
			} catch (Exception ex) {
				Insights.Report (ex);
			}
			return grid;
		}



		async void SetVote (object sender, EventArgs e)
		{
			SetVoteButton (sender as ButtonWide);
			// should NOT reference UILabel on background thread!
			string previousVote = DisplayPlace.vote;
			switch ((sender as ButtonWide).Text) {
			case LIKE_TEXT:
				DisplayPlace.vote = "1";
				DisplayPlace.untried = false;
				break;
			case DISLIKE_TEXT:
				DisplayPlace.vote = "-1";
				DisplayPlace.untried = false;
				break;
			case WISH_TEXT:
				DisplayPlace.vote = "0";
				DisplayPlace.untried = true;
				break;
			}
			string Message = "";
			if (previousVote == DisplayPlace.vote) {
				// have set to curren tsetting = unset
				var answer = await DisplayAlert (
					             "Remove Vote",
					             "If you remove your vote the place will not be on ANY of your lists",
					             "OK",
					             "Cancel");
				if (answer) {
					Insights.Track ("EditPage.DeletePlace", "Place", DisplayPlace.place_name);
					DisplayPlace.Delete ();
					Dirty = true;
					await Navigation.PopToRootAsync ();
				}
			} else if (DisplayPlace.Save (out Message)) {
				Dirty = true;
				Insights.Track ("DetailPage.SetVote", new Dictionary<string, string> {
					{ "PlaceName", DisplayPlace.place_name },
					{ "Vote", DisplayPlace.vote.ToString () },
					{ "Untried", DisplayPlace.untried.ToString () }
				});
				Device.BeginInvokeOnMainThread (() => {
					// manipulate UI controls
					SetVoteButton (sender as ButtonWide);
				});
			}
		}

		void ResetVoteButtons ()
		{
			VoteLike.TextColor = Color.Black;
			VoteDislike.TextColor = Color.Black;
			VoteWishlist.TextColor = Color.Black;
			VoteLike.BackgroundColor = Color.FromHex ("#444111111");
			VoteDislike.BackgroundColor = Color.FromHex ("#444111111");
			VoteWishlist.BackgroundColor = Color.FromHex ("#444111111");
		}

		void SetVoteButton (Button voteBtn)
		{
			ResetVoteButtons ();
			voteBtn.BackgroundColor = ColorUtil.Darker (settings.BaseColor);
			voteBtn.TextColor = Color.White;
		}

		object Lock = new object ();

		void LoadPage (string key)
		{
			lock (Lock) {
				try {

					DisplayPlace = (from p in Persist.Instance.Places
					                where p.key == key
					                select p).FirstOrDefault ();
					if (DisplayPlace == null) {
						Console.WriteLine ("LoadPage FAILED");
						return;
					}
					Console.WriteLine ("DetailsPage.LoadPage: dist is {0}", DisplayPlace.distance);
					
					Place_name.Text = DisplayPlace.place_name;
					Address.Text = DisplayPlace.address;
					if (!string.IsNullOrWhiteSpace (DisplayPlace.img)) {
						Img.Source = ImageSource.FromUri (new Uri (DisplayPlace.img));
						Img.HorizontalOptions = LayoutOptions.FillAndExpand;
						//Img.VerticalOptions = LayoutOptions.Start;
						Img.Aspect = Aspect.AspectFill;
					}
					Img.WidthRequest = this.Width;
					Img.HeightRequest = this.Height / 3;
					Category.Text = DisplayPlace.category;
					string comment;
					if (DisplayPlace.IsDraft)
						comment = DisplayPlace.DraftComment;
					else
						comment = DisplayPlace.Comment ();
					if (String.IsNullOrEmpty (comment))
						Comment.Text = "Click to Comment";
					else
						Comment.Text = '"' + DisplayPlace.Comment () + '"';

					CommentEditor.IsVisible = false;
					Comment.IsVisible = true;

					distance.Text = DisplayPlace.distance;
					if (string.IsNullOrWhiteSpace (DisplayPlace.website)) {
						WebBtn.Text = "No Website";
						WebBtn.IsEnabled = false;
					} else
						WebBtn.Text = "Go To Website";
					WebBtn.Clicked -= GotoWebPage;
					WebBtn.Clicked += GotoWebPage;
					if (string.IsNullOrWhiteSpace (DisplayPlace.telephone)) {
						CallBtn.Text = "No Phone Number";
						CallBtn.IsEnabled = false;
					} else
						CallBtn.Text = "Call Phone";
					VoteLike.TextColor = Color.Black;
					ResetVoteButtons ();
					switch (DisplayPlace.vote) {
					case "-1":
						SetVoteButton (VoteDislike);
						break;
					case "1":
						SetVoteButton (VoteLike);
						break;
					default:
						if (DisplayPlace.vote == "0" && DisplayPlace.untried == true)
							SetVoteButton (VoteWishlist);
						else
							ResetVoteButtons ();
						break;
					}
				} catch (Exception ex) {
					Insights.Report (ex);
				}
			}
		}

		#endregion

		#region Events

		void DoClickComment (object o, EventArgs e)
		{
			if (!DisplayPlace.iVoted) {
				DisplayAlert ("Comment", "You need to vote if you want to comment", "OK");
				return;
			}
			Comment.IsVisible = false;
			CommentEditor.IsVisible = true;
			CommentEditor.Text = DisplayPlace.Comment ();
			CommentEditor.Focus ();
		}

		void DoSaveComment (object o, EventArgs e)
		{
			try {
				DisplayPlace.setComment (CommentEditor.Text);
				CommentEditor.IsVisible = false;
				string msg;
				if (DisplayPlace.Save (out msg)) {
					Comment.IsVisible = true;
					Comment.Text = DisplayPlace.Comment ();
				} else
					DisplayAlert ("Error", "Couldn't save comment", "OK");
			} catch (Exception) {
				CommentEditor.Unfocus ();
			}
		}



		void DoEdit ()
		{
			Debug.WriteLine ("AddResultsPage.DoEdit: Push EditPage");
			Dirty = true;
			Navigation.PushAsync (new EditPage (DisplayPlace));
		}

		public void DoLoadPage (object sender, EventArgs e)
		{
			LoadPage (DisplayPlace.key);
		}

		public void GotoWebPage (object sender, EventArgs e)
		{
			if (DisplayPlace.website == null)
				return;
			Debug.WriteLine ("DetailPage.GotoWebPage: Push WebPage");
			Navigation.PushAsync (new WebPage (
				DisplayPlace.place_name,
				DisplayPlace.website));
		}

		void DoDirections (object sender, EventArgs e)
		{
			if (Device.OS == TargetPlatform.iOS) {
				//https://developer.apple.com/library/ios/featuredarticles/iPhoneURLScheme_Reference/MapLinks/MapLinks.html
				string uriString = String.Format ("http://maps.google.com/maps?daddr={0}", DisplayPlace.address);
				uriString = new Regex ("\\s+").Replace (uriString, "+");
				Device.OpenUri (new Uri (uriString));

			} else if (Device.OS == TargetPlatform.Android) {
				// opens the 'task chooser' so the user can pick Maps, Chrome or other mapping app
				Device.OpenUri (new Uri ("http://maps.google.com/?daddr=San+Francisco,+CA&saddr=Mountain+View"));

			} else if (Device.OS == TargetPlatform.WinPhone) {
				DisplayAlert ("To Do", "Not yet implemented", "OK");
			}
		}

		async void DoMakeCall (object sender, EventArgs e)
		{
			if (DisplayPlace.telephone == null)
				return;
			if (!await DisplayAlert (DisplayPlace.telephone, "Call this number?", "Yes", "No"))
				return;
			String EscapedNo = "";
			EscapedNo = Regex.Replace (DisplayPlace.telephone, @"[^0-9]+", "");
			if (!DependencyService.Get<IDeviceSpecific> ().MakeCall (EscapedNo)) {
				// Url is not able to be opened.
				await DisplayAlert ("Error", "Unable to call", "OK");
			}
		}

		#endregion

		public DetailPage (Place place, bool showToolbar = false, bool showMapBtn = true)
		{
			Analytics.TrackPage ("DetailPage");
			ShowToolbar = showToolbar;
			const int IMAGE_HEIGHT = 0;

			var MainGrid = new Grid {
				Padding = 2,
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },

					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },


				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			var CuisineAndDistanceGrid = new Grid {
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
		
			DisplayPlace = place;
			this.Appearing += DoLoadPage;

			Img = new Image ();
			try {
				Img.HorizontalOptions = LayoutOptions.FillAndExpand;
				//Img.VerticalOptions = LayoutOptions.Start;
				Img.Aspect = Aspect.AspectFill;
				Img.WidthRequest = this.Width;
			} catch {
				Img.Source = null;
			}
			//MainGrid.Children.Add (Img, 0, 2, 0, IMAGE_HEIGHT);
			Place_name = new LabelWide ();
			Place_name.FontAttributes = FontAttributes.Bold;
			MainGrid.Children.Add (Place_name, 0, 3, IMAGE_HEIGHT, IMAGE_HEIGHT + 1);
			Category = new LabelWide { };
			distance = new LabelWide {
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.End,
			};
			CuisineAndDistanceGrid.Children.Add (Category, 0, 0);
			CuisineAndDistanceGrid.Children.Add (distance, 1, 0);

			MainGrid.Children.Add (CuisineAndDistanceGrid, 0, 3, IMAGE_HEIGHT + 1, IMAGE_HEIGHT + 2);
			Address = new LabelWithImageButton {
				TextColor = Color.FromHex ("707070"),
				Source = "Icon default directions1.png",
				OnClick = DoDirections,
			};
			MainGrid.Children.Add (Address, 0, 3, IMAGE_HEIGHT + 2, IMAGE_HEIGHT + 3);

			WebBtn = new ButtonWide ();
			MainGrid.Children.Add (WebBtn, 0, 3, IMAGE_HEIGHT + 3, IMAGE_HEIGHT + 4);

			CallBtn = new ButtonWide ();
			CallBtn.Clicked += DoMakeCall;
			MainGrid.Children.Add (CallBtn, 0, 3, IMAGE_HEIGHT + 4, IMAGE_HEIGHT + 5);


			VoteLike = new ButtonWide {
				Text = LIKE_TEXT,
			};
			VoteLike.Clicked += SetVote;
			VoteDislike = new ButtonWide {
				Text = DISLIKE_TEXT,
			};
			VoteDislike.Clicked += SetVote;
			VoteWishlist = new ButtonWide {
				Text = WISH_TEXT,
			};
			VoteWishlist.Clicked += SetVote;
			MainGrid.Children.Add (VoteLike, 0, IMAGE_HEIGHT + 5);
			MainGrid.Children.Add (VoteWishlist, 1, IMAGE_HEIGHT + 5);
			MainGrid.Children.Add (VoteDislike, 2, IMAGE_HEIGHT + 5);

			Comment = new LabelWithImageButton {
				Source = "187-pencil@2x.png",
				OnClick = DoClickComment,
			};
			CommentEditor = new EntryWithButton {
				Source = "26-checkmark@2x.png",
				OnClick = DoSaveComment,
				IsVisible = false,
			};
			MainGrid.Children.Add (CommentEditor, 0, 3, IMAGE_HEIGHT + 6, IMAGE_HEIGHT + 7);
			MainGrid.Children.Add (Comment, 0, 3, IMAGE_HEIGHT + 6, IMAGE_HEIGHT + 7);

			LoadPage (DisplayPlace.key);

			ScrollView EditGrid = new ScrollView {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Content = new StackLayout {
					Children = {
						Img,
						MainGrid,
						GetFriendsComments (),
					}
				},
			};
			if (ShowToolbar) {
				StackLayout tools = new BottomToolbar (this, "add");
				Content = new StackLayout {
					Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
					Children = {
						EditGrid,
						tools
					}
				};
			} else {
				Content = EditGrid;
			}
			if (showMapBtn) {
				ToolbarItems.Add (new ToolbarItem {
					Text = "Map ",
					//				Icon = "icon-map.png",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => {
						Debug.WriteLine ("detailPage Map Toolbar: Push MapPage");
						Navigation.PushAsync (new MapPage (DisplayPlace));
					})
				});
			}
			ToolbarItems.Add (new ToolbarItem {
				Text = " Edit",
//				Icon = "187-pencil@2x.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => { 
					DoEdit ();
				})
			});
			this.Disappearing += (object sender, EventArgs e) => {
				OnClose (e);
			};
		}
	}
}
