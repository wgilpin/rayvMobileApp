using System;
using RayvMobileApp.Droid;
using Android.Content;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency (typeof(ShareIntent))]
namespace RayvMobileApp.Droid
{
	public class ShareIntent: IShareable
	{
		public ShareIntent ()
		{
		}

		public void OpenShareIntent (string textToShare)
		{
			var myIntent = new Intent (Android.Content.Intent.ActionSend);      
			myIntent.SetType ("text/plain"); 
			myIntent.PutExtra (Intent.ExtraText, textToShare); 
			Forms.Context.StartActivity (Intent.CreateChooser (myIntent, "Choose an App"));
		}
	}
}

