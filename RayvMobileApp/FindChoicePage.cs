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


		const string ALL_CUISINES = "All Cuisines";
		Grid _grid;
		List<Place> DisplayList;
		Position? SearchCentre;
		ActivityIndicator Spinner;
		string SearchLocationName;
		string SearchCuisine;
		VoteFilterWho ByWho;
		RayvButton SearchLocationBtn;
		ToolbarItem BackBtn;
		string TitlePlaceStyle = "Style of Place";
		string TitleWho = "Who?";
		string TitleFindMain = "Find...";
		string TitleMealKind = "Eating Time";
		bool DEBUG_ON_SIMULATOR = DependencyService.Get<IDeviceSpecific> ().RunningOnIosSimulator ();
		LocationListWithHistory _geoLookupBox;

		public MealKind Kind { get; private set; }

		public PlaceStyle Style { get; private set; }

		#endregion

		#region Cards

		public int RowCount {
			set { 
				_grid.RowDefinitions.Clear ();
				_grid.Children.Clear ();
				for (int i = 0; i < value; i++) {
					_grid.RowDefinitions.Add (new RowDefinition { 
						Height = new GridLength (1, GridUnitType.Star)
					});
				}
			}
		}


		void AddImgCard (int line, string imageSource, string text, EventHandler onClick)
		{
			Image _img;
			Label _text;
			Frame _bg;
			TapGestureRecognizer _gesture;

			_bg = new Frame { 
				HasShadow = false, 
				OutlineColor = settings.BaseColor, 
				BackgroundColor = settings.BaseColor,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};
			_img = new Image { Source = imageSource, Aspect = Aspect.AspectFit, WidthRequest = 60 };
			_text = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
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

		void AddTextCard (int line, string left, string right, EventHandler onClick)
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
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
				TextColor = Color.White,
				Text = left,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Start,
			};
			_right = new Label {
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
				TextColor = Color.White,
				Text = right,
				FontAttributes = FontAttributes.Italic,
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
			Persist.Instance.SetConfig (settings.CHOICE_BY_WHO, ByWho.ToString ());
			var cuisine = SearchCuisine == ALL_CUISINES ? null : SearchCuisine;
			Navigation.PushModalAsync (new RayvNav (new ListPage (Kind, Style, SearchCentre, cuisine, ByWho)));
		}

		void ChooseStyle ()
		{
			Title = TitlePlaceStyle;
			RowCount = 4;
			AddImgCard (0, "", "Any", (s, e) => {
				Style = PlaceStyle.None;
				ChooseMainMenu ();
			});
			AddImgCard (1, "a_quick_bite_place.png", "Quick Bite", (s, e) => {
				Style = PlaceStyle.QuickBite;
				ChooseMainMenu ();
			});
			AddImgCard (2, "a_relaxed_place.png", "Relaxed", (s, e) => {
				Style = PlaceStyle.Relaxed;
				ChooseMainMenu ();
			});
			AddImgCard (3, "a_fancy_place.png", "Fancy", (s, e) => {
				Style = PlaceStyle.Fancy;
				ChooseMainMenu ();
			});
		}

		void ChooseWho ()
		{
			Title = TitleWho;
			RowCount = 2;
			AddImgCard (0, "", "My Places Only", (s, e) => {
				ByWho = VoteFilterWho.Mine;
				ChooseMainMenu ();
			});
			AddImgCard (1, "", "All Places", (s, e) => {
				ByWho = VoteFilterWho.All;
				ChooseMainMenu ();
			});
		}

		void ChooseCuisine ()
		{
			var cuisinePage = new EditCuisinePage (null, inFlow: false);
			cuisinePage.Saved += (sender, e) => {
				SearchCuisine = e.Cuisine.Title;
				Navigation.PopAsync ();
				ChooseMainMenu ();
			};
			Navigation.PushAsync (new RayvNav (cuisinePage));
		}

		void DoBackBtn ()
		{
			if (Title == TitleFindMain) {
				Navigation.PushModalAsync (new RayvNav (new MainMenu ()));
			} else {
				ChooseMainMenu ();
			}
		}

		void ChooseMainMenu ()
		{
			Title = TitleFindMain;
			RowCount = 5;
			ToolbarItems.Clear ();
			_grid.RowDefinitions.Add (new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) });
			bool isFiltered = false;
			string kindStr;
			string styleStr;
			string locationStr;
			if (Kind == MealKind.None || (int)Kind == Vote.MAX_MEALKIND)
				kindStr = "Any time";
			else {
				kindStr = Kind.ToString ();
				isFiltered = true;
			}
			if (Style == PlaceStyle.None)
				styleStr = "Any style";
			else {
				styleStr = Style.ToString ();
				isFiltered = true;
			}
			if (string.IsNullOrEmpty (SearchLocationName))
				locationStr = "Near Me";
			else {
				locationStr = SearchLocationName;
				isFiltered = true;
			}
			if (string.IsNullOrEmpty (SearchCuisine))
				SearchCuisine = ALL_CUISINES;
			else {
				isFiltered = true;
			}
			AddTextCard (0, "When?", kindStr, (s, e) => {
				ChooseMealTime ();
			});
			AddTextCard (1, "Style?", styleStr, (s, e) => {
				ChooseStyle ();
			});
			AddTextCard (2, "Where?", locationStr, (s, e) => {
				ChooseLocation ();
			});
			AddTextCard (3, "Cuisine?", SearchCuisine, (s, e) => {
				ChooseCuisine ();
			});
			AddTextCard (4, "Who?", ByWho.ToString (), (s, e) => {
				ChooseWho ();
			});
			var goBtn = new RayvButton { 
				Text = isFiltered ? "Search" : "Show All Places", 
				BackgroundColor = settings.BaseColor,
				BorderColor = Color.White,
				BorderWidth = 1,
				BorderRadius = 0,
			};
			goBtn.Clicked += Done;
			_grid.Children.Add (goBtn, 0, 3, 5, 6);
		}

		void ChooseMealTime ()
		{
			Title = TitleMealKind;
			RowCount = 5;
			AddImgCard (0, "", "Any", (s, e) => {
				Kind = MealKind.Breakfast | MealKind.Coffee | MealKind.Lunch | MealKind.Dinner;
				ChooseMainMenu ();
			});
			AddImgCard (1, "a_breakfast.png", "Breakfast", (s, e) => {
				Kind = MealKind.Breakfast;
				ChooseMainMenu ();
			});
			AddImgCard (2, "a_coffee.png", "Coffee", (s, e) => {
				Kind = MealKind.Coffee;
				ChooseMainMenu ();
			});
			AddImgCard (3, "a_lunch.png", "Lunch", (s, e) => {
				Kind = MealKind.Lunch;
				ChooseMainMenu ();
			});
			AddImgCard (4, "a_dinner.png", "Dinner", (s, e) => {
				Kind = MealKind.Dinner;
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
				SearchLocationName = geoPt.Name;
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

		#endregion

		#region Constructor

		public FindChoicePage (bool showBackBtn = true)
		{
			string savedVoteChoice = Persist.Instance.GetConfig (settings.CHOICE_BY_WHO);
			if (string.IsNullOrEmpty (savedVoteChoice))
				ByWho = VoteFilterWho.All;
			else {
				if (savedVoteChoice == "All") {
					ByWho = VoteFilterWho.All;
				}
				if (savedVoteChoice == "Mine") {
					ByWho = VoteFilterWho.Mine;
				}
			}
			_grid = new Grid {
				Padding = 20,
				BackgroundColor = settings.BaseColor,
				RowSpacing = 5,
				ColumnSpacing = 20,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (70) },
					new ColumnDefinition { Width = new GridLength (70) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			Spinner = new ActivityIndicator{ Color = Color.Red, IsRunning = false };
			ChooseMainMenu ();
			Content = _grid;
			SearchCentre = null;
			if (showBackBtn) {
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
		}

		#endregion
	}
}


