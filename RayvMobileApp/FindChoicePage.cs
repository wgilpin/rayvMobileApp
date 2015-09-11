using System;

using Xamarin.Forms;
using Xamarin;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;
using System.Linq;

namespace RayvMobileApp
{
	

	public class FindChoicePage : ContentPage
	{
		#region Fields Properties

		string currentLocationStr;
		string currentLocationName;
		string currentCuisine;
		public MealKind currentKind;
		public PlaceStyle currentStyle;
		VoteFilterKind currentVoteKindFilter;
		string currentVoteByWho;

		const string ALL_CUISINES = "All Cuisines";
		Grid _grid;
		Position? SearchCentre;
		ActivityIndicator Spinner;
		RayvButton SearchLocationBtn;
		ToolbarItem BackBtn;
		string TitlePlaceStyle = "Style of Place";
		string TitleWho = "Who?";
		string TitleVote = "Votes...";
		string TitleFindMain = "Find...";
		string TitleMealKind = "Meal Time";
		LocationListWithHistory _geoLookupBox;
		Entry FilterSearchBox;

		public static int FilterMimimunStarValue = 0;

		#endregion

		#region Cards

		public int RowCount {
			set { 
				_grid.RowDefinitions.Clear ();
				_grid.Children.Clear ();
				_grid.RowDefinitions.Add (new RowDefinition { 
					Height = new GridLength (1, GridUnitType.Auto)
				});
				for (int i = 1; i < value; i++) {
					_grid.RowDefinitions.Add (new RowDefinition { 
						Height = new GridLength (1, GridUnitType.Star)
					});
				}
			}
			get { return _grid.RowDefinitions.Count; }
		}


		void AddImgCard (int line, string imageSource, string text, EventHandler onClick)
		{
			Image _img;
			Label _text;
			Frame _bg;
			TapGestureRecognizer _gesture;
			var imgWidth = RowCount < 6 ? 60 : 50;
			_bg = new Frame { 
				HasShadow = false, 
				OutlineColor = settings.BaseColor, 
				BackgroundColor = settings.BaseColor,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};
			_img = new Image {
				Source = imageSource,
				Aspect = Aspect.AspectFit,
				WidthRequest = imgWidth,
				HeightRequest = imgWidth
			};
			_text = new Label {
				FontSize = settings.FontSizeLabelLarge,
				TextColor = Color.White,
				Text = text,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Start,
			};
			_gesture = new TapGestureRecognizer ();
			_text.GestureRecognizers.Add (_gesture);
			_img.GestureRecognizers.Add (_gesture);
			_bg.GestureRecognizers.Add (_gesture);
//			var _line = new BoxView {
//				BackgroundColor = Color.White,
//				HeightRequest = 2,
//				VerticalOptions = LayoutOptions.End,
//				HorizontalOptions = LayoutOptions.FillAndExpand,
//			};
			_gesture.Tapped += onClick;
			_grid.Children.Add (_bg, 0, 3, line, line + 1);	
			_grid.Children.Add (_img, 0, 1, line, line + 1);	
			_grid.Children.Add (_text, 1, 3, line, line + 1);	
		}

		void AddTextCard (int line, string left, string right, EventHandler onClick, bool highlight = false)
		{
			Label _left;
			Label _right;
			Frame _bg;
			TapGestureRecognizer _gesture;

			_bg = new Frame { 
				HasShadow = false, 
				OutlineColor = settings.BaseColor, 
				BackgroundColor = settings.BaseColor,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};
			_left = new Label {
				FontSize = settings.FontSizeLabelLarge,
				TextColor = Color.White,
				Text = left,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Start,
			};
			_right = new Label {
				FontSize = settings.FontSizeLabelMedium,
				TextColor = highlight ? Color.White : settings.ColorDarkGray,
				Text = right,
				FontAttributes = highlight ? FontAttributes.Italic | FontAttributes.Bold : FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.End,
			};

			_gesture = new TapGestureRecognizer ();
			_right.GestureRecognizers.Add (_gesture);
			_left.GestureRecognizers.Add (_gesture);
			_bg.GestureRecognizers.Add (_gesture);
			//			var _line = new BoxView {
			//				BackgroundColor = Color.White,
			//				HeightRequest = 2,
			//				VerticalOptions = LayoutOptions.End,
			//				HorizontalOptions = LayoutOptions.FillAndExpand,
			//			};
			_gesture.Tapped += onClick;
			_grid.Children.Add (_bg, 0, 3, line, line + 1);	
			_grid.Children.Add (_left, 0, 2, line, line + 1);	
			_grid.Children.Add (_right, 2, 3, line, line + 1);	
			//			_grid.Children.Add (_line, 0, 1, line, line + 1);	
		}

		#endregion


		#region Choice Pages

		void Done (object sender, EventArgs e)
		{
			Persist.Instance.SetConfig (settings.FILTER_WHAT, (int)currentVoteKindFilter);
			Persist.Instance.SetConfig (settings.FILTER_WHO, currentVoteByWho);
			var cuisine = currentCuisine == ALL_CUISINES ? null : currentCuisine;
			Persist.Instance.SetConfig (settings.FILTER_CUISINE, cuisine);
			Persist.Instance.SetConfig (settings.FILTER_KIND, (int)currentKind);
			Persist.Instance.SetConfig (settings.FILTER_STYLE, (int)currentStyle);
			if (SearchCentre.Equals (null)) {
				Persist.Instance.SetConfig (settings.FILTER_WHERE_LAT, null);
				Persist.Instance.SetConfig (settings.FILTER_WHERE_LNG, null);
				Persist.Instance.SetConfig (settings.FILTER_WHERE_NAME, null);
			} else {
				Persist.Instance.SetConfig (settings.FILTER_WHERE_LAT, ((Position)SearchCentre).Latitude);
				Persist.Instance.SetConfig (settings.FILTER_WHERE_LNG, ((Position)SearchCentre).Longitude);
				Persist.Instance.SetConfig (settings.FILTER_WHERE_NAME, currentLocationName);
			}
			//write a comma separated list of keys
			var listPage = new ListPage { 
				FilterPlaceKind = currentKind,
				FilterPlaceStyle = currentStyle,
				FilterSearchCenter = SearchCentre,
				FilterCuisine = cuisine,
				FilterShowWho = currentVoteByWho,
				FilterVoteKind = currentVoteKindFilter
			};
			Navigation.PushModalAsync (new RayvNav (listPage));
		}

		void ChooseStyle ()
		{
			Title = TitlePlaceStyle;
			RowCount = 4;
			AddImgCard (0, "", "Any", (s, e) => {
				currentStyle = PlaceStyle.None;
				ChooseMainMenu ();
			});
			AddImgCard (1, "a_quick_bite_place.png", "Quick Bite", (s, e) => {
				currentStyle = PlaceStyle.QuickBite;
				ChooseMainMenu ();
			});
			AddImgCard (2, "a_relaxed_place.png", "Relaxed", (s, e) => {
				currentStyle = PlaceStyle.Relaxed;
				ChooseMainMenu ();
			});
			AddImgCard (3, "a_fancy_place.png", "Fancy", (s, e) => {
				currentStyle = PlaceStyle.Fancy;
				ChooseMainMenu ();
			});
		}

		void ChooseFriends ()
		{
			var chooser = new FriendsChooserView ();
			chooser.Saved += (sender, e) => {
				Content = _grid;
				currentVoteByWho = chooser.SelectedKey;
				ChooseMainMenu ();
			};
			Content = chooser;
		}

		void ChooseWho ()
		{
			Title = TitleWho;
			RowCount = 3;
			AddImgCard (0, "", "My Places Only", (s, e) => {
				currentVoteByWho = Persist.Instance.MyId.ToString ();
				ChooseMainMenu ();
			});
			AddImgCard (1, "", "All Places", (s, e) => {
				currentVoteByWho = "";
				ChooseMainMenu ();
			});
			AddImgCard (2, "", "Friend's Places", (s, e) => {
				currentVoteByWho = "";
				ChooseFriends ();
			});
		}

		void ChooseVote ()
		{
			Title = TitleVote;
			RowCount = 4;
			StarEditor Stars = new StarEditor (false);
			_grid.Children.Add (Stars, 0, 3, 0, 1);
			Stars.ChangedNotUI += (o, e) => {
				var args = e as StarEditorEventArgs;
				currentVoteKindFilter = VoteFilterKind.Stars;
				FilterMimimunStarValue = args.Vote;
				Device.BeginInvokeOnMainThread (() => {
					ChooseMainMenu ();
				});
			};
			if (currentVoteKindFilter == VoteFilterKind.Stars)
				Stars.Vote = FilterMimimunStarValue;
			AddTextCard (1, "Wishes Only", null, (s, e) => {
				currentVoteKindFilter = VoteFilterKind.Wish;
				ChooseMainMenu ();
			});
			AddTextCard (2, "Wish or Like", null, (s, e) => {
				currentVoteKindFilter = VoteFilterKind.Try;
				ChooseMainMenu ();
			});
			AddTextCard (3, "All", null, (s, e) => {
				currentVoteKindFilter = VoteFilterKind.All;
				ChooseMainMenu ();
			});
		}


		void ChooseCuisine ()
		{
			var cuisinePage = new EditCuisineView (null, inFlow: false, showAllButton: true);
			cuisinePage.Saved += (sender, e) => {
				if (e.ShowAll)
					currentCuisine = null;
				else
					currentCuisine = e.Cuisine.Title;
//				Navigation.PopAsync ();
				ChooseMainMenu ();
			};
			cuisinePage.Cancelled += (sender, e) => {
				ChooseMainMenu ();
			};
			Content = cuisinePage;
		}

		void DoBackBtn ()
		{
			if (Content.GetType () == typeof(FriendsChooserView)) {
				Content = _grid;
				return;
			}
			if (Title == TitleFindMain) {
				Console.WriteLine ("DoBackBtn Pop");
				Navigation.PopModalAsync ();
			} else {
				ChooseMainMenu ();
			}
		}

		void LoadPreviousSearch ()
		{
			currentVoteKindFilter = (VoteFilterKind)Persist.Instance.GetConfigInt (settings.FILTER_WHAT);
			currentVoteByWho = Persist.Instance.GetConfig (settings.FILTER_WHO);
			currentCuisine = Persist.Instance.GetConfig (settings.FILTER_CUISINE);
			if (string.IsNullOrEmpty (currentCuisine)) {
				currentCuisine = ALL_CUISINES;
			}
			currentKind = (MealKind)Persist.Instance.GetConfigInt (settings.FILTER_KIND);
			currentStyle = (PlaceStyle)Persist.Instance.GetConfigInt (settings.FILTER_STYLE);
			var lat = Persist.Instance.GetConfigDouble (settings.FILTER_WHERE_LAT);
			SearchCentre = null;
			if (lat != 0.0) {
				SearchCentre = new Position (
					lat,
					Persist.Instance.GetConfigDouble (settings.FILTER_WHERE_LNG));
				currentLocationName = Persist.Instance.GetConfig (settings.FILTER_WHERE_NAME);
			} 
		}

		string GetFriendText ()
		{
			if (currentVoteByWho == Persist.Instance.MyId.ToString ())
				return "Mine";
			if (currentVoteByWho == "")
				return "All";
			try {
				return Persist.Instance.Friends [currentVoteByWho].Name;
			} catch (KeyNotFoundException) {
				currentVoteByWho = "";
				return "All";
			}
		}

		void ChooseMainMenu ()
		{
			Title = TitleFindMain;
			RowCount = 7;

			bool isFiltered = false;
			string currentKindStr;
			bool kindFiltered = false;
			if (currentKind == MealKind.None || (int)currentKind == Vote.MAX_MEALKIND) {
				currentKindStr = "Any time";
			} else {
				currentKindStr = currentKind.ToString ();
				kindFiltered = true;
				isFiltered = true;
			}
			string currentStyleStr;
			var anyStyleStr = "Any style";
			if (currentStyle == PlaceStyle.None)
				currentStyleStr = anyStyleStr;
			else {
				currentStyleStr = currentStyle.ToString ();
				isFiltered = true;
			}
			var nearMeStr = "Near Me";
			if (string.IsNullOrEmpty (currentLocationName))
				currentLocationStr = nearMeStr;
			else {
				currentLocationStr = currentLocationName;
				isFiltered = true;
			}
			if (string.IsNullOrEmpty (currentCuisine))
				currentCuisine = ALL_CUISINES;
			else {
				isFiltered = true;
			}
			_grid.Children.Add (FilterSearchBox, 0, 3, 0, 1);
			AddTextCard (1, "Meal Time?", currentKindStr, (s, e) => {
				ChooseMealTime ();
			}, highlight: kindFiltered);
			AddTextCard (2, "Style?", currentStyleStr, (s, e) => {
				ChooseStyle ();
			}, highlight: currentStyleStr != anyStyleStr);
			AddTextCard (3, "Where?", currentLocationStr, (s, e) => {
				ChooseLocation ();
			}, highlight: currentLocationStr != nearMeStr);
			AddTextCard (4, "Cuisine?", currentCuisine, (s, e) => {
				ChooseCuisine ();
			}, highlight: currentCuisine != ALL_CUISINES);
			AddTextCard (5, "Who?", GetFriendText (), (s, e) => {
				ChooseWho ();
			}, highlight: !string.IsNullOrEmpty (currentVoteByWho));
			string RatingText;
			if (currentVoteKindFilter == VoteFilterKind.Stars) {
				RatingText = FilterMimimunStarValue > 1 ? 
					$"{FilterMimimunStarValue}+ Stars": 
					"All";
			} else
				RatingText = currentVoteKindFilter.ToString ();
			AddTextCard (6, "Rating?", RatingText, (s, e) => {
				ChooseVote ();
			}, highlight: currentVoteKindFilter != VoteFilterKind.All);
			var goBtn = new RayvButton { 
				Text = isFiltered ? "Search" : "Show All Places", 
				BackgroundColor = settings.BaseColor,
				BorderColor = Color.White,
				BorderWidth = 1,
				BorderRadius = 0,
			};
			goBtn.Clicked += Done;
			_grid.Children.Add (goBtn, 0, 3, 7, 8);
			Content = _grid;
		}

		void ChooseMealTime ()
		{
			Title = TitleMealKind;
			RowCount = 6;
			AddImgCard (0, "", "Any", (s, e) => {
				currentKind = MealKind.Breakfast | MealKind.Coffee | MealKind.Lunch | MealKind.Dinner | MealKind.Bar;
				ChooseMainMenu ();
			});
			AddImgCard (1, "a_breakfast.png", "Breakfast", (s, e) => {
				currentKind = MealKind.Breakfast;
				ChooseMainMenu ();
			});
			AddImgCard (2, "a_coffee.png", "Coffee", (s, e) => {
				currentKind = MealKind.Coffee;
				ChooseMainMenu ();
			});
			AddImgCard (3, "a_lunch.png", "Lunch", (s, e) => {
				currentKind = MealKind.Lunch;
				ChooseMainMenu ();
			});
			AddImgCard (4, "a_dinner.png", "Dinner", (s, e) => {
				currentKind = MealKind.Dinner;
				ChooseMainMenu ();
			});
			AddImgCard (5, "a_drinks.png", "Bar", (s, e) => {
				currentKind = MealKind.Bar;
				ChooseMainMenu ();
			});
		}

		void ChooseLocation ()
		{
			Title = "Location";
			_grid.RowDefinitions.Clear ();
			_grid.Children.Clear ();
			_geoLookupBox = new LocationListWithHistory ();
			_geoLookupBox.OnItemTapped = DoSearchAtLocation;
			_geoLookupBox.OnCancel = (s, e) => ChooseMainMenu ();
			_grid.RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) });
			_grid.RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) });
			_grid.RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) });
			_grid.RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) });
			_grid.RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) });
			_grid.RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Star) });
			_grid.Children.Add (new LabelWide ("Search near..."){ TextColor = Color.White }, 0, 3, 0, 1);
			SearchLocationBtn = new RayvButton { 
				Text = "Lookup Place", 
				BackgroundColor = settings.BaseColor,
				BorderColor = Color.White,
				BorderWidth = 1,
				BorderRadius = 0,
			};
			//			_geoLookupBox..Clicked += DoSearchAtLocation;
			//			placeHistory = new HistoryList ("FindChoiceLocation"){ HeightRequest = 230 };
			//			placeHistory.ItemSelected = (sender, args) => {
			//				DoSearchAtLocation (sender, null);
			//			};
			_grid.Children.Add (_geoLookupBox, 0, 3, 1, 6);
			//			_grid.Children.Add (SearchLocationBox, 0, 3, 1, 2);
			//			_grid.Children.Add (placeHistory, 0, 3, 2, 3);
			//			_grid.Children.Add (Spinner, 0, 3, 3, 4);
			//			_grid.Children.Add (SearchLocationBtn, 0, 3, 4, 5);
			_geoLookupBox.Focus ();	
		}

		#endregion

		#region Locations

		public void  NarrowGeoSearch (GeoLocation geoPt)
		{
			Debug.WriteLine ("Listpage.NarrowGeoSearch");
			try {
				SearchCentre = new Position (geoPt.Lat, geoPt.Lng);
				currentLocationName = geoPt.Name;
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}


		void DoSearchAtLocation (object o, ItemTappedEventArgs e)
		{
			SearchLocationBtn.IsEnabled = false;
			Spinner.IsRunning = true;
			var it = (GeoLocation)e.Item;
			NarrowGeoSearch (it);
			Spinner.IsRunning = false;
			ChooseMainMenu ();
			SearchLocationBtn.IsEnabled = true;
		}

		void DoFindLocation (object sender, EventArgs e)
		{
//			Spinner.IsRunning = true;
//			SearchHereBtn.IsVisible = false;
//			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
//				Parameters parameters = new Parameters ();
//				parameters ["address"] = LocationEditBox.Text;
//				try {
//					string result = restConnection.Instance.get ("/api/geocode", parameters).Content;
//					JObject obj = JObject.Parse (result);
//					//obj["results"][1]["formatted_address"].ToString()
//					LocationList = new List<GeoLocation> ();
//					int count = obj ["results"].Count ();
//					if (count == 0) {
//						NothingFound.IsVisible = true;
//					} else {
//						Double placeLat;
//						Double placeLng;
//						for (int idx = 0; idx < count; idx++) {
//							Double.TryParse (
//								obj ["results"] [idx] ["geometry"] ["location"] ["lat"].ToString (), out placeLat);
//							Double.TryParse (
//								obj ["results"] [idx] ["geometry"] ["location"] ["lng"].ToString (), out placeLng);
//							LocationList.Add (
//								new GeoLocation {
//									Name = obj ["results"] [idx] ["formatted_address"].ToString (),
//									Lat = placeLat,
//									Lng = placeLng,
//								});
//						}
//						Device.BeginInvokeOnMainThread (() => {
//							Spinner.IsRunning = false;
//							LocationResultsView.ItemsSource = LocationList;
//							LocationResultsView.IsVisible = true;
//							ResetLocationBtn.IsVisible = true;
//							LocationSearchedBox.IsVisible = false;
//							LocationEditBox.IsVisible = true;
//							NothingFound.IsVisible = false;
//							PlacesListView.IsVisible = false;
//						});
//					}
//				} catch (Exception ex) {
//					Insights.Report (ex);
//				}
//
//			})).Start ();
		}


		void DoClearFilters ()
		{
			currentVoteKindFilter = VoteFilterKind.All;
			currentVoteByWho = "";
			currentCuisine = ALL_CUISINES;
			currentKind = MealKind.Breakfast | MealKind.Coffee | MealKind.Lunch | MealKind.Dinner | MealKind.Bar;
			currentStyle = PlaceStyle.None;
			SearchCentre = null;
			currentLocationName = "";
			currentVoteByWho = "";
			ChooseMainMenu ();
		}


		#endregion

		#region Constructor

		public FindChoicePage (Page caller)
		{
			
			string savedVoteChoice = Persist.Instance.GetConfig (settings.FILTER_WHO);
			if (string.IsNullOrEmpty (savedVoteChoice))
				currentVoteByWho = "";
			else {
				if (savedVoteChoice == "All") {
					currentVoteByWho = "";
				}
				if (savedVoteChoice == "Mine") {
					currentVoteByWho = Persist.Instance.MyId.ToString ();
				}
			}
			_grid = new Grid {
				Padding = Device.OnPlatform (20, 2, 2),
				BackgroundColor = settings.BaseColor,
				RowSpacing = 5,
				ColumnSpacing = 20,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (60) },
					new ColumnDefinition { Width = new GridLength (50) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			FilterSearchBox = new Entry {
				Placeholder = "Search for place",
				Text = "",
			};
			Spinner = new ActivityIndicator{ Color = Color.Red, IsRunning = false };
			SearchCentre = null;
			LoadPreviousSearch ();
			ChooseMainMenu ();
			Padding = Device.OnPlatform (3, 15, 5);
			BackgroundColor = settings.BaseColor;
			Content = _grid;
			var ClearBtn = new ToolbarItem {
				Text = " Clear ",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					DoClearFilters ();
				}),
			};
			ToolbarItems.Add (ClearBtn);
			
			BackBtn = new ToolbarItem {
				Text = "Back ",
				//				Icon = "icon-map.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => {
					DoBackBtn ();
				}),
			};
			ToolbarItems.Add (BackBtn);
		}

		#endregion
	}
}


