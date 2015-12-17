using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;
using System.Linq;

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

	public class EditPlaceKindView : StackLayout
	{
		MealKind _kind;
		PlaceStyle _style;
		bool InFlow;
		Grid grid;

		CheckBox[] checks;
		Dictionary<string, PlaceStyle> Styles;
		Picker StylePicker;
		DoubleImageButton buttons;




		public event EventHandler<KindSavedEventArgs> Saved;
		public event EventHandler Cancelled;
		public event EventHandler<EventArgsMessage> ShowMessage;

		protected virtual void OnSaved ()
		{
			if (_kind == MealKind.None || _style == PlaceStyle.None) {
				ShowMessage?.Invoke (this, new EventArgsMessage ("You must select a price and a meal time"));
			} else {
				if (Saved != null)
					Saved (this, new KindSavedEventArgs (_kind, _style));
			}
		}

		public bool BothValuesSet ()
		{
			// at least one kind?
			if (StylePicker.SelectedIndex > 0) {
				// 0 = None
				if (_kind != MealKind.None) {
					_style = Styles [StylePicker.Items [StylePicker.SelectedIndex]];
					return true;
				}
			}
			return false;
		}

		void SetUpGrid ()
		{
			grid = new Grid { 
				VerticalOptions = LayoutOptions.StartAndExpand,
				RowSpacing = 10,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) }
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (20) },
					new ColumnDefinition {
						Width = new GridLength (1, GridUnitType.Star)
					},
					new ColumnDefinition {
						Width = new GridLength (30)
					}
				}
			};
		}

		void AddMealTimeButton (EventHandler onClick, MealKind value, string text, int row)
		{
			var chkBox = new CheckBox{ OnClick = onClick, Checked = ((value & _kind) > 0) };
			checks [row - 1] = chkBox;
			var voteLbl = new LabelClickable{ OnClick = onClick };
			voteLbl.Label.Text = text;
			grid.Children.Add (voteLbl, 1, row);
			grid.Children.Add (chkBox, 2, row);
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
			grid.Children.Add (lbl, 1, row);
			grid.Children.Add (imgBtn, 2, row);
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

		void DoStyleChanged (object sender, EventArgs e)
		{
			_style = Styles [StylePicker.Items [StylePicker.SelectedIndex]];
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
			grid.Children.Add (MealType, 0, 3, 0, 1);
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
				Text = "Price", 
				TextColor = Color.White,
				FontSize = settings.FontSizeLabelLarge
			};
			grid.Children.Add (PlaceType, 0, 3, 6, 7);

			Styles = new Dictionary<string, PlaceStyle> () {
				{ "select price",PlaceStyle.None },
				{ Vote.STYLE_QUICK,PlaceStyle.QuickBite },
				{ Vote.STYLE_RELAXED,PlaceStyle.Relaxed },
				{ Vote.STYLE_FANCY,PlaceStyle.Fancy },
			};
			StylePicker = new Picker ();
			foreach (var kvp in Styles) {
				StylePicker.Items.Add (kvp.Key);
			}

			grid.Children.Add (StylePicker, 1, 2, 7, 8);
			StylePicker.SelectedIndex = Styles.Values.ToList ().IndexOf (_style);
			StylePicker.SelectedIndexChanged += DoStyleChanged;

			buttons = new DoubleImageButton { 
				LeftText = "Back", 
				RightText = "Next",
				LeftSource = "back_1.png",
				RightSource = "forward_1.png"
			};
			buttons.LeftClick = (s, e) => Cancelled?.Invoke (this, null);
			buttons.RightClick = (s, e) => {
				if (inFlow) {
					if (BothValuesSet ())
						OnSaved ();
					else
						ShowMessage?.Invoke (this, new EventArgsMessage ("Needs both a meal time & a style"));
				} else
					Cancelled?.Invoke (this, null);
			};

			Children.Add (grid);
			Children.Add (buttons);
		}
	}
}


