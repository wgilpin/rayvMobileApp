using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class IntroPage : CarouselPage
	{
		public IntroPage ()
		{
			ContentPage p1 = new ContentPage {
				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				Content = new Image{ Source = "intro page 1.png" },
				Title = "Welcome"
			};
			ContentPage p2 = new ContentPage {
				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				Content = new Label {
					Text = "Page 2"
				},
				Title = "Intro 2"
			};
			ContentPage p3 = new ContentPage {
				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				Title = "Intro 3",
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.FillAndExpand,
					Children = {
						new Label {
							Text = "Page 3",
						},
						new RayvButton ("Get Started") {
							OnClick = (o, e) => {
								//Persist.Instance.SetConfig (settings.LAUNCHED_BEFORE, true);
								Navigation.PushModalAsync (App.GetFirstPage (SkipIntro: true));
							}
						}
					}
				}
			};
			Children.Add (p1);
			Children.Add (p2);
			Children.Add (p3);

			
		}
	}
}

