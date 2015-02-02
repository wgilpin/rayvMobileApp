﻿using System;
using Xamarin.Forms;
using Foundation;
using UIKit;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
		#region Fields

		Place DisplayPlace;
		Label Place_name;
		Image Img;
		Label Category;
		ButtonWide VoteLike;
		ButtonWide VoteDislike;
		Button VoteWishlist;
		ButtonWide CallBtn;
		ButtonWide WebBtn;
		Label distance;
		Label Address;
		Label descr;

		#endregion

		#region Logic

		void LoadPage (string key)
		{
			foreach (Place p in Persist.Instance.Places) {
				if (p.key == key) {
					DisplayPlace = p;
					break;
				}
			}
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
			descr.Text = DisplayPlace.descr;
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
				VoteDislike.BackgroundColor = Color.Olive;
				VoteDislike.TextColor = Color.White;
				break;
			case "1":
				VoteLike.BackgroundColor = Color.Olive;
				VoteLike.TextColor = Color.White;
				break;
			default:
				VoteWishlist.BackgroundColor = Color.Olive;
				VoteWishlist.TextColor = Color.White;
				break;
			}
		}

		#endregion

		#region Events

		public void DoLoadPage (object sender, EventArgs e)
		{
			Console.WriteLine ("DetailPage: Pre-Appearing distance is {0}", DisplayPlace.distance);
			LoadPage (DisplayPlace.key);
			Console.WriteLine ("DetailPage: Post-Appearing distance is {0}", DisplayPlace.distance);
		}

		public void GotoWebPage (object sender, EventArgs e)
		{
			Debug.WriteLine ("DetailPage.GotoWebPage: Push WebPage");
			Navigation.PushAsync (new WebPage (
				DisplayPlace.place_name,
				DisplayPlace.website));
		}

		void DoMakeCall (object sender, EventArgs e)
		{
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
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },

					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
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
			MainGrid.Children.Add (Place_name, 0, 3, IMAGE_HEIGHT, IMAGE_HEIGHT + 1);
			Category = new LabelWide {
				TextColor = Color.Red,
			};
			MainGrid.Children.Add (Category, 0, 3, IMAGE_HEIGHT + 1, IMAGE_HEIGHT + 2);
			Address = new LabelWide ();
			MainGrid.Children.Add (Address, 0, 3, IMAGE_HEIGHT + 2, IMAGE_HEIGHT + 3);
			descr = new LabelWide ();
			distance = new LabelWide ();
			WebBtn = new ButtonWide ();
			MainGrid.Children.Add (WebBtn, 0, 3, IMAGE_HEIGHT + 3, IMAGE_HEIGHT + 4);

			CallBtn = new ButtonWide ();
			CallBtn.Clicked += DoMakeCall;
			MainGrid.Children.Add (CallBtn, 0, 3, IMAGE_HEIGHT + 4, IMAGE_HEIGHT + 5);
			VoteLike = new ButtonWide {
				Text = "Like",
			};
			VoteLike.Clicked += (object sender, EventArgs e) => DisplayAlert ("Voting", "Not Implemented", "OK");
			VoteDislike = new ButtonWide {
				Text = "Dislike",
			};
			VoteWishlist = new ButtonWide {
				Text = "Wish",
			};
			MainGrid.Children.Add (VoteLike, 0, IMAGE_HEIGHT + 6);
			MainGrid.Children.Add (VoteWishlist, 1, IMAGE_HEIGHT + 6);
			MainGrid.Children.Add (VoteDislike, 2, IMAGE_HEIGHT + 6);
//			Grid voteGrid = new Grid {
//				RowDefinitions = {
//					new RowDefinition { Height = GridLength.Auto },
//				},
//				ColumnDefinitions = {
//					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
//					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
//					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
//				}
//			};

//			voteGrid.Children.Add (VoteLike, 0, 0);
//			voteGrid.Children.Add (VoteWishlist, 1, 0);
//			voteGrid.Children.Add (VoteDislike, 2, 0);

			LoadPage (DisplayPlace.key);

			this.Content = new ScrollView {
				Content = new StackLayout {
					Children = {
						Img,
						MainGrid,
					}
				},
			};
			ToolbarItems.Add (new ToolbarItem {
				Text = "Map",
				Icon = "icon-map.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					Debug.WriteLine ("detailPage Map Toolbar: Push MapPage");
					Navigation.PushAsync (new MapPage (DisplayPlace));
				})
			});
			ToolbarItems.Add (new ToolbarItem {
				Text = "Edit",
				Icon = "187-pencil@2x.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => { 
					Debug.WriteLine ("AddResultsPage.DoEdit: Push EditPage");
					Navigation.PushAsync (new EditPage (DisplayPlace));
				})
			});

		}
	}
}
