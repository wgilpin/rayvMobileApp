using System;
using Xamarin.Forms;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;

namespace RayvMobileApp.iOS
{
	public class EditPage : ContentPage
	{
		Entry Place_name;
		Image Img;
		Picker Category;
		ButtonWide VoteLike;
		ButtonWide VoteDislike;
		ButtonWide VoteWishlist;
		Button NewImgBtn;
		ButtonWide SaveBtn;
		Entry PhoneNo;
		Entry WebSite;
		Entry Address;
		Image ImgRotL;
		Image ImgRotR;
		Place EditPlace;
		Entry Comment;

		public EditPage (Place place) : this ()
		{
			EditPlace = place;
			if (EditPlace.img.Length > 0) {
				Img.Source = ImageSource.FromUri (new Uri (EditPlace.img));
			}
			Img.HorizontalOptions = LayoutOptions.CenterAndExpand;
			Img.VerticalOptions = LayoutOptions.Start;
			Img.Aspect = Aspect.AspectFill;
			Place_name.Text = EditPlace.place_name;
			Category.SelectedIndex = Category.Items.IndexOf (EditPlace.category);
			Address.Text = EditPlace.address;

			WebSite.Text = EditPlace.website;
			PhoneNo.Text = EditPlace.telephone;
			switch (EditPlace.vote) {
			case "-1":
				SetVoteButton (VoteDislike);
				break;
			case "1":
				SetVoteButton (VoteLike);
				break;
			default:
				EditPlace.vote = "0";
				SetVoteButton (VoteWishlist);
				break;
			}
			Comment.Text = EditPlace.descr;
		}

		public EditPage ()
		{

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

			Img = new Image ();
			try {
				Img.HorizontalOptions = LayoutOptions.CenterAndExpand;
				Img.VerticalOptions = LayoutOptions.Start;
				Img.Aspect = Aspect.AspectFill;
			} catch {
				Img.Source = null;
			}
			MainGrid.Children.Add (Img, 0, 3, 0, 5);
			ImgRotL = new Image {
				Source = ImageSource.FromFile ("left32x32.png"),
				HeightRequest = 32,
			};
			ImgRotR = new Image {
				Source = ImageSource.FromFile ("right32x32.png"),
				HeightRequest = 32,
			};
			NewImgBtn = new Button {
				BackgroundColor = Color.FromHex ("#444111111"),
				TextColor = Color.Black,
				Text = "Change Image",
			};
			MainGrid.Children.Add (ImgRotL, 0, 5);
			MainGrid.Children.Add (ImgRotR, 2, 5);
			MainGrid.Children.Add (NewImgBtn, 1, 5);


			Place_name = new Entry {
				Text = "",
			};
			MainGrid.Children.Add (Place_name, 0, 3, 6, 7);

			Category = new Picker {
				Title = "Cuisine",
			};
			foreach (string cat in Persist.Instance.Categories) {
				Category.Items.Add (cat);
			}


			MainGrid.Children.Add (Category, 0, 3, 7, 8);
			Address = new Entry {
				Text = "",
			};
			MainGrid.Children.Add (Address, 0, 3, 8, 9);
			WebSite = new Entry {
				Placeholder = "Website",
			};
			MainGrid.Children.Add (WebSite, 0, 3, 9, 10);
			PhoneNo = new Entry {
				Placeholder = "Phone",
			};
			MainGrid.Children.Add (PhoneNo, 0, 3, 10, 11);
			VoteLike = new ButtonWide {
				Text = "Like",
			};
			VoteLike.Clicked += (object sender, EventArgs e) => {
				EditPlace.vote = "1";
				SetVoteButton (VoteLike);
			};
			VoteDislike = new ButtonWide {
				Text = "Dislike",
			};
			VoteDislike.Clicked += (object sender, EventArgs e) => {
				EditPlace.vote = "-1";
				SetVoteButton (VoteDislike);
			};
			VoteWishlist = new ButtonWide {
				Text = "Wish",
			};
			VoteWishlist.Clicked += (object sender, EventArgs e) => {
				EditPlace.vote = "0";
				SetVoteButton (VoteWishlist);
			};

			MainGrid.Children.Add (VoteLike, 0, 11);
			MainGrid.Children.Add (VoteWishlist, 1, 11);
			MainGrid.Children.Add (VoteDislike, 2, 11);

			Comment = new Entry {
				Placeholder = "Comment",
			};
			MainGrid.Children.Add (Comment, 0, 3, 12, 13);
			SaveBtn = new ButtonWide {
				Text = "Save",
				BackgroundColor = Color.Blue,
				TextColor = Color.White,
			};
			SaveBtn.Clicked += DoSave;
			MainGrid.Children.Add (SaveBtn, 0, 3, 13, 14);
		
			this.Content = new ScrollView {
				Content = MainGrid,
			};
		}

		public EditPage (Position position, String address) : this ()
		{
			EditPlace = new Place ();
			EditPlace.lat = position.Latitude;
			EditPlace.lng = position.Longitude;
			EditPlace.address = address;
			Address.Text = address;
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

		async private void DoSave (object sender, EventArgs e)
		{
			lock (restConnection.Instance.Lock) {
				Dictionary<string, string> parameters = new Dictionary<string, string> ();
				if (Category.SelectedIndex == -1) {
					DisplayAlert ("Warning", "You must pick a cuisine", "OK");
					return;
				}
				parameters ["key"] = EditPlace.key;
				parameters ["lat"] = EditPlace.lat.ToString ();
				parameters ["lng"] = EditPlace.lng.ToString ();
				parameters ["addr"] = Address.Text;
				parameters ["place_name"] = Place_name.Text;
				parameters ["myComment"] = Comment.Text;
				parameters ["category"] = Category.Items [Category.SelectedIndex];
				parameters ["descr"] = "";
				switch (EditPlace.vote) {
				case "-1":
					parameters ["vote"] = "dislike";
					break;
				case "1":
					parameters ["voteScore"] = "like";
					break;
				default:
					parameters ["untried"] = "true";
					break;
				}
				try {
					string result = restConnection.Instance.post ("/item", parameters);
					//			JObject obj = JObject.Parse (result);
					Place place = JsonConvert.DeserializeObject<Place> (result);
					Console.WriteLine ("DoSave: read distance as {0}", place.distance);
					Persist.Instance.UpdatePlace (place);
					Console.WriteLine ("Saved");
					this.Navigation.PopToRootAsync ();
				} catch (Exception ex) {
					Console.WriteLine ("EditPage.DoSave: Exception {0}", ex);
					DisplayAlert ("Error", "Save Failed", "OK");
				}
			}

		}
	}
}

