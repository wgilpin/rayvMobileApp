using System;

using Xamarin.Forms;

namespace RayvMobileApp
{
	public class IntroSlide : StackLayout
	{
		Image TopImage;
		Frame ImageBg;
		Grid grid;
		StackLayout ShowLayout;

		public void DoLayout ()
		{
			Double factor;
			factor = Height < 500 ? 0.2 : 0.4;
			TopImage.HeightRequest = this.Height * factor;
			ImageBg.Padding = Height < 500 ? 15 : 40;
		}

		public IntroSlide (
			string topPic, 
			string heading, 
			string pic1, 
			string text1, 
			string pic2, 
			string text2, 
			string pic3,
			string text3, 
			string buttonLabel, 
			EventHandler buttonAction,
			EventHandler onStopShowing = null,
			bool fullWidthButton = false,
			bool showKeepShowingButton = false
		)
		{
			VerticalOptions = LayoutOptions.FillAndExpand;
			HorizontalOptions = LayoutOptions.FillAndExpand;
			grid = new Grid {
				VerticalOptions = LayoutOptions.StartAndExpand,
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (10) }, //left space
					new ColumnDefinition { Width = new GridLength (25) }, // icon
					new ColumnDefinition { Width = new GridLength (10) }, // mid space
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) }, //text
					new ColumnDefinition { Width = new GridLength (10) }, //right space
				},
				RowSpacing = 20,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },//image
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },//title
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },//line1
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },//line2
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },//line3
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },//stop showing
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },//stop showing
				}
			};
			ImageBg = new Frame {
				BackgroundColor = settings.BaseColor,
				HasShadow = false,
				OutlineColor = settings.BaseColor,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Padding = 20,
			};
			TopImage = new Image{ Source = topPic, Aspect = Aspect.AspectFit,  };
			ImageBg.Content = TopImage;
			grid.Children.Add (ImageBg, 0, 5, 0, 1);
			Label Heading = new Label { 
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)), 
				Text = heading, 
			};
			grid.Children.Add (Heading, 1, 4, 1, 2);
			Image Pic1 = new Image{ Source = pic1, Aspect = Aspect.AspectFit, HeightRequest = 25 };
			grid.Children.Add (Pic1, 1, 2, 2, 3);
			Image Pic2 = new Image{ Source = pic2, Aspect = Aspect.AspectFit, HeightRequest = 25 };
			grid.Children.Add (Pic2, 1, 2, 3, 4);
			Image Pic3 = new Image{ Source = pic3, Aspect = Aspect.AspectFit, HeightRequest = 25 };
			grid.Children.Add (Pic3, 1, 2, 4, 5);
			var smallFont = Device.GetNamedSize (NamedSize.Small, typeof(Label));
			Label Text1 = new Label { Text = text1, FontSize = smallFont };
			grid.Children.Add (Text1, 3, 4, 2, 3);
			Label Text2 = new Label { Text = text2, FontSize = smallFont, };
			grid.Children.Add (Text2, 3, 4, 3, 4);
			Label Text3 = new Label { Text = text3, FontSize = smallFont, };
			grid.Children.Add (Text3, 3, 4, 4, 5);

			RayvButton Btn = new RayvButton {
				Text = buttonLabel,
				OnClick = buttonAction
			};
			if (!fullWidthButton) {
				Btn.HorizontalOptions = LayoutOptions.End;
				Btn.Text = "  " + buttonLabel + "  ";
			}
			if (showKeepShowingButton) {
				Label ShowLbl = new Label { Text = "Show this every time" };
				Switch ShowSw = new Switch { IsToggled = true };
				ShowSw.Toggled += (sender, e) => { 
					Persist.Instance.SetConfig (settings.SKIP_INTRO, !ShowSw.IsToggled);
				};
				ShowLayout = new StackLayout {
					Orientation = StackOrientation.Horizontal,
					Children = {
						ShowLbl,
						ShowSw,
					}
				};
			}

			Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0);
			Children.Add (new ScrollView{ VerticalOptions = LayoutOptions.FillAndExpand, Content = grid, }); 
			if (ShowLayout != null)
				Children.Add (ShowLayout);
			Children.Add (Btn); 
		}
	}
}


