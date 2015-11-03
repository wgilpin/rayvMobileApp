using System;
using Xamarin.Forms;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Xamarin;
using System.Net;
using System.Text.RegularExpressions;

namespace RayvMobileApp
{
	enum NewsFilterKind: short
	{
		Good,
		All,
	}

	public class ListItem : Frame
	{
		public string Key { get; set; }

		public ListItem () : base ()
		{
		}
	}

	public class NewsPage: ContentPage
	{
		#region fields

		const int NEWS_IMAGE_SIZE = 60;
		const int NEWS_ICON_SIZE = 20;
		const int ROW1 = 20;
		const int ROW2 = 20;
		const int ROW3 = 15;
		const int ROW4 = 45;
		const int ROW_HEIGHT = ROW1 + ROW2 + ROW3 + ROW4 + 13;
		const int PAGE_SIZE = 10;
		int buttonSize = Device.OnPlatform (30, 50, 30);

		StackLayout list;
		DateTime? LastUpdate;
		bool Clicked;
		Button MoreBtn;
		int ShowRows;
		BottomToolbar Toolbar;
		ActivityIndicator Spinner;
		NewsFilterKind Filter = NewsFilterKind.All;

		#endregion

		#region Content


		Grid CreateGrid ()
		{
			return new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
					new RowDefinition { Height = new GridLength (ROW2, GridUnitType.Absolute)  },
					new RowDefinition { Height = new GridLength (ROW3, GridUnitType.Absolute)  },
					new RowDefinition { Height = new GridLength (ROW4, GridUnitType.Absolute)  },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (buttonSize + 1, GridUnitType.Absolute) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (NEWS_IMAGE_SIZE + 40, GridUnitType.Absolute) },
				}
			};
		}

		void AddName (Grid grid, string name, string comment, StarEditor stars = null)
		{
			var letterFontSize = Device.OnPlatform (
				                     settings.FontSizeButtonLarge,
				                     settings.FontSizeButtonMedium,
				                     settings.FontSizeButtonLarge);
			Button LetterBtn = new Button {
				WidthRequest = buttonSize,
				HeightRequest = buttonSize,
				FontSize = letterFontSize,
				BorderRadius = buttonSize / 2,
				BackgroundColor = Vote.RandomColor (name),
				Text = Vote.FirstLetter (name),
				TextColor = Color.White,
				VerticalOptions = LayoutOptions.Start,
				TranslationX = 5
			};

			Label nameLbl = new Label {
				FontAttributes = FontAttributes.Bold,
				TranslationY = 4,
				TextColor = Color.FromHex ("#444444"),
				Text = name.Substring (1, name.Length - 1),
				LineBreakMode = LineBreakMode.TailTruncation
			};
			Label CommentLbl = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
				FontAttributes = FontAttributes.Italic,
				TextColor = Color.FromHex ("#444444"),
				HorizontalOptions = LayoutOptions.Start,
				TranslationY = 4,
				Text = comment,
			};
			grid.Children.Add (new StackLayout { 
				Children = { 
					LetterBtn, 
				}, 
				Padding = 1, 
			}, 0, 1, 0, 2);
			var inner = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Children = {
					nameLbl,
					CommentLbl,
				}
			};
			if (stars != null) {
				inner.Children.Add (stars);
			}
			grid.Children.Add (inner, 1, 2, 0, 1);
		}

		ListItem CreateInviteItem (string name,
		                           string action,
		                           string btn1text,
		                           EventHandler btn1click,
		                           string btn2text = null,
		                           EventHandler btn2click = null)
		{
			Grid grid = CreateGrid ();
			grid.RowDefinitions [2].Height = Device.OnPlatform (30, new GridLength (20, GridUnitType.Auto), 20);
			AddName (grid, name, "");
			var btn1 = new Button {
				BackgroundColor = settings.BaseColor,
				TextColor = settings.BaseTextColor,
				Text = btn1text,
				FontAttributes = FontAttributes.Bold,
				CommandParameter = name
			};
			btn1.Clicked += btn1click;
			Button btn2 = new Button {
				BackgroundColor = ColorUtil.Lighter (settings.BaseColor),
				Text = btn2text,
				TextColor = settings.InvertTextColor,
				CommandParameter = name
			};
			btn2.Clicked += btn2click;
			Label ActionLbl = new Label {
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.CharacterWrap,
				Text = action,
				TextColor = settings.InvertTextColor
			};

			grid.Children.Add (ActionLbl, 1, 3, 1, 2);
			if (string.IsNullOrEmpty (btn2text)) {
				// one btn
				grid.Children.Add (btn1, 1, 3, 2, 3);
			} else {
				// two btns
				grid.Children.Add (btn1, 1, 2, 2, 3);
				grid.Children.Add (btn2, 2, 3, 2, 3);
			}
			grid.RowDefinitions [3].Height = 0;
			return new ListItem {
				HasShadow = false,
				BackgroundColor = Color.White,
				OutlineColor = Color.White,
				Content = grid,
				Padding = 0,
			};
		}

		Frame CreateNewsItem (Vote vote)
		{
			Grid grid = CreateGrid ();
			var stars = new StarEditor (false) { 
				Height = 10, 
				Vote = vote.vote, 
				ReadOnly = true, 
				TranslationY = 4 
			};
			AddName (grid, vote.VoterName, comment: "", stars: stars);
			Label TimeLbl = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.End,
				Text = vote.PrettyHowLongAgo,
			};

			Image PlaceImg = new Image { 
				Aspect = Aspect.AspectFill,
				WidthRequest = NEWS_IMAGE_SIZE, 
				HeightRequest = ROW_HEIGHT,
				TranslationX = 0,
				VerticalOptions = LayoutOptions.Start,
				Opacity = 0.45,
				Source = vote.PlaceImage
			};

			Label PlaceLbl = new Label {
				FontAttributes = FontAttributes.Bold,
				HorizontalOptions = LayoutOptions.Center,
				LineBreakMode = LineBreakMode.TailTruncation,

				Text = vote.place_name
			};

			Label CommentLbl = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
				FontAttributes = FontAttributes.Italic,
				BackgroundColor = Color.White,
				TextColor = Color.FromHex ("#606060"),
				HorizontalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
				Text = vote.PrettyComment
			};

			//TODO: Get address from vote
			Label AddressLbl = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
				LineBreakMode = LineBreakMode.TailTruncation,
				TextColor = Color.FromHex ("#808080"),
			};
			// number then anything
			string addressPattern = @"^(\d+[-\d+]* )(.*)";
			MatchCollection matches = Regex.Matches (vote.Place.address, addressPattern);
			AddressLbl.Text = matches.Count < 1 ? 
				vote.Place.address : 
				AddressLbl.Text = matches [0].Groups [2].ToString ();

			grid.Children.Add (PlaceLbl, 1, 3, 1, 2);
			grid.Children.Add (TimeLbl, 1, 3, 0, 1);
			grid.Children.Add (PlaceImg, 2, 3, 0, 4);
			grid.Children.Add (AddressLbl, 1, 2, 2, 3);
			grid.Children.Add (CommentLbl, 1, 2, 3, 4);
			//AddName (grid, vote.VoterName, vote.GetVoteAsString);

			return new ListItem {
				HasShadow = false,
				BackgroundColor = Color.White,
				OutlineColor = Color.White,
				Content = grid,
				Key = vote.key,
				Padding = 2,
			};
		}

		#endregion

		public NewsPage ()
		{
			Title = "Activity";
			Insights.Track ("News Page");
			BackgroundColor = Color.White;
			list = new StackLayout () {
				Padding = 5,
				Spacing = 10,
				BackgroundColor = Color.FromHex ("EEE"),

			};
			Spinner = new ActivityIndicator { Color = Color.Red, };
			Toolbar = new BottomToolbar (this, "news");
			ShowRows = PAGE_SIZE;
			MoreBtn = new RayvButton ("Show More...");
			MoreBtn.Clicked += DoShowMore;
			this.Content = new StackLayout {
				Children = {
					Spinner,
					new ScrollView {
						VerticalOptions = LayoutOptions.FillAndExpand,
						Content = new StackLayout {
							Children = {
								list,
								MoreBtn,
							}
						}
					},
					Toolbar
				}
			};
			Clicked = false;
			this.Appearing += CheckForUpdates;
		}

		#region Events

		async void DoListItemTap (object sender, EventArgs e)
		{
			if (Clicked) {
				Console.WriteLine ("Click ignored");
				return;
			}
			//				Clicked = true;
			Debug.WriteLine ("NewsPage.ItemTapped: Push DetailPage");
			Place p = Persist.Instance.GetPlace ((sender as ListItem).Key);
			this.Navigation.PushAsync (new DetailPage (p));
		}

		void DoShowMore (object sender, EventArgs e)
		{
			Console.WriteLine ("NewsPage.DoShowMore");
			ShowRows += PAGE_SIZE;
			SetSource ();
		}

		#endregion

		#region Logic

		void DoAccept (object sender, EventArgs e)
		{
			string name = ((sender as Button).CommandParameter as string);
			var friendId = Persist.Instance.InviteNames.Where (kvp => kvp.Value == name).Select (kvp2 => kvp2.Key).FirstOrDefault ();
			Console.WriteLine ("DoAccept " + name);
			if (Invite.AcceptInvite (friendId)) {
				CheckForUpdates (sender, e);
				Toolbar.SetActivityIcon ();
			} else
				DisplayAlert ("Failed", "Unable to accept friend request", "OK");
		}

		void DoRemoveAccept (object sender, EventArgs e)
		{
			string name = ((sender as Button).CommandParameter as string);
			var friendId = Persist.Instance.InviteNames.Where (kvp => kvp.Value == name).Select (kvp2 => kvp2.Key).FirstOrDefault ();
			Console.WriteLine ("DoDismiss " + name);
			if (!Invite.DismissAcceptance (friendId)) {
				DisplayAlert ("Failed", "Unable to dismiss friend request", "OK");
			}
			CheckForUpdates (sender, e);
			Toolbar.SetActivityIcon ();
		}

		void DoReject (object sender, EventArgs e)
		{
			string name = ((sender as Button).CommandParameter as string);
			var friendId = Persist.Instance.InviteNames.Where (kvp => kvp.Value == name).Select (kvp2 => kvp2.Key).FirstOrDefault ();
			Console.WriteLine ("DoDismiss " + name);
			if (Invite.RejectInvite (friendId)) { 
				DisplayAlert ("Rejected", "Friend request rejected", "OK");
				CheckForUpdates (sender, e);
				Toolbar.SetActivityIcon ();
			} else
				DisplayAlert ("Failed", "Please try later", "OK");
		}


		void DoGotoFriend (object sender, EventArgs e)
		{
			Persist.Instance.GetUserData (
				onFail: () => {
					DisplayAlert ("Error", "Couldn't contact server", "OK");
				},
				onFailVersion: () => {
					Navigation.PushModalAsync (new LoginPage ());
				},
				onSucceed: () => {
					string name = ((sender as Button).CommandParameter as string);
					var friendId = Persist.Instance.Friends.
						Where (f => f.Value.Name == name).
						Select (f2 => f2.Key).
						FirstOrDefault ().
						ToString ();
					var listPage = new ListPage {
						FilterShowWho = friendId
					};
					Navigation.PushModalAsync (new RayvNav (listPage));
				},
				since: LastUpdate, 
				incremental: true);
			
		}


		void LoadList (IEnumerable<Vote> newsList)
		{
			list.Children.Clear ();
			foreach (var accept in Persist.Instance.Acceptances) {
				list.Children.Add (CreateInviteItem (accept.name,
				                                     "is now a friend!", 
				                                       $"See {accept.name}'s places", DoGotoFriend, 
				                                     "Dismiss", DoRemoveAccept));
			}
			foreach (var invIn in Persist.Instance.InvitationsIn) {
				list.Children.Add (CreateInviteItem (invIn.name,
				                                     "sent you a friend request!", 
				                                     "Accept", DoAccept, 
				                                     "Reject", DoReject));
			}

			var clickVote = new TapGestureRecognizer ();
			clickVote.Tapped += DoListItemTap;
			foreach (Vote v in newsList) {
				var view = CreateNewsItem (v);
				list.Children.Add (view);
				view.GestureRecognizers.Add (clickVote);
			}
		}

		void CheckForUpdates (object sender, EventArgs e)
		{
			Spinner.IsVisible = true;
			Spinner.IsRunning = true;
			Console.WriteLine ("Spin up");
			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
				Clicked = false;
				if (list != null) {
					Console.WriteLine ("NewsPage.CheckForUpdates");
					try {
						Persist.Instance.GetUserData (
							onFail: () => {
								Device.BeginInvokeOnMainThread (() => {
									Spinner.IsRunning = false;
									Spinner.IsVisible = false;
									SetSource ();
								});
							},
							onFailVersion: () => {
								Device.BeginInvokeOnMainThread (() => {
									Navigation.PushModalAsync (new LoginPage ());
								});
							},
							onSucceed: () => {
								Device.BeginInvokeOnMainThread (() => {
									SetSource ();
									LastUpdate = DateTime.UtcNow;
									Spinner.IsRunning = false;
									Spinner.IsVisible = false;
									Console.WriteLine ("Spin down");
								});
							},
							since: DateTime.Now - new TimeSpan (settings.NEWS_PAGE_TIMESPAN_DAYS, 0, 0, 0), 
							incremental: true);
					} catch (ProtocolViolationException) {
						DisplayAlert ("Server Error", "The app is designed for another version of the server", "OK");
					} catch (Exception ex) {
						Insights.Report (ex);
					}
				}
			})).Start ();

		}

		void SetSource ()
		{
			Console.WriteLine ("NewsPage.SetSource");
			lock (Persist.Instance.Lock) {
				Persist.Instance.Votes.Sort (); 
				string MyStringId = Persist.Instance.MyId.ToString ();
				List<Vote> News;
				switch (Filter) {

					case NewsFilterKind.Good:
						News = (from v in Persist.Instance.Votes
						        where v.voter != MyStringId
						            && v.vote > 3
						        select v)
						.OrderByDescending (x => x.when)
						.ToList ();
						break;
					default:
						News = (from v in Persist.Instance.Votes
						        where v.voter != MyStringId
						        select v)
						.OrderByDescending (x => x.when)
						.ToList ();
						break;
				}
				Device.BeginInvokeOnMainThread (() => {
					LoadList (News.Take (ShowRows));
				});


				Device.BeginInvokeOnMainThread (() => {
					MoreBtn.IsVisible = News.Count > ShowRows;
				});
			}


		}

		#endregion
	}
}
