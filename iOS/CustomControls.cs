﻿using System;
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
			BackgroundColor = Color.FromHex ("#ddddff");
			Font = Font.SystemFontOfSize (NamedSize.Large);
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


