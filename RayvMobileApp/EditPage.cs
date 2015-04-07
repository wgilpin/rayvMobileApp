using System;
using Xamarin.Forms;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;
using System.Diagnostics;
using Xamarin;
using System.Linq;
using System.Globalization;

namespace RayvMobileApp
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
		ButtonWide ConfirmAddressBtn;
		Entry PhoneNo;
		Entry WebSite;
		Entry AddressBox;
		Place EditPlace;
		Entry Comment;
		bool IsNew;
		bool Voted;

		// if adding a new place, on save we show the detaiPage, else it is just Pop
		private bool AddingNewPlace = false;

		#endregion

		public string Address {
			get { return AddressBox.Text; }
			set { AddressBox.Text = value; }
		}

		public Double Lat {
			get { return EditPlace.lat; }
			set { EditPlace.lat = value; }
		}

		public Double Lng {
			get { return EditPlace.lng; }
			set { EditPlace.lng = value; }
		}

		#region Constructors

		void SetOfflineVisibility ()
		{
			bool online = Persist.Instance.Online;
			WebSite.IsVisible = online;
			PhoneNo.IsVisible = online;
			Category.IsVisible = online;
			ConfirmAddressBtn.IsVisible = online;
			DeleteButton.Text = !online ? "Delete Draft" : "Remove from my lists";
		}

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
			AddressBox.Text = EditPlace.address;
			AddressBox.IsEnabled = String.IsNullOrEmpty (EditPlace.address);
			if (EditPlace.IsDraft)
				Comment.Text = EditPlace.DraftComment;
			else
				Comment.Text = EditPlace.Comment (); 

			WebSite.Text = EditPlace.website;
			WebSite.IsEnabled = String.IsNullOrEmpty (EditPlace.website);
			PhoneNo.Text = EditPlace.telephone;
			PhoneNo.IsEnabled = String.IsNullOrEmpty (EditPlace.telephone);
			if (!IsNew || place.IsDraft) {
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
			if (EditPlace.IsDraft) {
				ConfirmAddressBtn.IsVisible = true;
			}
			SetOfflineVisibility ();
				
		}

		public EditPage (bool addingNewPlace = false, bool editAsDraft = false)
		{
			Analytics.TrackPage ("EditPage");
			Title = "Details";
			AddingNewPlace = addingNewPlace;
			if (addingNewPlace && editAsDraft) {
				EditPlace = new Place ();
				EditPlace.IsDraft = true;
			}
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

			if (Persist.Instance.Categories != null) {
				Category = new Picker {
					Title = "Cuisine",
				};
				foreach (Category cat in Persist.Instance.Categories) {
					Category.Items.Add (cat.Title);
				}

				if (!editAsDraft)
					MainGrid.Children.Add (Category, 0, 3, Row, Row + 1);
			}
			Row++;
			AddressBox = new Entry {
				Text = "",
				Placeholder = "Address",
			};
			ConfirmAddressBtn = new ButtonWide { 
				Text = "Confirm Location",
				TextColor = Color.White,
				FontAttributes = FontAttributes.Bold,
				OnClick = DoConfirmAddress,
				IsVisible = false,
				BackgroundColor = settings.ColorDark,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button)),
			};
			var editAddress = new StackLayout {
				Children = {
					AddressBox,
					ConfirmAddressBtn,
				}
			};
			MainGrid.Children.Add (editAddress, 0, 3, Row, Row + 1);
			Row++;
			if (!editAsDraft) {
				WebSite = new Entry {
					Placeholder = "Website",
				};
				MainGrid.Children.Add (WebSite, 0, 3, Row, Row + 1);
			}
			Row++;
			if (!editAsDraft) {
				PhoneNo = new Entry {
					Placeholder = "Phone",
				};
				MainGrid.Children.Add (PhoneNo, 0, 3, Row, Row + 1);
			}
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
			Comment = new Entry ();
			Comment.Keyboard = Keyboard.Create (KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck);

			MainGrid.Children.Add (new Label { Text = "My Comment" }, 0, 3, Row, Row + 1);
			Row++;
			MainGrid.Children.Add (Comment, 0, 3, Row, Row + 1);
			Row++;
			SaveBtn = new ButtonWide {
				Text = "Save",
				BackgroundColor = settings.ColorDark,
				TextColor = Color.White,
				FontAttributes = FontAttributes.Bold,
			};
			SaveBtn.FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button));
			SaveBtn.Clicked += DoSave;
			MainGrid.Children.Add (SaveBtn, 0, 3, Row, Row + 1);
			Row++;
			DeleteButton = new ButtonWide {
				Text = editAsDraft ? "Delete Draft" : "Remove from my lists",
				BackgroundColor = Color.Red,
				TextColor = Color.White,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.End,
				IsVisible = false,
			};
			DeleteButton.FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button));
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
			SetOfflineVisibility ();
		}

		public EditPage (Position position, String address, string placeName = "", bool addingNewPlace = false) : this ()
		{
			AddingNewPlace = addingNewPlace;
			DeleteButton.IsVisible = false;
			IsNew = true;
			EditPlace = new Place ();
			EditPlace.lat = position.Latitude;
			EditPlace.lng = position.Longitude;
			EditPlace.address = address;
			if (!string.IsNullOrEmpty (placeName)) {
				EditPlace.place_name = placeName;
			}
			AddressBox.Text = address;
			SetOfflineVisibility ();
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
			voteBtn.BackgroundColor = settings.ColorDark;
			voteBtn.TextColor = Color.White;
			Voted = true;
		}

		async void DeletePlace (object sender, EventArgs e)
		{
			//delete the current item
			//is it in my list?
			bool confirm_delete = await DisplayAlert (
				                      "Delete", "Remove this place from your list? This cannot be undone", "Yes", "No");
			if (confirm_delete) {
				String name = EditPlace.place_name;
				EditPlace.Delete ();
				Insights.Track ("EditPage.DeletePlace", "Place", name);
				await Navigation.PopToRootAsync ();
			}
		}

		#endregion

		#region Events

		async private void DoConfirmAddress (Object o, EventArgs e)
		{
			// click map
			// geocode address
			var positions = (await (new Geocoder ()).GetPositionsForAddressAsync (AddressBox.Text)).ToList ();
			if (positions.Count > 0) {
				// load map at that location
				Navigation.PushAsync (new AddMapPage (positions.First ()));
			} else {
				// load map at my location
				Navigation.PushAsync (new AddMapPage ());
			}
		}

		async private void DoSave (object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty (Place_name.Text)) {
				await DisplayAlert ("Warning", "You must name the place", "OK");
				Place_name.Focus ();
				return;
			}

			if (!Voted) {
				await DisplayAlert ("Warning", "You must vote", "OK");
				return;
			}
			if (String.IsNullOrEmpty (Comment.Text)) {
				if (EditPlace.vote == "1" || EditPlace.vote == "-1") {
					// need to comment unless its a wishlist item
					await DisplayAlert ("Warning", "Please add a comment - it's for other people to know what you thought", "OK");
					Comment.Focus ();
					return;
				}
			}
			if (EditPlace.IsDraft) {
				EditPlace.DraftComment = Comment.Text;
			}
			if (Category.IsVisible) {
				if (Category.SelectedIndex == -1) {
					await DisplayAlert ("Warning", "You must pick a cuisine", "OK");
					return;
				}
				EditPlace.category = Category.Items [Category.SelectedIndex];
			}
		
			// set the vote even if editing a draft, in case Save works
			EditPlace.setComment (Comment.Text);
			// Creates a TextInfo based on the "en-US" culture.
			TextInfo myTI = new CultureInfo ("en-US", false).TextInfo;
			EditPlace.address = myTI.ToTitleCase (AddressBox.Text);
			EditPlace.place_name = myTI.ToTitleCase (Place_name.Text);
			;
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
					this.Navigation.PushModalAsync (new NavigationPage (new DetailPage (EditPlace, true)));
				} else {
					if (IsNew) {
						this.Navigation.PopToRootAsync ();
					} else {
						this.Navigation.PopAsync ();
					}
				}
				#pragma warning restore 4014
			} else {
				EditPlace.IsDraft = true;
				await DisplayAlert ("Not Saved", "Kept as draft", "OK");
				Persist.Instance.Places.Add (EditPlace);
				this.Navigation.PopToRootAsync ();
			}


		}

		#endregion
	}
}

