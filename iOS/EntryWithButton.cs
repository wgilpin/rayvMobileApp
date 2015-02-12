using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
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
			RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
			ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (20.0) });
			HorizontalOptions = LayoutOptions.FillAndExpand;
			_entry = new Entry ();
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
}

