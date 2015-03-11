using System;
using Xamarin.Forms;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;
using System.Diagnostics;
using Xamarin;
using System.Linq;

namespace RayvMobileApp.iOS
{
	public class EditPage : ContentPage
	{
		#region Fields

		Entry Place_name;
		//		Image Img;
		Picker Category;
		ButtonWide VoteLike;
		ButtonWide VoteDislike;
		ButtonWide VoteWishlist;
		ButtonWide SaveBtn;
		ButtonWide DeleteButton;
		Entry PhoneNo;
		Entry WebSite;
		Entry Address;
		Place EditPlace;
		Editor Comment;
		bool IsNew;
		bool Voted;

		// if adding a new place, on save we show the detaiPage, else it is just Pop
		private bool AddingNewPlace = false;

		#endregion

		#region Constructors

		public EditPage (Place place, bool addingNewPlace = false) : this ()
		{
			AddingNewPlace = addingNewPlace;
			DeleteButton.IsVisible = !addingNewPlace;
			IsNew = String.IsNullOrEmpty (place.category);

			EditPlace = place;
//			if (EditPlace.img.Length > 0) {
//				Img.Source = ImageSource.FromUri (new Uri (EditPlace.img));
//
//			}
//			Img.HorizontalOptions = LayoutOptions.CenterAndExpand;
//			Img.VerticalOptions = LayoutOptions.Start;
//			Img.Aspect = Aspect.AspectFill;
			Place_name.Text = EditPlace.place_name;
			Category.SelectedIndex = Category.Items.IndexOf (EditPlace.category);
			Address.Text = EditPlace.address;
			Address.IsEnabled = String.IsNullOrEmpty (EditPlace.address);
			Comment.Text = EditPlace.Comment (); 

			WebSite.Text = EditPlace.website;
			WebSite.IsEnabled = String.IsNullOrEmpty (EditPlace.website);
			PhoneNo.Text = EditPlace.telephone;
			PhoneNo.IsEnabled = String.IsNullOrEmpty (EditPlace.telephone);
			if (!IsNew) {
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
				Voted = true;
			}
		}

		public EditPage (bool addingNewPlace = false)
		{
			AddingNewPlace = addingNewPlace;
			IsNew = true;
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
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = new GridLength (60, GridUnitType.Absolute) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};

			int Row = 6;
//			Img = new Image ();
//			try {
//				Img.HorizontalOptions = LayoutOptions.CenterAndExpand;
//				Img.VerticalOptions = LayoutOptions.Start;
//				Img.Aspect = Aspect.AspectFill;
//			} catch {
//				Img.Source = null;
//			}
//			MainGrid.Children.Add (Img, 0, 3, 0, 5);
//			ImgRotL = new Image {
//				Source = ImageSource.FromFile ("left32x32.png"),
//				HeightRequest = 32,
//			};
//			ImgRotR = new Image {
//				Source = ImageSource.FromFile ("right32x32.png"),
//				HeightRequest = 32,
//			};
//			NewImgBtn = new Button {
//				BackgroundColor = Color.FromHex ("#444111111"),
//				TextColor = Color.Black,
//				Text = "Change Image",
//			};
//			MainGrid.Children.Add (ImgRotL, 0, 5);
//			MainGrid.Children.Add (ImgRotR, 2, 5);
//			MainGrid.Children.Add (NewImgBtn, 1, 5);


			Place_name = new Entry {
				Text = "",
				Placeholder = "Name of place",
			};
			MainGrid.Children.Add (Place_name, 0, 3, Row, Row + 1);
			Row++;
			Category = new Picker {
				Title = "Cuisine",
			};
			foreach (string cat in Persist.Instance.Categories) {
				Category.Items.Add (cat);
			}


			MainGrid.Children.Add (Category, 0, 3, Row, Row + 1);
			Row++;
			Address = new Entry {
				Text = "",
				Placeholder = "Address",
			};
			MainGrid.Children.Add (Address, 0, 3, Row, Row + 1);
			Row++;
			WebSite = new Entry {
				Placeholder = "Website",
			};
			MainGrid.Children.Add (WebSite, 0, 3, Row, Row + 1);
			Row++;
			PhoneNo = new Entry {
				Placeholder = "Phone",
			};
			MainGrid.Children.Add (PhoneNo, 0, 3, Row, Row + 1);
			Row++;
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

			MainGrid.Children.Add (VoteLike, 0, Row);
			MainGrid.Children.Add (VoteWishlist, 1, Row);
			MainGrid.Children.Add (VoteDislike, 2, Row);
			Row++;
			Comment = new Editor ();
			Comment.Keyboard = Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck);

			MainGrid.Children.Add (new Label { Text = "Comments" }, 0, 3, Row, Row + 1);
			Row++;
			MainGrid.Children.Add (Comment, 0, 3, Row, Row + 1);
			Row++;
			SaveBtn = new ButtonWide {
				Text = "Save",
				BackgroundColor = Color.Blue,
				TextColor = Color.White,
				FontAttributes = FontAttributes.Bold,
			};
			SaveBtn.Font = Font.SystemFontOfSize (NamedSize.Large);
			SaveBtn.Clicked += DoSave;
			MainGrid.Children.Add (SaveBtn, 0, 3, Row, Row + 1);
			Row++;
			DeleteButton = new ButtonWide {
				Text = "Remove from my lists",
				BackgroundColor = Color.Red,
				TextColor = Color.White,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.End,
				IsVisible = false,
			};
			DeleteButton.Font = Font.SystemFontOfSize (NamedSize.Large);
			DeleteButton.Clicked += DeletePlace;
			MainGrid.Children.Add (DeleteButton, 0, 3, Row, Row + 1);
			Row++;
			this.Content = new ScrollView {
				Content = MainGrid,
			};

			this.Appearing += (sender, e) => {
//				Img.HeightRequest = this.Width / 3;
//				Img.Aspect = Aspect.AspectFill;
			};
		}

		public EditPage (Position position, String address, bool addingNewPlace = false) : this ()
		{
			AddingNewPlace = addingNewPlace;
			DeleteButton.IsVisible = false;
			IsNew = true;
			EditPlace = new Place ();
			EditPlace.lat = position.Latitude;
			EditPlace.lng = position.Longitude;
			EditPlace.address = address;
			Address.Text = address;
		}

		#endregion

		#region Methods

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
			Voted = true;
		}

		async void DeletePlace (object sender, EventArgs e)
		{
			//delete the current item
			//is it in my list?
			String name = EditPlace.place_name;
			Vote vote = (from v in Persist.Instance.Votes
			             where v.key == EditPlace.key
			             select v).FirstOrDefault ();
			if (vote != null) {
				bool confirm_delete = await DisplayAlert (
					                      "Delete", "Remove this place from your list? This cannot be undone", "Yes", "No");
				if (confirm_delete) {
					string res = restConnection.Instance.post (
						             "api/delete",
						             new Dictionary<string, string> () {
							{ "key", EditPlace.key }
						});
					if (res != null) {
						Persist.Instance.Places.Remove (EditPlace);
						Insights.Track ("EditPage.DeletePlace", "Place", name);
					}
					await Navigation.PopToRootAsync ();
				}
			}
		}

		#endregion

		#region Events

		async private void DoSave (object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty (Place_name.Text)) {
				await DisplayAlert ("Warning", "You must name the place", "OK");
				Place_name.Focus ();
				return;
			}
			if (Category.SelectedIndex == -1) {
				await DisplayAlert ("Warning", "You must pick a cuisine", "OK");
				return;
			}
			if (!Voted) {
				await DisplayAlert ("Warning", "You must vote", "OK");
				return;
			}
			if (String.IsNullOrEmpty (Comment.Text)) {
				await DisplayAlert ("Warning", "Please add a comment - it's for other people to know what you thought", "OK");
				Comment.Focus ();
				return;
			}
			EditPlace.category = Category.Items [Category.SelectedIndex];
			EditPlace.setComment (Comment.Text);
			EditPlace.address = Address.Text;
			EditPlace.place_name = Place_name.Text;
			string Message = "";
			if (EditPlace.Save (out Message)) {
				Console.WriteLine ("Saved - PopToRootAsync");
				Insights.Track ("EditPage.DoSave", new Dictionary<string, string> {
					{ "PlaceName", EditPlace.place_name },
					{ "Lat", EditPlace.lat.ToString () },
					{ "Lng", EditPlace.lng.ToString () },
					{ "Vote", EditPlace.vote },
				});
				await DisplayAlert ("Saved", "Details Saved", "OK");
				#pragma warning disable 4014
				Persist.Instance.HaveAdded = this.IsNew;
				if (AddingNewPlace) {
					this.Navigation.PushModalAsync (new DetailPage (EditPlace, true));
				} else {
					if (IsNew) {
						this.Navigation.PopToRootAsync ();
					} else {
						this.Navigation.PopAsync ();
					}
				}
				#pragma warning restore 4014
			} else {
				await DisplayAlert ("Error", Message, "OK");
			}


		}

		#endregion
	}
}

