using System;
using Xamarin.Forms;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace RayvMobileApp.iOS
{
	public class NewsPage: ContentPage
	{
		const int NEWS_IMAGE_SIZE = 60;
		const int NEWS_ICON_SIZE = 20;
		const int ROW_HEIGHT = 79;

		ListView list;
		DateTime? LastUpdate;

		public NewsPage ()
		{
			list = new ListView () {
				RowHeight = ROW_HEIGHT,

				ItemTemplate = new DataTemplate (() => {
					Grid grid = new Grid {
						VerticalOptions = LayoutOptions.FillAndExpand,
						RowDefinitions = {
							new RowDefinition { Height = new GridLength (21, GridUnitType.Absolute)  },
							new RowDefinition { Height = new GridLength (23, GridUnitType.Absolute)  },
							new RowDefinition { Height = new GridLength (23, GridUnitType.Absolute)  },
						},
						ColumnDefinitions = {
							new ColumnDefinition { Width = new GridLength (30, GridUnitType.Absolute) },
							new ColumnDefinition { Width = new GridLength (20, GridUnitType.Absolute) },
							new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
							new ColumnDefinition { Width = new GridLength (NEWS_IMAGE_SIZE + 10, GridUnitType.Absolute) },
						}
					};

					Label CommenterLbl = new Label {
						FontAttributes = FontAttributes.Bold,
						TextColor = Color.Blue,
					};
					CommenterLbl.SetBinding (Label.TextProperty, "VoterName");

					Label TimeLbl = new Label {
						FontAttributes = FontAttributes.Italic,
						Font = Font.SystemFontOfSize (NamedSize.Small),
						TextColor = Color.FromHex ("#606060"),
					};
					TimeLbl.SetBinding (Label.TextProperty, "PrettyHowLongAgo");

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
					};
					PlaceImg.SetBinding (Image.SourceProperty, "PlaceImage");

					Label PlaceLbl = new Label {
						FontAttributes = FontAttributes.Bold,
					};
					PlaceLbl.SetBinding (Label.TextProperty, "place_name");

					Label CommentLbl = new Label {
						FontAttributes = FontAttributes.Italic,
						BackgroundColor = Color.White,
						TextColor = Color.FromHex ("#606060"),
						HorizontalOptions = LayoutOptions.Start,
						LineBreakMode = LineBreakMode.TailTruncation,
					};
					CommentLbl.SetBinding (Label.TextProperty, "PrettyComment");

					grid.Children.Add (CommenterLbl, 0, 2, 0, 1);
					grid.Children.Add (TimeLbl, 2, 3, 0, 1);
					grid.Children.Add (VoteImg, 0, 1, 1, 2);
					grid.Children.Add (PlaceLbl, 1, 3, 1, 2);
					grid.Children.Add (PlaceImg, 3, 4, 0, 4);
					grid.Children.Add (CommentLbl, 0, 4, 2, 3);

					return new ViewCell {
						View = grid,
					};
				})
			};
			StackLayout tools = new toolbar (this);
			SetSource ();
			this.Content = new StackLayout {
				Children = {
					new ScrollView {
						Content = list,
					},
					tools
				}
			};
			list.ItemTapped += (object sender, ItemTappedEventArgs e) => {
				Debug.WriteLine ("NewsPage.ItemTapped: Push DetailPage");
				Place p = Persist.Instance.GetPlace ((e.Item as Vote).key);
				this.Navigation.PushAsync (new DetailPage (p));
			};
			this.Appearing += CheckForUpdates;
		}

		void CheckForUpdates (object sender, EventArgs e)
		{
			if (list != null) {
				Console.WriteLine ("NewsPage.CheckForUpdates");
				Persist.Instance.GetUserData (this, LastUpdate);
				SetSource ();
				LastUpdate = DateTime.Now;
			}

		}

		void SetSource ()
		{
			Console.WriteLine ("NewsPage.SetSource");
			lock (Persist.Instance.Lock) {
				list.ItemsSource = null;
				Persist.Instance.Votes.Sort (); 
				string MyStringId = Persist.Instance.MyId.ToString ();
				List<Vote> News = (from v in Persist.Instance.Votes
				                   where v.voter != MyStringId
				                       && v.vote == 1
				                   select v)
					.OrderByDescending (x => x.when)
					.ToList ();
				list.ItemsSource = News.Take (20);
			}
		}
	}
}
