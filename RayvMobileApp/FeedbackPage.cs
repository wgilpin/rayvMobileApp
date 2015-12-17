using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin;

namespace RayvMobileApp
{
	public class FeedbackPage : ContentPage
	{
		Editor editor;

		async void DoComplete ()
		{
			await DisplayAlert ("Saved", "Thank you!", "OK");
			Navigation.PopAsync ();
		}

		void DoSend (Object s, EventArgs e)
		{
			// send the feedback to the server
			try {
				string text = editor.Text.Trim ();
				var parms = new Dictionary<string,string> {
					{ "Author",Persist.Instance.MyId.ToString () },
					{ "Comment",text },
					{ "ReplyTo","0" },
					{ "When",DateTime.UtcNow.ToString ("s") }
				};
				var res = Persist.Instance.GetWebConnection ().post ("/api/feedback", parms);
				if (res == "OK") {
					DoComplete ();
				} else {
					DisplayAlert ("Error", "Save failed", "OK");
				}
			} catch (Exception ex) {
				Console.WriteLine ($"FeedbackPage.DoSend ERROR {ex}");
				Insights.Report (ex);
			}
		}

		public FeedbackPage ()
		{
			editor = new Editor {
				VerticalOptions = LayoutOptions.FillAndExpand,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Editor)),
			};
			Content = new StackLayout { 
				VerticalOptions = LayoutOptions.FillAndExpand,
				Padding = 5,
				Children = {
					new Label { Text = "What does not destroy us makes us stronger. Give us your feedback." },
					new Frame { Content = editor, VerticalOptions = LayoutOptions.FillAndExpand },
					new ColouredButton { Text = "Send", OnClick = DoSend },
				}
			};
			Appearing += (sender, e) => {
				editor.Focus ();
			};
		}
	}
}


