using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class IntroPage : CarouselPage	
	{
		public IntroPage ()
		{
			ContentPage p1 = new ContentPage {
				Content = new Label {
					Text = "Page 1"
				},
				Title = "Intro 1"
			};
			ContentPage p2 = new ContentPage {
				Content = new Label {
					Text = "Page 2"
				},
				Title = "Intro 2"
			};
			ContentPage p3 = new ContentPage {
				Content = new Label {
					Text = "Page 3"
				},
				Title = "Intro 3"
			};
			Children.Add (p1);
			Children.Add (p2);
			Children.Add (p3);

			
		}
	}
}

