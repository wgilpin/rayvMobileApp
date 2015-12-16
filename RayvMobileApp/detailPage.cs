using System;
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

	class TopRowBtn: ImageButton
	{
		public TopRowBtn () : base ()
		{
			HeightRequest = 30;
			HorizontalOptions = LayoutOptions.CenterAndExpand;
			Aspect = Aspect.AspectFit;
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

		public Place DisplayPlace;
		Label Place_name;
		Grid ImgGrid;
		Image Img;
		Label CuisineEd;

		ActivityIndicator Spinner;
		Label Address;
		Label Comment;
		EditCommentView CommentEditor;
		//		private bool ShowToolbar;
		public bool Dirty;
		bool IsNew;
		TopRowBtn WebImgBtn;
		TopRowBtn TelImgBtn;
		Frame SaveFrame;
		StackLayout tools;
		StarEditor Stars;
		View MainContent;
		StackLayout FriendComments;


		#endregion

		#region Logic


		// set the list of friends' comments into the FriendsComments StackLayout
		void SetupFriendsComments ()
		{
			FriendComments.Children.Clear ();
			FriendComments.Children.Add (new LabelWide ("Comments"));
			try {
				string MyStringId = Persist.Instance.MyId.ToString ();
				Persist.Instance.Votes
					.Where (v => v.key == DisplayPlace.key && v.voter != MyStringId)
					.OrderBy (x => x.when)
					.ToList ()
					.ForEach (vote => {
					try {
						Persist.Instance.CommentCache[vote.voteId] = Persist.Instance.LoadComments (vote);
						int replies = Persist.Instance.CommentCache[vote.voteId].Count;
						if (vote.replies != replies){
							vote.replies = replies;
							Persist.Instance.SaveVoteToDb (vote);
						}
						CommentViewGrid entry = new CommentViewGrid (vote, onReply:DoCommentTapped, showReplyBtn:true);
						FriendComments.Children.Add (entry);
					} catch (Exception ex) {
						Debug.Fail ("SetupFriendsComments inner Exception {ex.Message}");
					}
					});
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}



		// load the Place DisplayPlace into the page UI
		void LoadPage (Place place)
		{
			lock (DisplayPlace) {
				try {
//					if (!string.IsNullOrEmpty (place.key)) {
//						DisplayPlace = (from p in Persist.Instance.Places
//						                where p.key == place.key
//						                select p).FirstOrDefault ();
//					}
//					if (DisplayPlace == null) {
//						DisplayPlace = place;
//					}
//					if (DisplayPlace == null) {
//						Console.WriteLine ("LoadPage FAILED");
//						return;
//					}
					Console.WriteLine ("DetailsPage.LoadPage: dist is {0}", DisplayPlace.distance);
					
					var pn = new FormattedString ();
					pn.Spans.Add (new Span { 
						Text = $" {DisplayPlace.place_name}",
						FontSize = settings.FontSizeLabelLarge,
						ForegroundColor = Color.Black,
						FontAttributes = FontAttributes.Bold,
					});
					pn.Spans.Add (new Span { 
						Text = $"  {DisplayPlace.distance}",
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
						ForegroundColor = Color.Gray
					});
						
					Place_name.FormattedText = pn;
					Address.Text = DisplayPlace.address;
					Address.IsVisible = !string.IsNullOrWhiteSpace (DisplayPlace.address);
					if (!string.IsNullOrWhiteSpace (DisplayPlace.img)) {
						Img.HorizontalOptions = LayoutOptions.FillAndExpand;
						//Img.VerticalOptions = LayoutOptions.Start;
						Img.Aspect = Aspect.AspectFill;
						Img.Source = ImageSource.FromUri (new Uri (DisplayPlace.img));
						Img.WidthRequest = this.Width;
						Img.HeightRequest = this.Height / 3;
					} else {
						Img.HorizontalOptions = LayoutOptions.Center;
						Img.VerticalOptions = LayoutOptions.Center;
						Img.Source = settings.DevicifyFilename ("default_image.png");
					}
					ImgGrid.HeightRequest = this.Height / 3;
					CuisineEd.Text = DisplayPlace.vote.cuisineName;
					string comment;
					if (DisplayPlace.IsDraft)
						comment = DisplayPlace.DraftComment;
					else
						comment = DisplayPlace.Comment ();
					if (String.IsNullOrEmpty (comment))
						Comment.Text = "Click to Comment";
					else
						Comment.Text = $"\"{DisplayPlace.Comment ()}\"";


					WebImgBtn.IsVisible = !string.IsNullOrWhiteSpace (DisplayPlace.website);
					TelImgBtn.IsVisible = !string.IsNullOrWhiteSpace (DisplayPlace.telephone);
					Stars.Untried = DisplayPlace.vote.untried;
					Stars.Vote = DisplayPlace.vote.vote;
				} catch (Exception ex) {
					Console.Write ("Details page ctor {0}", ex);
					Insights.Report (ex);
				}
			}
		}

		#endregion

		#region Events

		// Event handler afet comments are edited to reload the lsit
		void DoCommentsSaved (object sender, EventArgs e)
		{
			SetupFriendsComments ();
		}

		//Event handler when a comment ios tapped, opens the comment list page for reply / browse
		public void DoCommentTapped(Object sender, EventArgs e)
		{
			var voteKey = (sender as Element).StyleId;
			var vote = Persist.Instance.Votes.Where (v => v.voteId.ToString () == voteKey).FirstOrDefault ();
			if (vote != null) {
				var commentsPage = new CommentReplyPage (vote);
				commentsPage.Finished += DoCommentsSaved;
				Navigation.PushAsync (commentsPage);
			}
		}



		// Event handler for when a StarEditor has been clicked
		void DoSaveVote (object sender, StarEditorEventArgs voteArgs)
		{
			if (voteArgs.Vote == DisplayPlace.vote.vote && voteArgs.Untried == DisplayPlace.vote.untried)
				return;
			DisplayPlace.vote.vote = voteArgs.Vote;
			DisplayPlace.vote.untried = voteArgs.Vote == 0 ? voteArgs.Untried : false;
			if (!string.IsNullOrEmpty (DisplayPlace.key)) {
				string msg = "";
				DisplayPlace.SaveVote (out msg);
			}
			Dirty = true;
		}

		// Edit the comment for the current user (new page content)
		void EditComment ()
		{
			CommentEditor = new EditCommentView (
				DisplayPlace.Comment (), 
				inFlow: false,
				vote: DisplayPlace.vote.vote);
			CommentEditor.Saved += DoSaveComment;
			CommentEditor.NoComment += (sender, e) => {
				// alert when a comment is missing
				DisplayAlert ("No Comment", "You have to comment", "OK");
			};
			CommentEditor.Cancelled += (s, e) => {
				// cnacelled edit, re-display the page
				Content = MainContent;
			};
			Content = CommentEditor;
		}

		// Event handler when the comment is clicked. Either force a vote if there isn't one, or edit comment if there is
		void DoClickComment (object o, EventArgs e)
		{
			if (DisplayPlace.vote.vote == Vote.VoteNotSetValue && !DisplayPlace.vote.untried) {
				DoEdit ();
				return;
			}
			EditComment ();
		}

		// save comment to server
		void DoSaveComment (object o, CommentSavedEventArgs e)
		{
			try {
				DisplayPlace.setComment (e.Comment);
				string msg;
				ShowSpinner ();
				new System.Threading.Thread (new System.Threading.ThreadStart (() => {
					if (!string.IsNullOrEmpty (DisplayPlace.key)) {
						if (DisplayPlace.SaveVote (out msg)) {
							Device.BeginInvokeOnMainThread (() => {
								Comment.Text = '"' + DisplayPlace.Comment () + '"';
								ShowSpinner (false);
								Content = MainContent;
							});
						} else
							Device.BeginInvokeOnMainThread (() => {
								ShowSpinner (false);
								DisplayAlert ("Error", "Couldn't save comment", "OK");
								Content = MainContent;
							});
					}
				})).Start ();

			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		// remove my vote for this place
		void DoRemove (object sender, EventArgs e)
		{
			int votesCount = Persist.Instance.Votes.Where (v => v.key == DisplayPlace.key).Count ();
			if (votesCount == 0) {
				// the place has gone
				Navigation.PushModalAsync (new RayvNav (new ListPage ()));
			} else {
				// still some votes
				Navigation.PopAsync ();
				LoadPage (DisplayPlace);
			}
		}

		// edit the DisplayPlace via the PlaceEditor
		void DoEdit ()
		{
			Debug.WriteLine ("Detail.DoEdit: Push EditPage");
			Dirty = true;
			var editor = new PlaceEditor (DisplayPlace, false);
			editor.Saved += DoSave;
			editor.Removed += DoRemove;
			editor.Cancelled += (s, ev) => Navigation.PopAsync ();
			Navigation.PushAsync (editor);
		}

		// event handler to load the DisplayPlace into the UI. Called in the Page Appearing event
		public void DoLoadPage (object sender, EventArgs e)
		{
			LoadPage (DisplayPlace);
		}

		void PushWithNavigation (ContentPage page)
		{
			if (Navigation.NavigationStack.Count == 0)
				Navigation.PushModalAsync (new RayvNav (page));
			else
				Navigation.PushAsync (page);
		}

		public void SharePlace (object sender, EventArgs e)
		{
			var sharer = DependencyService.Get<IShareable> ();
			var shareBody = 
				$"Let's go to {DisplayPlace.place_name}\n" +
				$"{DisplayPlace.address}\n"+
				$"http://maps.google.com/maps?daddr={DisplayPlace.lat},{DisplayPlace.lng}\n"+
				"Sent by Sprout";
			Console.WriteLine (shareBody);
			sharer.OpenShareIntent (shareBody);

		}

		public void GotoWebPage (object sender, EventArgs e)
		{
			try {
				if (DisplayPlace.website == null)
					return;
				Debug.WriteLine ("DetailPage.GotoWebPage: Push WebPage");
				var web = new WebPage (
					          DisplayPlace.place_name,
					          DisplayPlace.website);
				PushWithNavigation (web);
			} catch (Exception ex) {
				DisplayAlert ("Internal Error", "Unable to show web page", "OK");
				Console.WriteLine ($"GotoWebPage ERROR {ex}");
				Insights.Report (ex);
			}

		}
		void DoDirections (object sender, EventArgs e)
		{
			try {
				if (string.IsNullOrEmpty (DisplayPlace.address))
					return;
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
			} catch (Exception ex) {
				DisplayAlert ("Internal Error", "Unable to get directions", "OK");
				Console.WriteLine ($"DoDirections ERROR {ex}");
				Insights.Report (ex);
			}
		}

		async void DoMakeCall (object sender, EventArgs e)
		{
			try {
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
			} catch (Exception ex) {
				await DisplayAlert ("Internal Error", "Unable to call", "OK");
				Console.WriteLine ($"DoMakeCall ERROR {ex}");
				Insights.Report (ex);
			}
		}

		void ShowSpinner (bool IsVisible = true)
		{
			Debug.WriteIf (!IsVisible, "Spinner off");
			Spinner.IsRunning = IsVisible;
			Spinner.IsVisible = IsVisible;
		}

		async void SaveWasGood ()
		{
			Console.WriteLine ("Saved - PopToRootAsync");
			Insights.Track ("DetailPage.DoSave", new Dictionary<string, string> {
				{ "PlaceName", DisplayPlace.place_name },
				{ "Lat", DisplayPlace.lat.ToString () },
				{ "Lng", DisplayPlace.lng.ToString () },
				{ "Vote", DisplayPlace.vote.ToString () },
			});
			ShowSpinner (false);
			Navigation.PopAsync ();
			await DisplayAlert ("Saved", "Details Saved", "OK");
			Persist.Instance.HaveAdded = this.IsNew;
			SaveFrame.IsVisible = true;
			tools.IsVisible = true;
		}

		async void SaveWasBad (String msg = null)
		{
			ShowSpinner (false);
			DisplayPlace.IsDraft = true;
			await DisplayAlert ("Not Saved", msg ?? "Kept as draft", "OK");
			Navigation.PopAsync ();
			Persist.Instance.Places.Add (DisplayPlace);
			SaveFrame.IsVisible = false;
		}

		void DoSave (object sender = null, EventArgs e = null)
		{
			String Message = null;
			ShowSpinner ();
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				if (DisplayPlace.Save (out Message)) {
					Device.BeginInvokeOnMainThread (() => {
						SaveWasGood ();
					});
				} else {
					Device.BeginInvokeOnMainThread (() => {
						SaveWasBad (Message);
					});
				}

			})).Start ();
		}

		#endregion
		Grid GetCommentLine(Label comment, int count, long voteId)
		{
			var grid = new Grid {
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (RoundButton.letterButtonSize) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) }
				},
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			RoundButton CountIndicator = new RoundButton() {
				BackgroundColor = settings.BaseDarkColor,
				Text = count.ToString (),
				StyleId = voteId.ToString (),
			};
			CountIndicator.OnClick = DoCommentTapped;
			grid.Children.Add (comment,0,0);
			grid.Children.Add (CountIndicator,1,0);
			return grid;
		}

		Label GetVoteCountLabel ()
		{
			// get the overall vote score
			Double score;
			try {
				score = Persist.Instance.Votes.Where (v => v.key == DisplayPlace.key && v.vote > 0).Select (v => v.vote).Average ();
			} catch (Exception ex) {
				Console.WriteLine ($"ERROR {ex}");
				Insights.Report (ex);
				score = 0;
			}
			return new Label{ Text = $"({score:F1})", FontSize = settings.FontSizeLabelMedium, TextColor = settings.ColorDarkGray, TranslationY = 5}; 
		}

		public DetailPage (
			Place place, bool showToolbar = false, bool showMapBtn = true, bool showSave = false, bool isDraft = false)
		{
			Analytics.TrackPage ("DetailPage");
			DisplayPlace = place;
			IsNew = showSave;
			BackgroundColor = Color.White;
//			ShowToolbar = showToolbar;
			const int IMAGE_HEIGHT = 0;
			Spinner = new ActivityIndicator {
				BackgroundColor = Color.FromRgba (55, 55, 55, 0.5),
				IsVisible = false,
				Color = Color.Red,
			};
			var TopRow = new StackLayout {
				Padding = 1,
				Spacing = 0,
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};

			DisplayPlace = place;
			this.Appearing += DoLoadPage;

			SaveFrame = new Frame {
				OutlineColor = Color.Transparent,
				Padding = 2,
				Content = new Label {
					Text = "Saved",
					TextColor = ColorUtil.Lighter (Color.Red),
					FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
					HorizontalOptions = LayoutOptions.Center
				},
				HasShadow = false,
				IsVisible = showSave,
			};

			Img = new Image ();
			try {
				Img.HorizontalOptions = LayoutOptions.FillAndExpand;
				//Img.VerticalOptions = LayoutOptions.Start;
				Img.Aspect = Aspect.AspectFill;
				Img.WidthRequest = this.Width;
			} catch {
				Img.Source = null;
			}
			ImgGrid = new Grid {
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (40) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) }
				}
			};
			ImgGrid.Children.Add (Img, 0, 2, 0, 4);
			//MainGrid.Children.Add (Img, 0, 2, 0, IMAGE_HEIGHT);

			Place_name = new Label ();
//			Place_name.FontAttributes = FontAttributes.Bold;
//			Place_name.FontSize = settings.FontSizeLabelLarge;
//			Place_name.XAlign = TextAlignment.Center;
			CuisineEd = new LabelWide { };

			Address = new Label {
				TextColor = Color.FromHex ("707070"),
				FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
			};
			var DirectionsImgBtn = new TopRowBtn () { 
				Source = settings.DevicifyFilename ("directions_white.png"),
				OnClick = DoDirections
			};
			WebImgBtn = new TopRowBtn { 
				Source = settings.DevicifyFilename ("Web_white2.png"),
				//"Web_white.png"),
				OnClick = GotoWebPage
			};
			var ShareImgBtn = new TopRowBtn { 
				Source = settings.DevicifyFilename ("Share.png"),
				//"Web_white.png"),
				OnClick = SharePlace
			};
			TelImgBtn = new TopRowBtn {
				Source = settings.DevicifyFilename ("phone_white.png"),
				OnClick = DoMakeCall,
			};
			var GreyBar = new Frame {
				BackgroundColor = Color.FromRgba (100, 100, 100, 100),
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HasShadow = false,
				Padding = 0,
			};
			GreyBar.OutlineColor = Color.Transparent;
			GreyBar.Content = null;
			ImgGrid.Children.Add (GreyBar, 1, 2, 0, 4);
			ImgGrid.Children.Add (TelImgBtn, 1, 0);
			ImgGrid.Children.Add (DirectionsImgBtn, 1, 1);
			ImgGrid.Children.Add (WebImgBtn, 1, 2);
			ImgGrid.Children.Add (ShareImgBtn, 1, 3);

//			TopRow.Children.Add (TelImgBtn);
//			TopRow.Children.Add (VoteImgBtn);
//			TopRow.Children.Add (DirectionsImgBtn);
//			TopRow.Children.Add (WebImgBtn);

			Comment = new Label {
				FontAttributes = FontAttributes.Italic,
			};
			var tapComment = new TapGestureRecognizer ();
			tapComment.Tapped += DoClickComment;
			Comment.GestureRecognizers.Add (tapComment);

			var VoteCountLbl = GetVoteCountLabel ();
			Stars = new StarEditor (true) { HorizontalOptions = LayoutOptions.FillAndExpand };
			Stars.ChangedNotUI += DoSaveVote;
			var StarLine = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Start
			};
			StarLine.Children.Add (Stars);
			StarLine.Children.Add (VoteCountLbl);
			Label DraftText = new Label {
				Text = "DRAFT",
				TextColor = Color.Red,
				FontAttributes = FontAttributes.Bold,
				FontSize = settings.FontSizeLabelLarge,
				XAlign = TextAlignment.Center,
				IsVisible = false,
			};
			if (isDraft) {
				DraftText.IsVisible = true;
			} else
				LoadPage (DisplayPlace);
			var styleGrid = new Grid {
				Padding = new Thickness (5, 20, 5, 5),
				RowSpacing = 10,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (100) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
//					new RowDefinition { Height = new GridLength (2) },
				}
			};

			styleGrid.Children.Add (new Label{ FontAttributes = FontAttributes.Bold, Text = "Cuisine" }, 0, 0); 
			styleGrid.Children.Add (new Label{ FontAttributes = FontAttributes.Bold, Text = "Price" }, 0, 1); 
			styleGrid.Children.Add (new Label{ FontAttributes = FontAttributes.Bold, Text = "Meal" }, 0, 2); 
			styleGrid.Children.Add (new Label{ Text = DisplayPlace.vote.cuisineName }, 1, 0); 
			styleGrid.Children.Add (new Label{ Text = DisplayPlace.vote.style.ToFriendlyString () }, 1, 1); 
			styleGrid.Children.Add (new Label{ Text = DisplayPlace.vote.kind.ToString () }, 1, 2); 
//			styleGrid.Children.Add (new Frame{ HasShadow = false, OutlineColor = Color.Gray, HeightRequest = 2 }, 0, 2, 3, 4); 
			Vote myVote = null;
			Vote votersVote = null;
			var voter = Persist.Instance.FilterWhoKey;
			Persist.Instance.Votes.Where (v => v.key == place.key).ToList ().ForEach (vo => {
				Console.WriteLine ($"{place.place_name} {vo.VoterName}");
				if (vo.voter == Persist.Instance.MyId.ToString ())
					myVote = vo	;
				else if (!string.IsNullOrEmpty (voter) && vo.voter == voter)
					votersVote = vo;
				else if (votersVote == null)
					votersVote = vo;
			});
//			Persist.Instance.Votes.ForEach (v=>{
//				try{
//					Console.WriteLine ($"{v.place_name} {v.key}");
//					if(v.key == place.key)
//						if (v.voter == voter)
//							myVote = v;
//				}
//				catch (Exception ex){
//					Console.WriteLine (ex);
//				}
//			});
			Vote theVote = votersVote ?? myVote;
			FriendComments = new StackLayout ();
			SetupFriendsComments ();
			ScrollView EditGrid = new ScrollView {
				Padding = 2,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Start,
				Content = new StackLayout {
					Children = {
						SaveFrame,
						Spinner,
						Place_name,
						Address,
						ImgGrid,
						TopRow,
						StarLine,
						styleGrid,
						GetCommentLine(Comment,myVote?.replies??0,myVote?.voteId??-1),
						new Frame{ HasShadow = false, OutlineColor = settings.ColorMidGray, Padding = 0, HeightRequest = 1 }, 
						FriendComments,
					}
				},
			};
			tools = new BottomToolbar (this, "add"){ IsVisible = showToolbar };
			Content = new StackLayout {
//				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				Children = {
					EditGrid,
					tools
				}
			};
			MainContent = Content;
			if (showMapBtn) {
				ToolbarItems.Add (new ToolbarItem {
					Text = "Map ",
					//				Icon = "icon-map.png",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => {
						Debug.WriteLine ("detailPage Map Toolbar: Push MapPage");
						PushWithNavigation (new MapPage (DisplayPlace));
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
//				if (Dirty) {
//					if (await DisplayAlert ("Not Saved", "You will lose your changes", "Save", "Ignore"))
//						DoSave (sender, e);
//				}
				OnClose (e);
			};
		} 
	}
}
