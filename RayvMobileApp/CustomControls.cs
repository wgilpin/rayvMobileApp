using System;
using Xamarin.Forms;
using System.Text;

namespace RayvMobileApp
{
	public class GridWithCounter: Grid
	{
		public int Row;
		public bool ShowGrid;

		public GridWithCounter AddPart (View View, int left, int right)
		{
			if (ShowGrid) {
				Frame frame = new Frame ();
				frame.Padding = 0;
				frame.HasShadow = false;
				frame.OutlineColor = Color.Aqua;
				frame.Content = View;
				Children.Add (frame, left, right, Row, Row + 1);
			} else
				Children.Add (View, left, right, Row, Row + 1);
			return this;
		}

		public GridWithCounter AddRow (View View)
		{
			if (ShowGrid) {
				Frame frame = new Frame ();
				frame.Padding = 0;
				frame.HasShadow = false;
				frame.OutlineColor = Color.Aqua;
				frame.Content = View;
				Children.Add (frame, 0, ColumnDefinitions.Count, Row, Row + 1);
			} else
				Children.Add (View, 0, ColumnDefinitions.Count, Row, Row + 1);
			return this;
		}

		public void NextRow ()
		{
			Row++;
		}

		public GridWithCounter () : base ()
		{
			Row = 0;
			ShowGrid = false;
		}
	}

	public class LabelClickable: Label
	{
		TapGestureRecognizer _click;

		public EventHandler OnClick {
			get { return null; }
			set { _click.Tapped += value; }
		}

		public LabelClickable () : base ()
		{
			_click = new TapGestureRecognizer ();
			this.GestureRecognizers.Add (_click);
		}
	}

	public class CheckBox: Image
	{
		bool _checked;
		TapGestureRecognizer _clickImage;

		public EventHandler OnClick {
			get { return null; }
			set { _clickImage.Tapped += value; }
		}

		private void  DoCLicked (object sender, EventArgs e)
		{
			Checked = !Checked;
		}

		public bool Checked {
			get { return _checked; }
			set { 
				_checked = value;
				this.Source = _checked ? "checkbox_checked.png" : "checkbox_unchecked.png";
			}
		}

		public CheckBox () : base ()
		{
			Aspect = Aspect.AspectFit;
			_clickImage = new TapGestureRecognizer ();
			this.GestureRecognizers.Add (_clickImage);
			_clickImage.Tapped += DoCLicked;
			Checked = false;
		}
	}

	public class RayvNav: NavigationPage
	{
		public RayvNav (Page p) : base (p)
		{
			BarBackgroundColor = settings.BaseColor;
			BarTextColor = Color.White;
		}
	}

	public class EntryWithButton : Grid
	{
		Entry _entry;
		Image _img;
		TapGestureRecognizer _clickImage;

		public String Placeholder { set { _entry.Placeholder = value; } }

		public String Source { set { _img.Source = value; } }

		public String Text {
			get { return _entry.Text; }
			set { _entry.Text = value; }
		}

		public Entry TextEntry {
			get { return _entry; }
		}

		public EventHandler OnClick {
			get { return null; }
			set { _clickImage.Tapped += value; }
		}

		public EntryWithButton () : base ()
		{
			Padding = 2;
			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (25.0) });
			HorizontalOptions = LayoutOptions.FillAndExpand;
			_entry = new Entry { TextColor = Color.Black };
			_img = new Image ();
			_clickImage = new TapGestureRecognizer ();
			_img.GestureRecognizers.Add (_clickImage);
			Children.Add (_entry, 0, 0);
			Children.Add (_img, 1, 0);
		}

		public EntryWithButton (string placeholder, string imageSource) : this ()
		{
			_entry.Placeholder = placeholder;
			_img.Source = imageSource;
		}
	}

	public class LabelWithImageButton : Grid
	{
		Label _label;
		Image _img;
		TapGestureRecognizer _clickImage;

		public String Source { set { _img.Source = value; } }

		public String Text {
			get { return _label.Text; }
			set { _label.Text = value; }
		}

		public Color TextColor {
			get { return _label.TextColor; }
			set { _label.TextColor = value; }
		}

		public Label Label {
			get { return _label; }
		}

		public EventHandler OnClick {
			get { return null; }
			set { _clickImage.Tapped += value; }
		}

		public TextAlignment XAlign {
			get { return _label.XAlign; }
			set { _label.XAlign = value; }
		}

		public FontAttributes FontAttributes {
			get { return _label.FontAttributes; }
			set { _label.FontAttributes = value; }
		}

		public LabelWithImageButton () : base ()
		{
			Padding = 2;
			RowDefinitions.Add (new RowDefinition { Height = new GridLength (5, GridUnitType.Absolute) });
			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			RowDefinitions.Add (new RowDefinition { Height = new GridLength (5) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (20.0) });
			HorizontalOptions = LayoutOptions.FillAndExpand;
			_label = new Label { TextColor = Color.Black, FontAttributes = FontAttributes.Bold };
			_img = new Image { Aspect = Aspect.AspectFit, HeightRequest = 20, };
			_clickImage = new TapGestureRecognizer ();
			_img.GestureRecognizers.Add (_clickImage);
			_label.GestureRecognizers.Add (_clickImage);
			Children.Add (_label, 0, 1);
			Children.Add (_img, 1, 1);
		}

		public LabelWithImageButton (string placeholder, string imageSource) : this ()
		{
			_img.Source = imageSource;
		}
	}

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
				TextColor = Color.White,
			};
			ButtonChange.HeightRequest = 20;
			ButtonLabel = new Label {
				Text = "",
				TextColor = Color.White,
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			Padding = new Thickness (5, 5, 2, 5);
			HorizontalOptions = LayoutOptions.StartAndExpand;
			RowDefinitions.Add (new RowDefinition { Height = new GridLength (20, GridUnitType.Absolute) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1000, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (60, GridUnitType.Absolute) });
			Children.Add (ButtonLabel, 0, 1, 0, 1);
			Children.Add (ButtonChange, 1, 2, 0, 1);
		}
	}

	public class EntryWithChangeButton: Grid
	{
		private Button ButtonChange;
		Entry TextEntry;
		private TapGestureRecognizer ClickLabel;


		public EventHandler OnClick {
			get { return null; }
			set { 
				//the button
				ButtonChange.Clicked += value; 
				//the label
				ClickLabel = new TapGestureRecognizer ();
				ClickLabel.Tapped += value;
			}
		}

		public string Text {
			get { return TextEntry.Text; }
			set { TextEntry.Text = value; }
		}

		public string PlaceHolder {
			get { return TextEntry.Placeholder; }
			set { TextEntry.Placeholder = value; }
		}

		public string ButtonText {
			get { return ButtonChange.Text; }
			set { ButtonChange.Text = value; }
		}

		public Entry Entry {
			get { return TextEntry; }
		}

		public EntryWithChangeButton () : base ()
		{
			ButtonChange = new Button {
				Text = "Change",
				HorizontalOptions = LayoutOptions.End,
				TextColor = Color.White,
				FontAttributes = FontAttributes.Bold,
			};
			ButtonChange.HeightRequest = 30;
			TextEntry = new Entry {
				Text = "",
				TextColor = Color.FromHex ("#666"),
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			Padding = new Thickness (5, 5, 2, 5);
			HorizontalOptions = LayoutOptions.StartAndExpand;
			RowDefinitions.Add (new RowDefinition { Height = new GridLength (30, GridUnitType.Absolute) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1000, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (60, GridUnitType.Absolute) });
			Children.Add (TextEntry, 0, 1, 0, 1);
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
			TextColor = Color.White;
			BackgroundColor = ColorUtil.Darker (settings.BaseColor);
			FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Button));
			if (text != null)
				Text = text;
//			HorizontalOptions = LayoutOptions.FillAndExpand;
//			HorizontalOptions = LayoutOptions.CenterAndExpand;
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
			BackgroundColor = Color.Transparent;
			TextColor = settings.BaseColor;
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
			TextColor = Color.Black;
			BackgroundColor = Color.Transparent;
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

		public Double Height {
			set { HeightRequest = value; }
		}

		public ImageButton (string source, EventHandler onClick) : this ()
		{
			Source = source;
			OnClick = onClick;
			VerticalOptions = LayoutOptions.Center;
			HorizontalOptions = LayoutOptions.Center;
		}
	}

	public class ButtonWithImage: Grid
	{
		private Button ButtonLeft;
		Image ImageRight;
		private TapGestureRecognizer ClickImage;


		public EventHandler OnClick {
			get { return null; }
			set { 
				//the button
				ButtonLeft.Clicked += value; 
				//the label
				ClickImage = new TapGestureRecognizer ();
				ClickImage.Tapped += value;
				ImageRight.GestureRecognizers.Add (ClickImage);
			}
		}

		public string Text {
			get { return ButtonLeft.Text; }
			set { ButtonLeft.Text = value; }
		}

		public ImageSource ImageSource {
			set { ImageRight.Source = value; }
		}

		public  ButtonWithImage () : base ()
		{
			ButtonLeft = new Button {
				Text = "Change",
				HorizontalOptions = LayoutOptions.End,
				TextColor = Color.White,
			};
			ButtonLeft.HeightRequest = 30;


			Padding = new Thickness (5, 5, 2, 5);
			HorizontalOptions = LayoutOptions.StartAndExpand;
			RowDefinitions.Add (new RowDefinition { Height = new GridLength (20, GridUnitType.Absolute) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1000, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (60, GridUnitType.Absolute) });
			Children.Add (ButtonLeft, 0, 1, 0, 1);
			Children.Add (ImageRight, 1, 2, 0, 1);
		}
	}

}


