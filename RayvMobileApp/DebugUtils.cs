using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class DebugUtils
	{
		public static void AddLinesToGrid (Grid g)
		{
			for (int r = 0; r < g.RowDefinitions.Count; r++) {
				for (int c = 0; c < g.ColumnDefinitions.Count; c++) {
					var box = new BoxView (){ BackgroundColor = Color.FromRgb (r * 40, c * 40, 255) };
					g.Children.Add (box, c, r);
				}
			}
		}

		public DebugUtils ()
		{
		}
	}
}

