//using System;
//
//using Xamarin.Forms;
//
//namespace RayvMobileApp
//{
//	public class TestFormsPage : ContentPage
//	{
//		public static void TestHandler (object s, EventArgs e)
//		{
//			new System.Threading.Thread (new System.Threading.ThreadStart (() => {
//				Console.WriteLine ("Thread");
//				Device.BeginInvokeOnMainThread (() => {
//					Console.WriteLine ($"TestHandler {addingPlace.place_name}");
//				});
//			})).Start ();
//		}
//
//		public TestFormsPage ()
//		{
//			var button = new Button { Text = "Hello ContentPage" };
//			button.Clicked += TestHandler;
//			Content = new StackLayout { 
//				Children = {
//					button
//				}
//			};
//		}
//	}
//}


