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
			p2.DoLayout ();
		}

		void GoTo3 (object o, EventArgs e)
		{
			Content = p3;
			p3.DoLayout ();
		}

		void GetStarted (object o, EventArgs e)
		{
			Navigation.PushModalAsync (App.GetFirstPage (SkipIntro: true));
		}

		void StopShowingIntro (object o, EventArgs e)
		{
			Persist.Instance.SetConfig (settings.SKIP_INTRO, true);	
		}

		public IntroPage ()
		{
			p1 = new IntroSlide (
				settings.DevicifyFilename ("logo.png"),
				"Welcome to Sprout",
				settings.DevicifyFilename ("directions_green2.png"),
				"Find place to eat, liked by your friends",
				settings.DevicifyFilename ("Share_green2.png"),
				"Share places you like, with friends",
				settings.DevicifyFilename ("Wish_Green.png"),
				"Add places to try later",
				"Next",
				GoTo2
			);
			p2 = new IntroSlide (
				settings.DevicifyFilename ("Intro_page2.png"),
				"Add places to your list",
				settings.DevicifyFilename ("stars_example.png"),
				"Stars show how you rate a place'",
				"Or add places to try later",
				settings.DevicifyFilename ("Wish_Green.png"),
				"Next time I want to try this place",
				"Next",
				GoTo3
			);
			p3 = new IntroSlide (
				settings.DevicifyFilename ("Intro_page3.png"),
				"Follow Foodie Friends for Trusted Reviews",
				settings.DevicifyFilename ("Friends_green.png"),
				"Connect to friends to share your trusted reviews",
				settings.DevicifyFilename ("Share_green2.png"),
				"Tell your friends where to meet you",
				"",
				"",
				"Get Started",
				GetStarted,
				fullWidthButton: true,
				onStopShowing: StopShowingIntro,
				showKeepShowingButton: true
			);
			Content = p1;
			this.Appearing += (sender, e) => {
				p1.DoLayout ();
			};
		}
	}
}

