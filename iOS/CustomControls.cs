using System;
using Xamarin.Forms;
using System.Text;

namespace RayvMobileApp.iOS
{
	public class RayvButton : Button
	{
		public EventHandler OnClick {
			get { return null; }
			set { Clicked += value; }
		}

		public RayvButton (String text = null) : base ()
		{
			//BorderWidth = 2;
			//BorderColor = Color.FromHex ("#4444AA");
			BorderRadius = 0;
			BackgroundColor = Color.FromHex ("#ddddff");
			Font = Font.SystemFontOfSize (NamedSize.Large);
			if (text != null)
				Text = text;
//			HorizontalOptions = LayoutOptions.FillAndExpand;
//			HorizontalOptions = LayoutOptions.CenterAndExpand;
		}
	}



	public class ColorButtonFromName : Button
	{
		public static readonly BindableProperty NameProperty = BindableProperty.Create<ColorButtonFromName, String> (p => p.Name, "");
		Color _color;
		String _name;

		public String Name { 
			get { 
				return (String)GetValue (NameProperty); 
			}
			set {
				SetValue (NameProperty, value);
			}
		}

		public ColorButtonFromName () : base ()
		{
			WidthRequest = 30;
			Font = Font.SystemFontOfSize (NamedSize.Large);
			BorderRadius = 15;
			Text = "";
		}


	}

	public class ButtonWide : Button
	{
		public EventHandler OnClick {
			get { return null; }
			set { Clicked += value; }
		}

		public ButtonWide (String text = null) : base ()
		{
			if (text != null)
				Text = text;
			BorderRadius = 0;
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

		public LabelWide (string text) : this ()
		{
			Text = text;
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

	public class ImageButton : Image
	{
		TapGestureRecognizer Gesture;

		public EventHandler OnClick {
			get { return null; }
			set { 
				if (Gesture == null) {
					Gesture = new TapGestureRecognizer ();
					GestureRecognizers.Add (Gesture);
				}
				Gesture.Tapped += value; 
			}
		}

		public ImageButton () : base ()
		{

		}
	}
}


