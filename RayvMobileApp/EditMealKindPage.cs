using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;

namespace RayvMobileApp
{
	


	public class KindSavedEventArgs : EventArgs
	{

		public KindSavedEventArgs (MealKind kind, PlaceStyle style)
		{
			Kind = kind;
			Style = style;
		}

		public MealKind Kind {
			get ;
			private set;
		}

		public PlaceStyle Style {
			get ;
			private set;
		}
	}

	public class EditPlaceKindView : Grid
	{
		MealKind _kind;
		PlaceStyle _style;
		bool InFlow;

		CheckBox[] checks;

		const string STYLE_FANCY = "fancy",
			STYLE_QUICK = "quick bite",
			STYLE_RELAXED = "relaxed";


		public event EventHandler<KindSavedEventArgs> Saved;
		public event EventHandler Cancelled;
		public event EventHandler<EventArgsMessage> ShowMessage;

		protected virtual void OnSaved ()
		{
			if (_kind == MealKind.None || _style == PlaceStyle.None) {
				ShowMessage?.Invoke (this, new EventArgsMessage ("You must select a style and a meal time"));
			} else {
				if (Saved != null)
					Saved (this, new KindSavedEventArgs (_kind, _style));
			}
		}

		public void DoClickStyleFancy (object sender, EventArgs e)
		{
			// at least one kind?
			if (_kind != MealKind.None) {
				_style = PlaceStyle.Fancy;
				OnSaved ();
			} else
				ShowMessage?.Invoke (this, new EventArgsMessage ("You must check at least one meal kind"));
		}

		public void DoClickStyleRelaxed (object sender, EventArgs e)
		{
			// at least one kind?
			if (_kind != MealKind.None) {
				_style = PlaceStyle.Relaxed;
				OnSaved ();
			} else
				ShowMessage?.Invoke (this, new EventArgsMessage ("You must check at least one meal kind"));
		}

		public void DoClickStyleQuick (object sender, EventArgs e)
		{
			// at least one kind?
			if (_kind != MealKind.None) {
				_style = PlaceStyle.QuickBite;
				OnSaved ();
			} else
				ShowMessage?.Invoke (this, new EventArgsMessage ("You must check at least one meal kind"));
		}

		void SetUpGrid ()
		{
			RowSpacing = 10;
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (1, GridUnitType.Auto)
			});
			ColumnDefinitions.Add (new ColumnDefinition {
				Width = new GridLength (20)
			});
			ColumnDefinitions.Add (new ColumnDefinition {
				Width = new GridLength (1, GridUnitType.Star)
			});
			ColumnDefinitions.Add (new ColumnDefinition {
				Width = new GridLength (30)
			});
		}

		void AddMealTimeButton (EventHandler onClick, MealKind value, string text, int row)
		{
			var chkBox = new CheckBox{ OnClick = onClick, Checked = ((value & _kind) > 0) };
			checks [row - 1] = chkBox;
			var voteLbl = new LabelClickable{ OnClick = onClick };
			voteLbl.Label.Text = text;
			Children.Add (voteLbl, 1, row);
			Children.Add (chkBox, 2, row);
		}

		void AddPlaceStyleButton (EventHandler onClick, PlaceStyle value, string text, int row)
		{
			var lbl = new LabelClickable { 
				OnClick = onClick, 
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			lbl.Label.Text = text;
			lbl.Label.TextColor = (_style == value) ? Color.White : Color.Black;
			lbl.SetBackgroundColor ((_style == value) ? settings.BaseColor : Color.Transparent);
			lbl.Label.YAlign = TextAlignment.Center;
			var imgBtn = new ImageButton {
				Height = 20,
				Source = settings.DevicifyFilename ("arrow.png"), 
				OnClick = onClick
			};
			Children.Add (lbl, 1, row);
			Children.Add (imgBtn, 2, row);
		}

		void DoCheck (object sender, MealKind kind, int row)
		{
			var check = checks [row - 1];
			if (!(sender.GetType ().Equals (typeof(CheckBox))))
				// if the checkbox was clicked, it toggles. If label was clicked, we have to toggle
				check.Checked = !check.Checked;
			if (check.Checked)
				_kind = _kind | kind;
			else
				_kind = (MealKind)((int)_kind & ~(int)kind);
		}

		public EditPlaceKindView (MealKind kind, PlaceStyle style, bool inFlow = true)
		{
			_kind = kind;
			_style = style;
			InFlow = inFlow;

			checks = new CheckBox[5];
			BackgroundColor = Color.White;
			SetUpGrid ();
			Label MealType = new  Label { 
				BackgroundColor = ColorUtil.Darker (settings.BaseColor), 
				Text = "Meal Time", 
				TextColor = Color.White,
				FontSize = settings.FontSizeLabelLarge
			};
			// meal time buttons
			AddMealTimeButton ((s, e) => {
				DoCheck (s, MealKind.Breakfast, 1);
			}, MealKind.Breakfast, "breakfast", 1);
			AddMealTimeButton ((s, e) => {
				DoCheck (s, MealKind.Lunch, 2);
			}, MealKind.Lunch, "lunch", 2);
			AddMealTimeButton ((s, e) => {
				DoCheck (s, MealKind.Coffee, 3);
			}, MealKind.Coffee, "coffee", 3);
			AddMealTimeButton ((s, e) => {
				DoCheck (s, MealKind.Dinner, 4);
			}, MealKind.Dinner, "dinner", 4);
			AddMealTimeButton ((s, e) => {
				DoCheck (s, MealKind.Bar, 5);
			}, MealKind.Bar, "bar", 5);

			Label PlaceType = new Label { 
				BackgroundColor = ColorUtil.Darker (settings.BaseColor), 
				Text = "Style", 
				TextColor = Color.White,
				FontSize = settings.FontSizeLabelLarge
			};
			Children.Add (PlaceType, 0, 3, 6, 7);
			AddPlaceStyleButton (DoClickStyleQuick, PlaceStyle.QuickBite, STYLE_QUICK, 7);
			AddPlaceStyleButton (DoClickStyleRelaxed, PlaceStyle.Relaxed, STYLE_RELAXED, 8);
			AddPlaceStyleButton (DoClickStyleFancy, PlaceStyle.Fancy, STYLE_FANCY, 9);

			var buttons = new DoubleButton { 
				LeftText = "Back", 
				LeftSource = "298-circlex@2x.png",
				RightText = "Next",
				RightSource = "Add Select right button.png"
			};
			if (style == PlaceStyle.None) {
				buttons.IsEnabledRight = false;
			}
			buttons.LeftClick = (s, e) => Cancelled?.Invoke (this, null);
			buttons.RightClick = (s, e) => {
				if (InFlow)
					OnSaved ();
				else
					Cancelled?.Invoke (this, null);
			};
			Children.Add (buttons, 0, 3, 10, 11);
		}
	}
}


