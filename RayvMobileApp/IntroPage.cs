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
				"Welcome to Taste5",
				settings.DevicifyFilename ("Location.png"),
				"Find place to eat, liked by your friends",
				settings.DevicifyFilename ("add 2.png"),
				"Add places to try later",
				settings.DevicifyFilename ("Share.png"),
				"Share places you like, with friends",
				"Next",
				GoTo2
			);
			p2 = new IntroSlide (
				settings.DevicifyFilename ("Intro head 2a.png"),
				"Add places to your list",
				settings.DevicifyFilename ("Like.png"),
				"Like: 'I would go here again'",
				settings.DevicifyFilename ("Dislike.png"),
				"Dislike: 'I would never go back here'",
				settings.DevicifyFilename ("Wish1.png"),
				"Wish: 'I want to try here soon'",
				"Next",
				GoTo3
			);
			p3 = new IntroSlide (
				settings.DevicifyFilename ("Intro 3 foodie friends.png"),
				"Follow Foodie Friends for Trusted Reviews",
				"",
				"",
				"",
				"",
				"",
				"",
				"Sign-in To Get Started",
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

