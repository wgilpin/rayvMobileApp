﻿using System;
using Xamarin.Forms;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{
	public class NewsPage: ContentPage
	{
		const int NEWS_IMAGE_SIZE = 60;
		const int NEWS_ICON_SIZE = 20;
		const int ROW_HEIGHT = 85;

		public NewsPage ()
		{
			ListView list = new ListView () {
				RowHeight = ROW_HEIGHT,

				ItemTemplate = new DataTemplate (() => {
					Grid grid = new Grid {
						VerticalOptions = LayoutOptions.FillAndExpand,
						RowDefinitions = {
							new RowDefinition { Height = new GridLength (23, GridUnitType.Absolute)  },
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
					};
					CommenterLbl.SetBinding (Label.TextProperty, "VoterName");

					Label TimeLbl = new Label {
						FontAttributes = FontAttributes.Italic
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
						Aspect = Aspect.AspectFit,
						WidthRequest = NEWS_IMAGE_SIZE, 
						HeightRequest = NEWS_IMAGE_SIZE,
						TranslationX = 0,
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
			lock (Persist.Instance.Lock) {
				Persist.Instance.Votes.Sort ();
				list.ItemsSource = Persist.Instance.Votes;
			}
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
		}
	}
}
