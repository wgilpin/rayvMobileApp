using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class IntroPage : ContentPage
	{
		IntroSlide p1, p2, p3;

		void GoTo2 (object o, EventArgs e)
		{
			Content = p2;
		}

		void GoTo3 (object o, EventArgs e)
		{
			Content = p3;
		}

		void GetStarted (object o, EventArgs e)
		{
			Navigation.PushModalAsync (App.GetFirstPage (SkipIntro: true));
		}

		public IntroPage ()
		{
			Image Image1 = new Image{ Source = "intro page 1.png" };
			p1 = new IntroSlide (
				"logo.png",
				"Welcome to Taste5",
				"Location.png",
				"Instantly find place to eat, liked by your friends",
				"Wish1.png",
				"Quickly add places you wish to try in the future",
				"Share.png",
				"Share places you like with your friends",
				"Next",
				GoTo2
			);
			p2 = new IntroSlide (
				"Icon-60@2x.png",
				"Welcome to Taste5",
				"TB default search.png",
				"Instantly find place to eat, liked by your friends",
				"Icon default Website.png",
				"Quickly add places you wish to try in the future",
				"TB default news (1).png",
				"Share places you like with your friends",
				"Next",
				GoTo3
			);
			p3 = new IntroSlide (
				"Icon-60@2x.png",
				"Page2",
				"TB default search.png",
				"Instantly find place to eat, liked by your friends",
				"Icon default Website.png",
				"Quickly add places you wish to try in the future",
				"TB default news (1).png",
				"Share places you like with your friends",
				"Get Started",
				GetStarted 
			);
			Content = p1;
			this.Appearing += (sender, e) => {
				p1.DoLayout ();
			};
		}
	}
}

