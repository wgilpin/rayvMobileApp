using System;
using Xamarin.Forms;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{
	public class WebPage : ContentPage
	{
		public WebPage ()
		{

		}

		public WebPage (String placeName, String url) : this ()
		{
			Debug.WriteLine ("WebPage");
			Label header = new Label {
				Text = placeName,
				Font = Font.SystemFontOfSize (24, FontAttributes.Bold),
				HorizontalOptions = LayoutOptions.Center,
			};

			WebView webView = new WebView {
				Source = new UrlWebViewSource {
					Url = url,
				},
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			// Accomodate iPhone status bar.
			this.Padding = new Thickness (10, Device.OnPlatform (20, 0, 0), 10, 5);

			// Build the page.
			this.Content = new StackLayout {
				Children = {
					header,
					webView
				}
			};
		}
	}
}

