using System;
using Xamarin.Forms;
using RayvMobileApp;
using RayvMobileApp.iOS;
using Xamarin.Forms.Platform.iOS;
using UIKit;

[assembly: ExportRenderer (typeof(EntryClearable), typeof(EntryClearableRenderer))]
namespace RayvMobileApp.iOS
{
	public class EntryClearableRenderer: EntryRenderer
	{
		protected override void OnElementChanged (ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged (e);

			if (this.Control == null) {
				return;
			}

			Console.WriteLine (this.Control.GetType ().ToString ());
			var entry = this.Control as UITextField;

			entry.ClearButtonMode = UITextFieldViewMode.WhileEditing;

		}
	}
}


