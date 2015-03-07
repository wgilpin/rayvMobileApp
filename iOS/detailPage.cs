using System;
using Xamarin.Forms;
using Foundation;
using UIKit;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Xamarin;
using System.Collections.Generic;
using System.Linq;

namespace RayvMobileApp.iOS
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
		Label Address;
		Label descr;

		#endregion

		#region Logic

		Grid GetFriendsComments ()
		{
			string MyStringId = Persist.Instance.MyId.ToString ();
			List<Vote> voteList = (from v in Persist.Instance.Votes
			                       where v.key == DisplayPlace.key
			                           && v.voter != MyStringId
			                       select v).OrderBy (x => x.comment).ToList ();
			Grid grid = new Grid {
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (50.0) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			int whichRow = 0;
			for (int row = 0; row < voteList.Count (); row++) {
				if (voteList [row].voter != MyStringId) {
					try {

						grid.RowDefinitions.Add (new RowDefinition (){ Height = GridLength.Auto });
						grid.RowDefinitions.Add (new RowDefinition (){ Height = GridLength.Auto });
						string FriendName = Persist.Instance.Friends [voteList [whichRow].voter].Name;
						grid.Children.Add (new Label { Text = FriendName }, 0, 1, whichRow * 2, whichRow * 2 + 1);
						grid.Children.Add (new Label { 
							Text = voteList [row].PrettyHowLongAgo,
							Font = Font.SystemFontOfSize (NamedSize.Small),
							FontAttributes = FontAttributes.Italic,
							TextColor = Color.FromHex ("#606060"),
						}, 2, 4, whichRow * 2, whichRow * 2 + 1);
						String comment_text = voteList [row].PrettyComment;
						if (!String.IsNullOrEmpty (comment_text)) {
							grid.Children.Add (new Label { 
								Text = comment_text,
								FontAttributes = FontAttributes.Italic,
							}, 0, 4, whichRow * 2 + 1, whichRow * 2 + 2);
						}
						Label vote = new Label {
							Text = voteList [row].GetIconName,
						};
						grid.Children.Add (vote, 1, 2, whichRow * 2, whichRow * 2 + 1);
						whichRow++;
					} catch (Exception ex) {
						Console.WriteLine ("detailPage.GetFriendsComments {0}", ex);
						Insights.Report (ex);
					}
				}
			}
			return grid;
		}

		void SetVote (object sender, EventArgs e)
		{
			SetVoteButton (sender as ButtonWide);
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				// should NOT reference UILabel on background thread!
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
				if (DisplayPlace.Save (out Message)) {
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
			})).Start ();

		}

		void SetVoteButton (Button voteBtn)
		{
			VoteLike.TextColor = Color.Black;
			VoteDislike.TextColor = Color.Black;
			VoteWishlist.TextColor = Color.Black;
			VoteLike.BackgroundColor = Color.FromHex ("#444111111");
			VoteDislike.BackgroundColor = Color.FromHex ("#444111111");
			VoteWishlist.BackgroundColor = Color.FromHex ("#444111111");
			voteBtn.BackgroundColor = Color.Olive;
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
					if (DisplayPlace.img.Length > 0) {
						Img.Source = ImageSource.FromUri (new Uri (DisplayPlace.img));
						Img.HorizontalOptions = LayoutOptions.FillAndExpand;
						//Img.VerticalOptions = LayoutOptions.Start;
						Img.Aspect = Aspect.AspectFill;
						Img.WidthRequest = this.Width;
						Img.HeightRequest = this.Height / 3;
					}
					Category.Text = DisplayPlace.category;
					var comment = DisplayPlace.Comment ();
					if (comment != null && comment.Length > 0)
						descr.Text = '"' + DisplayPlace.Comment () + '"';
					else {
						descr.Text = null;
					}
					distance.Text = DisplayPlace.distance;
					if (DisplayPlace.website != null && DisplayPlace.website.Length > 0)
						WebBtn.Text = "Go To Website";
					else
						WebBtn.Text = "";
					WebBtn.Clicked -= GotoWebPage;
					WebBtn.Clicked += GotoWebPage;
					CallBtn.Text = DisplayPlace.telephone;
					VoteLike.TextColor = Color.Black;
					VoteDislike.TextColor = Color.Black;
					VoteWishlist.TextColor = Color.Black;
					VoteLike.BackgroundColor = Color.FromHex ("#444111111");
					VoteDislike.BackgroundColor = Color.FromHex ("#444111111");
					VoteWishlist.BackgroundColor = Color.FromHex ("#444111111");
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
						break;
					}
				} catch (Exception ex) {
					Insights.Report (ex);
				}
			}
		}

		#endregion

		#region Events

		void DoEdit ()
		{
			Debug.WriteLine ("AddResultsPage.DoEdit: Push EditPage");
			if (Navigation.NavigationStack [0] is ListPage) {
				(Navigation.NavigationStack [0] as ListPage).NeedsReload = true;
			}
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

		void DoMakeCall (object sender, EventArgs e)
		{
			if (DisplayPlace.telephone == null)
				return;
			String EscapedNo = "";
			EscapedNo = Regex.Replace (DisplayPlace.telephone, @"[^0-9]+", "");
			var urlToSend = new NSUrl ("tel:" + EscapedNo); // phonenum is in the format 1231231234

			if (UIApplication.SharedApplication.CanOpenUrl (urlToSend)) {
				Console.WriteLine ("DoMakeCall: calling {0}", EscapedNo);
				UIApplication.SharedApplication.OpenUrl (urlToSend);
			} else {
				// Url is not able to be opened.
				DisplayAlert ("Error", "Unable to call", "OK");
			}
		}

		#endregion

		public DetailPage (Place place)
		{
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
			Address = new LabelWide ();
			MainGrid.Children.Add (Address, 0, 3, IMAGE_HEIGHT + 2, IMAGE_HEIGHT + 3);

			WebBtn = new ButtonWide ();
			MainGrid.Children.Add (WebBtn, 0, 3, IMAGE_HEIGHT + 3, IMAGE_HEIGHT + 4);

			CallBtn = new ButtonWide ();
			CallBtn.Clicked += DoMakeCall;
			MainGrid.Children.Add (CallBtn, 0, 3, IMAGE_HEIGHT + 4, IMAGE_HEIGHT + 5);

			descr = new LabelWide ();
			MainGrid.Children.Add (descr, 0, 3, IMAGE_HEIGHT + 5, IMAGE_HEIGHT + 6);
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
			MainGrid.Children.Add (VoteLike, 0, IMAGE_HEIGHT + 6);
			MainGrid.Children.Add (VoteWishlist, 1, IMAGE_HEIGHT + 6);
			MainGrid.Children.Add (VoteDislike, 2, IMAGE_HEIGHT + 6);
			LoadPage (DisplayPlace.key);

			this.Content = new ScrollView {
				Content = new StackLayout {
					Children = {
						Img,
						MainGrid,
						GetFriendsComments (),
					}
				},
			};
			ToolbarItems.Add (new ToolbarItem {
				Text = "Map",
//				Icon = "icon-map.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("detailPage Map Toolbar: Push MapPage");
					Navigation.PushAsync (new MapPage (DisplayPlace));
				})
			});
			ToolbarItems.Add (new ToolbarItem {
				Text = "Edit",
//				Icon = "187-pencil@2x.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => { 
					DoEdit ();
				})
			});
		}
	}
}
