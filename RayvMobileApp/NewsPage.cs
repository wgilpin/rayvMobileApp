﻿using System;
using Xamarin.Forms;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Xamarin;
using System.Net;

namespace RayvMobileApp
{
	enum NewsFilterKind: short
	{
		Good,
		All,
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

		ListView list;
		DateTime? LastUpdate;
		bool Clicked;
		Button MoreBtn;
		int ShowRows;
		StackLayout Toolbar;
		ActivityIndicator Spinner;

		NewsFilterKind Filter = NewsFilterKind.All;

		#endregion

		public NewsPage ()
		{
			Title = "Activity";
			Insights.Track ("News Page");
			list = new ListView () {
				RowHeight = ROW_HEIGHT,
				BackgroundColor = Color.White,

				ItemTemplate = new DataTemplate (() => {
					Grid grid = new Grid {
						VerticalOptions = LayoutOptions.FillAndExpand,
						RowDefinitions = {
							new RowDefinition { Height = new GridLength (ROW1, GridUnitType.Absolute)  },
							new RowDefinition { Height = new GridLength (ROW2, GridUnitType.Absolute)  },
							new RowDefinition { Height = new GridLength (ROW3, GridUnitType.Absolute)  },
							new RowDefinition { Height = new GridLength (ROW4, GridUnitType.Absolute)  },
						},
						ColumnDefinitions = {
							new ColumnDefinition { Width = new GridLength (31, GridUnitType.Absolute) },
							new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
							new ColumnDefinition { Width = new GridLength (NEWS_IMAGE_SIZE + 20, GridUnitType.Absolute) },
						}
					};


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
					LetterBtn.SetBinding (
						Button.TextProperty, 
						new Binding ("voter", converter: new VoterToFirstLetterConverter ()));
					LetterBtn.SetBinding (
						Button.BackgroundColorProperty, 
						new Binding ("voter", converter: new VoterToRandomColorConverter ()));

					Label CommenterLbl = new Label {
						FontAttributes = FontAttributes.Bold,
						TranslationY = 2,
						TextColor = Color.FromHex ("#444444"),
					};
					CommenterLbl.SetBinding (Label.TextProperty, "VoterName");

					Label TimeLbl = new Label {
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
						FontAttributes = FontAttributes.Italic,
						//TextColor = Color.FromHex ("#606060"),
						HorizontalOptions = LayoutOptions.End,
					};
					TimeLbl.SetBinding (
						Label.TextProperty, 
						new Binding ("when", converter: new WhenToPrettyStringConverter ()));

					Label VoteLbl = new Label {
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
						FontAttributes = FontAttributes.Italic,
						TextColor = Color.FromHex ("#444444"),
						HorizontalOptions = LayoutOptions.Start,
						TranslationY = 4,
					};
					VoteLbl.SetBinding (
						Label.TextProperty, 
						new Binding ("vote", converter: new VoteToVerbConverter ()));


					Image VoteImg = new Image { 
						Aspect = Aspect.AspectFit,
						WidthRequest = NEWS_ICON_SIZE, 
						HeightRequest = NEWS_ICON_SIZE,
						TranslationX = 0,
					};
					VoteImg.SetBinding (Image.SourceProperty, "GetIconName");

					Image PlaceImg = new Image { 
						Aspect = Aspect.AspectFill,
						WidthRequest = NEWS_IMAGE_SIZE, 
						HeightRequest = ROW_HEIGHT,
						TranslationX = 0,
						VerticalOptions = LayoutOptions.Start,
						Opacity = 0.45,
					};
					PlaceImg.SetBinding (Image.SourceProperty, "PlaceImage");

					Label PlaceLbl = new Label {
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						LineBreakMode = LineBreakMode.TailTruncation,
					};
					PlaceLbl.SetBinding (Label.TextProperty, "place_name");

					Label CommentLbl = new Label {
						FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
						FontAttributes = FontAttributes.Italic,
						BackgroundColor = Color.White,
						TextColor = Color.FromHex ("#606060"),
						HorizontalOptions = LayoutOptions.Start,
						LineBreakMode = LineBreakMode.WordWrap,
					};
					CommentLbl.SetBinding (
						Label.TextProperty, 
						new Binding ("comment", converter: new CommentToPrettyStringConverter ()));


					//TODO: Get address from vote
					Label AddressLbl = new Label {
						FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
						LineBreakMode = LineBreakMode.TailTruncation,
						TextColor = Color.FromHex ("#808080"),
					};
					AddressLbl.SetBinding (
						Label.TextProperty, 
						new Binding ("key", converter: new KeyToShortAddressConverter ()));
					
					grid.Children.Add (PlaceLbl, 1, 3, 1, 2);
					grid.Children.Add (TimeLbl, 1, 3, 0, 1);
					grid.Children.Add (new StackLayout{ Children = { LetterBtn, }, Padding = 1, }, 0, 1, 0, 2);
					grid.Children.Add (new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							CommenterLbl,
							VoteLbl,
						}
					}, 1, 2, 0, 1);
					grid.Children.Add (PlaceImg, 2, 3, 0, 4);
					grid.Children.Add (AddressLbl, 1, 2, 2, 3);
					grid.Children.Add (CommentLbl, 1, 2, 3, 4);

					return new ViewCell {
						View = grid,
					};
				})
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
			list.ItemTapped += DoListItemTap;
		


			this.Appearing += CheckForUpdates;
			SetSource ();
		}

		#region Events

		async void DoListItemTap (object sender, ItemTappedEventArgs e)
		{
			if (Clicked) {
				Console.WriteLine ("Click ignored");
				return;
			}
			//				Clicked = true;
			Debug.WriteLine ("NewsPage.ItemTapped: Push DetailPage");
			Place p = Persist.Instance.GetPlace ((e.Item as Vote).key);
			string action = await DisplayActionSheet (
				                p.place_name, 
				                "Cancel",
				                null, 
				                "Show Detail", 
				                "Like", 
				                "Dislike",
				                "Add to Wishlist");
			string errorMsg = "";
			switch (action) {
				case "Show Detail":
					this.Navigation.PushAsync (new DetailPage (p));
					break;
				case "Like":
					p.vote.vote = VoteValue.Liked;
					p.SaveVote (out errorMsg);
					break;
				case "Dislike":
					p.vote.vote = VoteValue.Disliked;
					p.SaveVote (out errorMsg);
					break;
				case "Add to Wishlist":
					p.vote.vote = VoteValue.Untried;
					p.SaveVote (out errorMsg);
					break;
			}
			if (errorMsg.Length > 0) {
				await DisplayAlert ("Error Saving", errorMsg, "OK");
				Console.WriteLine ("NewsPage.DoListItemTap Error: {0}", errorMsg);
			}
		}

		void DoShowMore (object sender, EventArgs e)
		{
			Console.WriteLine ("NewsPage.DoShowMore");
			ShowRows += PAGE_SIZE;
			SetSource ();
		}

		#endregion

		#region Logic

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
								Navigation.PushModalAsync (new LoginPage ());
							}, 
							onSucceed: () => {
								SetSource ();
								LastUpdate = DateTime.UtcNow;
								Spinner.IsRunning = false;
								Spinner.IsVisible = false;
								Console.WriteLine ("Spin down");
							},
							since: LastUpdate, 
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
						            && v.vote == VoteValue.Liked
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
					list.ItemsSource = null;
					list.ItemsSource = News.Take (ShowRows);
				});


				Device.BeginInvokeOnMainThread (() => {
					MoreBtn.IsVisible = News.Count > ShowRows;
				});
			}


		}

		#endregion
	}
}
