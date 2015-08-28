using System;
using Xamarin.Forms;
using System.Globalization;
using Xamarin;
using System.Diagnostics;
using System.Text;

namespace RayvMobileApp
{
	public class FriendToFirstCharConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var name = value as string;
				if (string.IsNullOrEmpty (name))
					return null;
				return name.Remove (1);
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			} 
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "FriendToFirstCharConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

	public class FriendToRandomColorConverter: IValueConverter
	{
		public static Color stringToColor (string name)
		{
			if (name.Length > 3) {
				int i1 = ((Encoding.ASCII.GetBytes (name) [0] - 97) % 26) * 10;
				int i2 = ((Encoding.ASCII.GetBytes (name) [1] - 97) % 26) * 10;
				int i3 = ((Encoding.ASCII.GetBytes (name) [2] - 97) % 26) * 10;
				Color c = Color.FromRgb (i1, i2, i3);
				Console.WriteLine ("{0} {1}", name, c);
				return c;
			}
			return Color.Black;
		}

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var name = value as string;
			if (string.IsNullOrEmpty (name))
				return null;
			try {
				name = name.ToLower ();
				return stringToColor (name);
			} catch (Exception ex) {
				Insights.Report (ex);
				return Color.Black;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "FriendToRandomColorConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}
}

