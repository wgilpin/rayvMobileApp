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
		Image Img;
		Label CuisineEd;

		ActivityIndicator Spinner;
		Label distance;
		Label Address;
		LabelWithImageButton Comment;
		EditCommentPage CommentEditor;
		//		private bool ShowToolbar;
		public bool Dirty;
		bool IsNew;
		TopRowBtn WebImgBtn;
		TopRowBtn TelImgBtn;
		TopRowBtn VoteImgBtn;
		Frame SaveFrame;
		StackLayout tools;
		EditVotePage VotePage;

		#endregion

		#region Logic

		static Grid AddFriendCommentToGrid (Vote vote, Grid grid, int whichRow)
		{
			try {
				grid.RowDefinitions.Add (new RowDefinition () {
					Height = GridLength.Auto
				});
				grid.RowDefinitions.Add (new RowDefinition () {
					Height = GridLength.Auto
				});
				grid.RowDefinitions.Add (new RowDefinition () {
					Height = GridLength.Auto
				});
				var Separator = new BoxView { 
					BackgroundColor = Color.Gray, 
					HeightRequest = 1, 
					WidthRequest = 600, 
					Opacity = 0.5,
					VerticalOptions = LayoutOptions.Start,
				};
				grid.Children.Add (Separator, 0, 5, whichRow * 3, whichRow * 3 + 1);
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
				LetterBtn.Text = Vote.FirstLetter (vote.VoterName);
				LetterBtn.BackgroundColor = Vote.RandomColor (vote.VoterName);
				grid.Children.Add (LetterBtn, 0, 1, whichRow * 3 + 1, whichRow * 3 + 2);
				var FriendLine = new FormattedString ();

				string voter = "";
				try {
					voter = Persist.Instance.Friends [vote.voter].Name;
				} catch (Exception ex) {
					var data = new Dictionary<string,string> { 
						{ "Friend", $"{vote.voter}" },
						{ "Vote",$"{vote.Id}" }
					};
					Insights.Report (ex, data);
					throw new KeyNotFoundException ();
				}
				FriendLine.Spans.Add (new Span {
					Text =$"{voter}  {vote.GetVoteAsString}  ",
				});
				FriendLine.Spans.Add (new Span {
					Text = vote.PrettyHowLongAgo,
					FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					ForegroundColor = Color.FromHex ("#606060"),
				});
				grid.Children.Add (new Label { FormattedText = FriendLine, }, 1, 5, whichRow * 3 + 1, whichRow * 3 + 2);
				String comment_text = vote.PrettyComment;
				if (!String.IsNullOrEmpty (comment_text)) {
					grid.Children.Add (new Label {
						Text = comment_text,
						TextColor = settings.ColorDarkGray,
						FontAttributes = FontAttributes.Italic,
					}, 0, 5, whichRow * 3 + 2, whichRow * 3 + 3);
				}
			} catch (KeyNotFoundException) {
				// already handled
			} catch (Exception ex) {
				Console.WriteLine ("detailPage.AddFriendCommentToGrid {0}", ex);
				Insights.Report (ex);
			}
			return grid;
		}

		Grid GetFriendsComments ()
		{
			GridWithCounter friendCommentsGrid = new GridWithCounter {
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (31) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (70) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
			};
			friendCommentsGrid.ShowGrid = true;

			try {
				int whichRow = 0;
				string MyStringId = Persist.Instance.MyId.ToString ();
				Persist.Instance.Votes
					.Where (v => v.key == DisplayPlace.key && v.voter != MyStringId)
					.OrderBy (x => x.when)
					.ToList ().ForEach (vote => {
					AddFriendCommentToGrid (vote, friendCommentsGrid, whichRow);
					whichRow += 2;
				});
			} catch (Exception ex) {
				Insights.Report (ex);
			}
			return friendCommentsGrid;
		}

		async void SetVote (object sender, EventArgs e)
		{
			SetVoteButton (sender as ButtonWide);
			// should NOT reference UILabel on background thread!
			VoteValue previousVote = DisplayPlace.vote.vote;
			switch ((sender as ButtonWide).Text) {
				case LIKE_TEXT:
					DisplayPlace.vote.vote = VoteValue.Liked;
					break;
				case DISLIKE_TEXT:
					DisplayPlace.vote.vote = VoteValue.Disliked;
					break;
				case WISH_TEXT:
					DisplayPlace.vote.vote = VoteValue.Untried;
					break;
			}
			string Message = "";
			if (previousVote == DisplayPlace.vote.vote) {
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
					if (Navigation.NavigationStack.Count > 0)
						await Navigation.PopToRootAsync ();
					else {
						tools.IsVisible = true;
					}
				}
			} else {
				new System.Threading.Thread (new System.Threading.ThreadStart (() => {
					Device.BeginInvokeOnMainThread (() => {
						ShowSpinner ();
					});
					if (DisplayPlace.SaveVote (out Message)) {
						Dirty = true;
						var details = new Dictionary<string, string> {
							{ "PlaceName", DisplayPlace.place_name },
							{ "Vote", DisplayPlace.vote.ToString () },
						};
						Insights.Track ("DetailPage.SetVote", details);
						Debug.WriteLine ("UpdateVote {0}", details.ToString ());
					}
					Device.BeginInvokeOnMainThread (() => {
						// manipulate UI controls
						SetVoteButton (sender as ButtonWide);
						ShowSpinner (false);
						if (string.IsNullOrWhiteSpace (DisplayPlace.Comment ())) {
							DisplayAlert ("No Comment", "Please add a comment to explain your vote", "OK");
							DoClickComment (null, null);
							EditComment ();
						}
					});
				})).Start ();
			}
		}

		void SetVoteButton (Button voteBtn)
		{
			voteBtn.BackgroundColor = ColorUtil.Darker (settings.BaseColor);
			voteBtn.TextColor = Color.White;
		}


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
						FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
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
					} else {
						Img.HorizontalOptions = LayoutOptions.Center;
						Img.VerticalOptions = LayoutOptions.Center;
						Img.Source = settings.DevicifyFilename ("logo.png");
					}
					Img.WidthRequest = this.Width;
					Img.HeightRequest = this.Height / 3;
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

					Comment.IsVisible = true;

					if (string.IsNullOrWhiteSpace (DisplayPlace.website)) {
						WebImgBtn.IsEnabled = false;
						WebImgBtn.Source = settings.DevicifyFilename ("Icon_active_Website.png");
					} else {
						WebImgBtn.Source = settings.DevicifyFilename ("Icon default Website.png");
						WebImgBtn.IsEnabled = true;
					}
					if (string.IsNullOrWhiteSpace (DisplayPlace.telephone)) {
						TelImgBtn.IsEnabled = false;
						TelImgBtn.Source = settings.DevicifyFilename ("Icon_active_Phone.png");
					} else {
						TelImgBtn.Source = settings.DevicifyFilename ("Icon default Phone.png");
						TelImgBtn.IsEnabled = true;
					}
					switch (DisplayPlace.vote.vote) {
						case VoteValue.Disliked:
							VoteImgBtn.Source = "Dislike.png";
							break;
						case VoteValue.Liked:
							VoteImgBtn.Source = "Like.png";
							break;
						case VoteValue.Untried:
							VoteImgBtn.Source = "Wish1.png";
							break;
						default:
							VoteImgBtn.Source = "Add_Vote.png";
							break;
					}
				} catch (Exception ex) {
					Console.Write ("Details page ctor {0}", ex);
					Insights.Report (ex);
				}
			}
		}

		void SetDirty ()
		{
			SaveFrame.IsVisible = true;
			Dirty = true;
		}

		#endregion

		#region Events

		void VoteSaved (object sender, VoteSavedEventArgs voteArgs)
		{
			if (voteArgs.Vote == DisplayPlace.vote.vote)
				return;
			DisplayPlace.vote.vote = voteArgs.Vote;
			switch (voteArgs.Vote) {
				case VoteValue.None:
					VoteImgBtn.Source = "Add_Vote.png";
					break;
				case VoteValue.Liked:
					VoteImgBtn.Source = "Like.png";
					break;
				case VoteValue.Disliked:
					VoteImgBtn.Source = "Dislike.png";
					break;
				case VoteValue.Untried:
					VoteImgBtn.Source = "Wish1.png";
					break;
			}
			if (!string.IsNullOrEmpty (DisplayPlace.key)) {
				string msg = "";
				DisplayPlace.SaveVote (out msg);
			}
			Dirty = true;
			VotePage.Navigation.PopModalAsync ();
		}

		void DoVote (object o, EventArgs e)
		{
			VotePage = new EditVotePage (DisplayPlace.vote.vote, inFlow: false);
			VotePage.Saved += VoteSaved;
			VotePage.Cancelled += (sender, ev) => {
				VotePage.Navigation.PopModalAsync ();
			};
			Navigation.PushModalAsync (new RayvNav (VotePage));
		}

		void EditComment ()
		{
			CommentEditor = new EditCommentPage (
				DisplayPlace.Comment (), 
				inFlow: false,
				vote: DisplayPlace.vote.vote);
			CommentEditor.Saved += DoSaveComment;
			CommentEditor.Cancelled += (s, e) => {
				CommentEditor?.Navigation.PopModalAsync ();
			};
			Navigation.PushModalAsync (new RayvNav (CommentEditor));
		}

		void DoClickComment (object o, EventArgs e)
		{
			if (!DisplayPlace.iVoted) {
				DisplayAlert ("Comment", "You need to vote if you want to comment", "OK");
				return;
			}
			Comment.IsVisible = false;
			EditComment ();
		}

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
							});
						} else
							Device.BeginInvokeOnMainThread (() => {
								ShowSpinner (false);
								DisplayAlert ("Error", "Couldn't save comment", "OK");
							});
					}
				})).Start ();

			} catch (Exception) {
			}
			CommentEditor?.Navigation.PopModalAsync ();
		}

		void DoEdit ()
		{
			Debug.WriteLine ("Detail.DoEdit: Push EditPage");
			Dirty = true;
			var editor = new PlaceEditor (DisplayPlace, this, false);
			editor.Edit ();
		}

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

		public void GotoWebPage (object sender, EventArgs e)
		{
			if (DisplayPlace.website == null)
				return;
			Debug.WriteLine ("DetailPage.GotoWebPage: Push WebPage");
			var web = new WebPage (
				          DisplayPlace.place_name,
				          DisplayPlace.website);
			PushWithNavigation (web);

		}

		void DoDirections (object sender, EventArgs e)
		{
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
			await DisplayAlert ("Saved", "Details Saved", "OK");
			Persist.Instance.HaveAdded = this.IsNew;
			SaveFrame.IsVisible = false;
			tools.IsVisible = true;
			if (Navigation.NavigationStack.Count > 0)
				await Navigation.PopToRootAsync ();
		}

		async void SaveWasBad ()
		{
			ShowSpinner (false);
			DisplayPlace.IsDraft = true;
			await DisplayAlert ("Not Saved", "Kept as draft", "OK");
			Persist.Instance.Places.Add (DisplayPlace);
			SaveFrame.IsVisible = true;
		}

		void DoSave (object sender = null, EventArgs e = null)
		{
			string Message = "";
			ShowSpinner ();
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				if (DisplayPlace.Save (out Message)) {
					Device.BeginInvokeOnMainThread (() => {
						SaveWasGood ();
					});
				} else {
					Device.BeginInvokeOnMainThread (() => {
						SaveWasBad ();
					});
				}

			})).Start ();
		}

		#endregion

		Frame GetVoteCountText ()
		{
			Double MedFont = Device.GetNamedSize (NamedSize.Medium, typeof(Label));
			var VoteCountText = new FormattedString ();
			VoteCountText.Spans.Add (new Span {
				Text = "Likes   ",
				FontSize = MedFont,
				ForegroundColor = Color.Green,
				FontAttributes = FontAttributes.Italic
			});
			VoteCountText.Spans.Add (new Span {
				Text = DisplayPlace.up.ToString (),
				ForegroundColor = Color.Gray,
				FontSize = MedFont,
				FontAttributes = FontAttributes.Bold,
			});
			VoteCountText.Spans.Add (new Span {
				Text = "        Dislikes   ",
				FontSize = MedFont,
				ForegroundColor = Color.Red,
				FontAttributes = FontAttributes.Italic
			});
			VoteCountText.Spans.Add (new Span {
				Text = DisplayPlace.down.ToString (),
				ForegroundColor = Color.Gray,
				FontSize = MedFont,
				FontAttributes = FontAttributes.Bold,
			});
			return new Frame {
				OutlineColor = Color.Transparent, 
				Padding = new Thickness (2, 10, 2, 5), 
				HasShadow = false,
				Content = new Label { FormattedText = VoteCountText, }
				
			};
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
			var CuisineAndDistanceGrid = new Grid {
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Auto) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Auto) },
				}
			};
		
			DisplayPlace = place;
			this.Appearing += DoLoadPage;

			var saveBtn = new RayvButton ("Save"){ HorizontalOptions = LayoutOptions.FillAndExpand, };
			saveBtn.OnClick += DoSave;
			var cancelBtn = new RayvButton ("Cancel"){ HorizontalOptions = LayoutOptions.FillAndExpand, };
			cancelBtn.OnClick += async (s, e) => {
				if (await DisplayAlert ("Are you sure?", "Your changes will be lost", "Yes", "No"))
					await Navigation.PushModalAsync (
						new RayvNav (new MainMenu ()));
			};
			SaveFrame = new Frame {
				OutlineColor = Color.Transparent,
				Padding = 2,
				Content = new StackLayout {
					Children = { saveBtn, cancelBtn },
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.FillAndExpand,
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
			//MainGrid.Children.Add (Img, 0, 2, 0, IMAGE_HEIGHT);
			Place_name = new Label ();
//			Place_name.FontAttributes = FontAttributes.Bold;
//			Place_name.FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label));
//			Place_name.XAlign = TextAlignment.Center;
			CuisineEd = new LabelWide { };
			distance = new LabelWide {
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.End,
			};
			var DirectionsImgBtn = new TopRowBtn () { 
				Source = settings.DevicifyFilename ("Icon default directions1.png"),
				OnClick = DoDirections
			};
			CuisineAndDistanceGrid.Children.Add (CuisineEd, 0, 0);
			CuisineAndDistanceGrid.Children.Add (DirectionsImgBtn, 1, 0);
			CuisineAndDistanceGrid.Children.Add (distance, 2, 0);

			Address = new Label {
				TextColor = Color.FromHex ("707070"),
				FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
			};
			WebImgBtn = new TopRowBtn { 
				Source = settings.DevicifyFilename ("Icon default Website.png"),
				OnClick = GotoWebPage
			};
			TelImgBtn = new TopRowBtn {
				Source = settings.DevicifyFilename ("Icon default Phone.png"),
				OnClick = DoMakeCall,
			};
			var voteIconName = "Like.png";
			if (DisplayPlace.vote.vote == VoteValue.Disliked)
				voteIconName = "Dislike.png";
			if (DisplayPlace.vote.vote == VoteValue.Untried)
				voteIconName = "Wish1.png";
			VoteImgBtn = new TopRowBtn () {
				Source = settings.DevicifyFilename (voteIconName),
				OnClick = DoVote,
			};
			TopRow.Children.Add (TelImgBtn);
			TopRow.Children.Add (VoteImgBtn);
			TopRow.Children.Add (DirectionsImgBtn);
			TopRow.Children.Add (WebImgBtn);

			Comment = new LabelWithImageButton {
				Source = settings.DevicifyFilename ("187-pencil@2x.png"),
				OnClick = DoClickComment,
				FontAttributes = FontAttributes.Italic,
			};

			var VoteCountLbl = GetVoteCountText ();

			Label DraftText = new Label {
				Text = "DRAFT",
				TextColor = Color.Red,
				FontAttributes = FontAttributes.Bold,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
				XAlign = TextAlignment.Center,
				IsVisible = false,
			};
			if (isDraft) {
				DraftText.IsVisible = true;
			} else
				LoadPage (DisplayPlace);
			var styleGrid = new Grid {
				Padding = new Thickness (5, 20, 5, 5),
				RowSpacing = 20,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (100) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
//					new RowDefinition { Height = new GridLength (2) },
				}
			};
			styleGrid.Children.Add (new Label{ FontAttributes = FontAttributes.Bold, Text = "Cuisine" }, 0, 0); 
			styleGrid.Children.Add (new Label{ FontAttributes = FontAttributes.Bold, Text = "Style" }, 0, 1); 
			styleGrid.Children.Add (new Label{ FontAttributes = FontAttributes.Bold, Text = "Meal" }, 0, 2); 
			styleGrid.Children.Add (new Label{ Text = DisplayPlace.vote.cuisineName }, 1, 0); 
			styleGrid.Children.Add (new Label{ Text = DisplayPlace.vote.style.ToString () }, 1, 1); 
			styleGrid.Children.Add (new Label{ Text = DisplayPlace.vote.kind.ToString () }, 1, 2); 
//			styleGrid.Children.Add (new Frame{ HasShadow = false, OutlineColor = Color.Gray, HeightRequest = 2 }, 0, 2, 3, 4); 
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
						Img,
						TopRow,
						styleGrid,
						Comment,
						new Frame{ HasShadow = false, OutlineColor = settings.ColorMidGray, Padding = 0, HeightRequest = 1 }, 
						new Label { Text = "Friend Votes", FontAttributes = FontAttributes.Bold }, 
						VoteCountLbl,
						new Label { Text = "Comments", FontAttributes = FontAttributes.Bold }, 
						GetFriendsComments (),
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
