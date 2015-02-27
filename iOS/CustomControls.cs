using System;
using Xamarin.Forms;
using System.Text;

namespace RayvMobileApp.iOS
{
	public class LabelWithChangeButton: Grid
	{
		private Button ButtonChange;
		Label ButtonLabel;
		private TapGestureRecognizer ClickLabel;


		public EventHandler OnClick {
			get { return null; }
			set { 
				//the button
				ButtonChange.Clicked += value; 
				//the label
				ClickLabel = new TapGestureRecognizer ();
				ClickLabel.Tapped += value;
				ButtonLabel.GestureRecognizers.Add (ClickLabel);
			}
		}

		public string Text {
			get { return ButtonLabel.Text; }
			set { ButtonLabel.Text = value; }
		}

		public string ButtonText {
			get { return ButtonChange.Text; }
			set { ButtonChange.Text = value; }
		}

		public LabelWithChangeButton () : base ()
		{
			ButtonChange = new Button {
				Text = "Change",
				HorizontalOptions = LayoutOptions.End,
			};
			ButtonChange.HeightRequest = 20;
			ButtonLabel = new Label {
				Text = "",
				TextColor = Color.FromHex ("#666"),
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.Start,
			};

			Padding = new Thickness (5, 5, 2, 5);
			HorizontalOptions = LayoutOptions.StartAndExpand;
			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			Children.Add (ButtonLabel, 0, 1, 0, 1);
			Children.Add (ButtonChange, 1, 2, 0, 1);
		}
	}

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


