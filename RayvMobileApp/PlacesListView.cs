using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;

namespace RayvMobileApp
{

	public class PlacesListView : ScrollView
	{
		public EventHandler<ItemTappedEventArgs> OnItemTapped { 
			set {
				DisplayedList.ItemTapped += value;
				SecondList.ItemTapped += value;
			}
		}

		private const int IMAGE_SIZE = 78;
		private bool ShowVotes;
		bool showingDistance = true;
		public List<Place> suppliedList;
		public List<Place> visibleList;
		public ListView DisplayedList;
		public ListView SecondList;
		public Position? SearchCentre;
		Button ShowMoreBtn;
		Label ShowStrangerLbl;
		Button ShowStrangerBtn;
		int currentListLength;
		Label SecondListHeaderLbl;

		public bool IsShowingDistance {
			get { return showingDistance; }
			set {
				showingDistance = value;
				DisplayedList.ItemTemplate = GetTemplate (ShowVotes);
				SecondList.ItemTemplate = GetTemplate (showVotes: false);
			}
		}

		public string ShowFriend = "";

		void AddListItems (int length)
		{
			DisplayedList.ItemsSource = null;
			visibleList = suppliedList.Take (length).ToList ();
			DisplayedList.ItemsSource = visibleList;
			ShowMoreBtn.IsVisible = suppliedList.Count > length;
			currentListLength = length;
		}

		public void SetMainItemSource (List<Place> list)
		{
			// the list is pre filtered to exclude those too far away
			// we only show the first 20
			Console.WriteLine ("SetMainItemSource");
			suppliedList = list;
			AddListItems (settings.MAX_INITIAL_LIST_LENGTH);
			if (!SecondList.IsVisible) {
				ShowStrangerLbl.IsVisible = list.Count < 10;
				ShowStrangerBtn.IsVisible = list.Count < 10;
			}
		}

		void DoShowMore (object sender, EventArgs e)
		{
			// Show more entries in mainlist
			if (suppliedList.Count > currentListLength) {
				Place currentEnd = suppliedList [currentListLength - 1];
				AddListItems (currentListLength + settings.MAX_INITIAL_LIST_LENGTH);
				DisplayedList.ScrollTo (currentEnd, ScrollToPosition.MakeVisible, false);
			} else
				ShowMoreBtn.IsVisible = false;
		}

		DataTemplate GetTemplate (bool showVotes)
		{
			return new DataTemplate (() => {
				// Create views with bindings for displaying each property.
				BackgroundColor = Color.White;
				Label nameLabel = new Label {
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
					FontAttributes = FontAttributes.Bold,
					LineBreakMode = LineBreakMode.TailTruncation,
				};
				nameLabel.SetBinding (Label.TextProperty, "place_name");

				Label catLabel = new Label ();
				catLabel.SetBinding (Label.TextProperty, "vote.cuisineName");
				catLabel.TextColor = Color.Black;
				catLabel.BackgroundColor = Color.Transparent;
				catLabel.FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label));

				Label distLabel = new Label {
					FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					TextColor = Color.Gray,
					BackgroundColor = Color.Transparent,
				};
				if (IsShowingDistance)
					distLabel.SetBinding (Label.TextProperty, "distance");

				Label addressLabel = new Label {
					FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					FontAttributes = FontAttributes.Italic,
					TranslationX = 85,
					TextColor = Color.Black,
					BackgroundColor = Color.Transparent,
				};
				addressLabel.SetBinding (
					Label.TextProperty, 
					new Binding ("address", converter: new AddressToShortAddressConverter ()));
				addressLabel.TextColor = Color.Black;

				Image webImage = new Image { 
					Aspect = Aspect.AspectFill,
					WidthRequest = IMAGE_SIZE, 
					HeightRequest = IMAGE_SIZE,
					TranslationY = 5,
					TranslationX = -3
				};
				webImage.SetBinding (Image.SourceProperty, "thumb_url");

				StackLayout draftSign = new StackLayout {
					BackgroundColor = Color.White,
					VerticalOptions = LayoutOptions.CenterAndExpand,
					Children = {
						new Label { Text = "Draft", TextColor = Color.Red, BackgroundColor = Color.White },
					}
				};
				draftSign.SetBinding (
					StackLayout.IsVisibleProperty, "IsDraft");

				var distCuisineLine = new StackLayout { Orientation = StackOrientation.Horizontal };
				distCuisineLine.Children.Add (distLabel);
				distCuisineLine.Children.Add (new Label{ Text = " " });
				distCuisineLine.Children.Add (catLabel);
				Grid grid = new Grid {
					VerticalOptions = LayoutOptions.FillAndExpand,
					Padding = 5,
					RowDefinitions = {
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						new RowDefinition { Height = new GridLength (1, GridUnitType.Auto)  },
						//						new RowDefinition { Height = new GridLength (19, GridUnitType.Absolute)  },
						//						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
						//						new RowDefinition { Height = new GridLength (15, GridUnitType.Absolute)  },
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength (70, GridUnitType.Absolute) },
						new ColumnDefinition { Width = new GridLength (IMAGE_SIZE, GridUnitType.Absolute) },
					}
				};
				grid.Children.Add (webImage, 2, 3, 0, 3);
				grid.Children.Add (draftSign, 2, 3, 0, 3);
				grid.Children.Add (nameLabel, 0, 2, 0, 1);
				grid.Children.Add (distCuisineLine, 0, 2, 1, 2);
				if (!ShowVotes)
					grid.Children.Add (addressLabel, 0, 2, 2, 3);

				// votes are shown on the main list, not on the Add lists
				if (showVotes) {
					var Stars = new StarEditor (showUntried: true) { Height = 12, ReadOnly = true, IsInFriendMode = ShowFriend != "" };
					Stars.SetBinding (StarEditor.VoteProperty, "vote.vote");
					Stars.SetBinding (
						StarEditor.VoteProperty,
						new Binding ("key", converter: new PlaceKeyToCorrectVoteScoreForList (), mode: BindingMode.OneWay)
					);
					Stars.SetBinding (
						StarEditor.UntriedProperty,
						new Binding ("key", converter: new PlaceKeyToCorrectUntriedForList (), mode: BindingMode.OneWay)
					);
//					Stars.SetBinding (StarEditor.UntriedProperty, "vote.untried");
//					Stars.SetBinding (StackLayout.IsVisibleProperty, "iVoted");
//					grid.Children.Add (Stars, 0, 2, 2, 3);
					var ratingLine = new StackLayout { Orientation = StackOrientation.Horizontal };
					var rating = new Label { TextColor = settings.ColorDarkGray, FontSize = settings.FontSizeLabelMicro };
					rating.SetBinding (Label.FormattedTextProperty, "Rating", stringFormat: "({0:F1})");
//					ratingLine.SetBinding (Label.IsVisibleProperty, "noVote");
					ratingLine.Children.Add (Stars);
					ratingLine.Children.Add (rating);
					grid.Children.Add (ratingLine, 0, 2, 2, 3);

				}
				return new ViewCell {
					View = grid,
				};
				// Return an assembled ViewCell.
			});
		}

		void DoShowStrangers (object sender, EventArgs e)
		{
			// server side search at thje chosen location (or here if there is no search position selected)
			Double lat = SearchCentre == null ? Persist.Instance.GpsPosition.Latitude : ((Position)SearchCentre).Latitude;
			Double lng = SearchCentre == null ? Persist.Instance.GpsPosition.Longitude : ((Position)SearchCentre).Longitude;
			var parms = new Dictionary<string, string> () {
				{ "lat", lat.ToString () },
				{ "lng", lng.ToString () }
			};
			var result = Persist.Instance.GetWebConnection ().get ("/api/items/all", parms).Content;
			if (string.IsNullOrEmpty (result)) {
				// nothing found on the server
				ShowStrangerLbl.Text = "No more found";
				ShowStrangerBtn.IsVisible = false;
			} else {
				ShowStrangerBtn.IsVisible = false;
				ShowStrangerLbl.IsVisible = false;
				JObject obj = JObject.Parse (result);
				string myId = Persist.Instance.GetConfig (settings.MY_ID);
				string placesStr = obj ["points"].ToString ();
				List<Place> place_list = JsonConvert.DeserializeObject<List<Place>> (placesStr);
				List<Place> strangerPlaces = new List<Place> ();
				foreach (Place p in place_list) {
					if (Persist.Instance.GetPlace (p.key) != null) {
						// we already had it
						continue;
					}
					p.CalculateDistanceFromPlace (SearchCentre);
					strangerPlaces.Add (p);
				}
				if (strangerPlaces.Count == 0) {
					SecondListHeaderLbl.Text = "No extra places found";
				} else {
					SecondListHeaderLbl.Text = "Other nearby places";
					strangerPlaces.Sort ();
					SecondList.ItemsSource = null;
					SecondList.ItemsSource = strangerPlaces;
				}
				SecondList.IsVisible = true;
			}
		}

		public PlacesListView (bool showVotes = true, bool showDistance = true) : base ()
		{
			Console.WriteLine ("PlacesListView()");
			ShowVotes = showVotes;
			DisplayedList = new ListView ();
			SecondList = new ListView { IsVisible = false, VerticalOptions = LayoutOptions.FillAndExpand };
			SecondListHeaderLbl = new Label{ Text = "Other nearby places" };
			SecondList.Header = SecondListHeaderLbl;
			ShowStrangerBtn = new RayvButton ("Show strangers' places"){ IsVisible = false };
			ShowStrangerBtn.Clicked += DoShowStrangers;
			ShowMoreBtn = new RayvButton ("More..."){ IsVisible = false };
			ShowMoreBtn.Clicked += DoShowMore;
			ShowStrangerLbl = new Label {
				Text = "No more places listed by your friends",
				LineBreakMode = LineBreakMode.WordWrap,
				XAlign = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				IsVisible = false
			};
			DisplayedList.RowHeight = showVotes ? 100 : 90;
			DisplayedList.Footer = ShowMoreBtn;
			SecondList.RowHeight = showVotes ? 100 : 90;
			DisplayedList.SeparatorColor = Color.FromHex ("CCC");
			SecondList.SeparatorColor = Color.FromHex ("CCC");
			//isShowingDistance setter also sets the data template
			IsShowingDistance = showDistance;
			VerticalOptions = LayoutOptions.FillAndExpand;
			Content = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					DisplayedList,
					ShowStrangerLbl,
					ShowStrangerBtn,
					SecondList
				}
			};
		}
	}
}

