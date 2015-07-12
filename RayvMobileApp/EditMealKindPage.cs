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

	public class EditPlaceKindPage : ContentPage
	{
		MealKind _kind;
		PlaceStyle _style;
		CheckBox breakfast;
		CheckBox lunch;
		CheckBox dinner;
		CheckBox coffee;
		bool InFlow;
		LabelClickable StyleQuickLbl;
		LabelClickable StyleRelaxedLbl;
		LabelClickable StylefancyLbl;

		const string STYLE_FANCY = "fancy",
			STYLE_QUICK = "quick bite",
			STYLE_RELAXED = "relaxed";


		public event EventHandler<KindSavedEventArgs> Saved;
		public event EventHandler Cancelled;

		protected virtual void OnSaved ()
		{
			if (breakfast.Checked)
				_kind = MealKind.Breakfast;
			if (lunch.Checked)
				_kind = _kind | MealKind.Lunch;
			if (dinner.Checked)
				_kind = _kind | MealKind.Dinner;
			if (coffee.Checked)
				_kind = _kind | MealKind.Coffee;
			if (Saved != null)
				Saved (this, new KindSavedEventArgs (_kind, _style));
		}

		public void DoClickKind (object sender, EventArgs e)
		{
			_kind = MealKind.None;
			if (sender is LabelClickable) {
				var lbl = (sender as LabelClickable);
				switch (lbl.Label.Text) {
					case "breakfast":
						breakfast.Checked = true;
						break;
					case "lunch":
						lunch.Checked = true;
						break;
					case "dinner":
						dinner.Checked = true;
						break;
					case "coffee":
						coffee.Checked = true;
						break;
				}
			}

		}

		public void DoClickStyleFancy (object sender, EventArgs e)
		{
			// at least one kind?
			if (breakfast.Checked || lunch.Checked || dinner.Checked || coffee.Checked) {
				_style = PlaceStyle.Fancy;
				OnSaved ();
			} else
				DisplayAlert ("Kind?", "You must check at least one meal kind", "OK");
		}

		public void DoClickStyleRelaxed (object sender, EventArgs e)
		{
			// at least one kind?
			if (breakfast.Checked || lunch.Checked || dinner.Checked || coffee.Checked) {
				_style = PlaceStyle.Relaxed;
				OnSaved ();
			} else
				DisplayAlert ("Kind?", "You must check at least one meal kind", "OK");
		}

		public void DoClickStyleQuick (object sender, EventArgs e)
		{
			// at least one kind?
			if (breakfast.Checked || lunch.Checked || dinner.Checked || coffee.Checked) {
				_style = PlaceStyle.QuickBite;
				OnSaved ();
			} else
				DisplayAlert ("Kind?", "You must check at least one meal kind", "OK");
		}

		void SetupStyles ()
		{
			StyleQuickLbl.Label.TextColor = (_style == PlaceStyle.QuickBite) ? Color.White : Color.Black;
			StyleQuickLbl.SetBackgroundColor ((_style == PlaceStyle.QuickBite) ? settings.BaseColor : Color.Transparent);
			StyleRelaxedLbl.Label.TextColor = (_style == PlaceStyle.Relaxed) ? Color.White : Color.Black;
			StyleRelaxedLbl.SetBackgroundColor ((_style == PlaceStyle.Relaxed) ? settings.BaseColor : Color.Transparent);
			StylefancyLbl.Label.TextColor = (_style == PlaceStyle.Fancy) ? Color.White : Color.Black;
			StylefancyLbl.SetBackgroundColor ((_style == PlaceStyle.Fancy) ? settings.BaseColor : Color.Transparent);
		}

		public EditPlaceKindPage (MealKind kind, PlaceStyle style, bool inFlow = true)
		{
			_kind = kind;
			_style = style;
			InFlow = inFlow;

			BackgroundColor = Color.White;
			Grid grid = new Grid {
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
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (20) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (30) },
				} 
			};
			Label MealType = new Label { 
				BackgroundColor = ColorUtil.Darker (settings.BaseColor), 
				Text = "Meal Kind", 
				TextColor = Color.White,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label))
			};
			breakfast = new CheckBox{ OnClick = DoClickKind, Checked = ((MealKind.Breakfast & kind) > 0) };
			var breakfastVoteLbl = new LabelClickable{ OnClick = DoClickKind };
			breakfastVoteLbl.Label.Text = "breakfast";
			lunch = new CheckBox{ OnClick = DoClickKind, Checked = ((MealKind.Lunch & kind) > 0)  };
			var lunchVoteLbl = new LabelClickable{ OnClick = DoClickKind };
			lunchVoteLbl.Label.Text = "lunch";
			dinner = new CheckBox{ OnClick = DoClickKind, Checked = ((MealKind.Dinner & kind) > 0)  };
			var dinnerVoteLbl = new LabelClickable{ OnClick = DoClickKind };
			dinnerVoteLbl.Label.Text = "dinner";
			coffee = new CheckBox{ OnClick = DoClickKind, Checked = ((MealKind.Coffee & kind) > 0)  };
			var coffeeVoteLbl = new LabelClickable{ OnClick = DoClickKind };
			coffeeVoteLbl.Label.Text = "coffee";
			grid.Children.Add (MealType, 0, 3, 0, 1);
			grid.Children.Add (breakfastVoteLbl, 1, 2, 1, 2);
			grid.Children.Add (breakfast, 2, 3, 1, 2);
			grid.Children.Add (lunchVoteLbl, 1, 2, 2, 3);
			grid.Children.Add (lunch, 2, 3, 2, 3);
			grid.Children.Add (dinnerVoteLbl, 1, 2, 3, 4);
			grid.Children.Add (dinner, 2, 3, 3, 4);
			grid.Children.Add (coffeeVoteLbl, 1, 2, 4, 5);
			grid.Children.Add (coffee, 2, 3, 4, 5);

			Label PlaceType = new Label { 
				BackgroundColor = ColorUtil.Darker (settings.BaseColor), 
				Text = "Style", 
				TextColor = Color.White,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label))
			};
			grid.Children.Add (PlaceType, 0, 3, 5, 6);
			StyleQuickLbl = new LabelClickable { 
				OnClick = DoClickStyleQuick, 
			};
			StyleQuickLbl.Label.Text = STYLE_QUICK;
			StyleQuickLbl.Label.YAlign = TextAlignment.Center;
			var StyleQuickImgBtn = new ImageButton {
				Height = 20,
				Source = settings.DevicifyFilename ("arrow.png"), 
				OnClick = DoClickStyleQuick
			};
			StyleRelaxedLbl = new LabelClickable { 
				OnClick = DoClickStyleRelaxed, 
			};
			StyleRelaxedLbl.Label.YAlign = TextAlignment.Center;
			StyleRelaxedLbl.Label.Text = STYLE_RELAXED;
			var StyleRelaxedImgBtn = new ImageButton {
				Height = 20,
				Source = settings.DevicifyFilename ("arrow.png"), 
				OnClick = DoClickStyleRelaxed 
			};
			StylefancyLbl = new LabelClickable { 
				OnClick = DoClickStyleFancy, 
			};
			StylefancyLbl.Label.Text = STYLE_FANCY;
			StylefancyLbl.Label.YAlign = TextAlignment.Center;
			var StylefancyImgBtn = new ImageButton {
				Height = 20,
				Source = settings.DevicifyFilename ("arrow.png"), 
				OnClick = DoClickStyleFancy 
			};
			grid.Children.Add (
				StyleQuickLbl, 1, 2, 6, 7);
			grid.Children.Add (StyleQuickImgBtn, 2, 3, 6, 7);
			grid.Children.Add (StyleRelaxedLbl, 1, 2, 7, 8);
			grid.Children.Add (StyleRelaxedImgBtn, 2, 3, 7, 8);
			grid.Children.Add (StylefancyLbl, 1, 2, 8, 9);
			grid.Children.Add (StylefancyImgBtn, 2, 3, 8, 9);
			Content = grid;

			if (!(kind == MealKind.None || style == PlaceStyle.None)) {
				ToolbarItems.Add (new ToolbarItem {
					Text = InFlow ? " Next " : " Cancel ",
					//				Icon = "187-pencil@2x.png",
					Order = ToolbarItemOrder.Primary,
					Command = new Command (() => { 
						if (InFlow)
							OnSaved ();
						else
							Cancelled?.Invoke (this, null);
					})
				});
			}
			Appearing += (sender, e) => {
				SetupStyles ();
			};
		}
	}
}


