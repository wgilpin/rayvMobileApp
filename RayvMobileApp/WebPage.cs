using System;
using Xamarin.Forms;
using System.Diagnostics;

namespace RayvMobileApp
{
	public class WebPage : ContentPage
	{
		string URL;

		public WebPage ()
		{

		}

		public WebPage (String placeName, String url) : this ()
		{
			URL = url;

			Analytics.TrackPage ("WebPage");
			ActivityIndicator WebSpinner = new ActivityIndicator { Color = Color.Red, };

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
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.Blue,

			};

			webView.Navigating += (sender, e) => {
				WebSpinner.IsVisible = true;
				WebSpinner.IsRunning = true;
			};

			webView.Navigated += (sender, e) => {
				WebSpinner.IsVisible = false;
				WebSpinner.IsRunning = false;
			};

			// Accomodate iPhone status bar.
			this.Padding = new Thickness (10, Device.OnPlatform (20, 0, 0), 10, 5);

			// Build the page.
			this.Content = new StackLayout {
				Children = {
					header,
					WebSpinner,
					webView
				}
			};
			ToolbarItems.Add (new ToolbarItem {
				Text = " Browser  ",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => { 
					Device.OpenUri (new Uri (URL));
				})
			});
			ToolbarItems.Add (new ToolbarItem {
				Text = " Close",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => { 
					Navigation.PopModalAsync ();
				})
			});
		}
	}
}

