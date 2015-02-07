using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class RayvButton : Button
	{
		public RayvButton () : base ()
		{
			//BorderWidth = 2;
			//BorderColor = Color.FromHex ("#4444AA");
			BorderRadius = 0;
			BackgroundColor = Color.FromHex ("#ddddff");
			Font = Font.SystemFontOfSize (NamedSize.Large);
//			HorizontalOptions = LayoutOptions.FillAndExpand;
//			HorizontalOptions = LayoutOptions.CenterAndExpand;
		}

		public RayvButton (string text) : this ()
		{
			Text = text;
		}
	}

	public class ButtonWide : Button
	{
		public ButtonWide () : base ()
		{
			HorizontalOptions = LayoutOptions.FillAndExpand;
			VerticalOptions = LayoutOptions.CenterAndExpand;
		}
	}

	public class LabelWide : Label
	{
		public LabelWide () : base ()
		{
			HorizontalOptions = LayoutOptions.FillAndExpand;
			VerticalOptions = LayoutOptions.CenterAndExpand;
		}
	}

	public class LabelWithData : Label
	{
		public string Data;

		public LabelWithData () : base ()
		{
			Data = "";
		}
	}
}

