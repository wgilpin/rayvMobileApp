using System;
using Xamarin.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xamarin;
using System.Text;

namespace RayvMobileApp
{
	#region Address-ShortAddress
	public class KeyToShortAddressConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var key = value as string;
				if (String.IsNullOrEmpty (key))
					return null;
				
				Place p = Persist.Instance.GetPlace (key);
				string address = p.address;
				string res;
				// number then anything
				string pattern = @"^(\d+[-\d+]* )(.*)";
				MatchCollection matches = Regex.Matches (address, pattern);
				if (matches.Count < 1) {
					res = address;
				} else {
					res = matches [0].Groups [2].ToString ();
				}
				return res;
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}



	public class VoterToNameConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var voter = value as string;
				if (string.IsNullOrEmpty (voter))
					return null;
				return Persist.Instance.Friends [voter].Name;
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

	public class VoterToFirstLetterConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var voter = value as string;
				if (string.IsNullOrEmpty (voter))
					return null;
				return Persist.Instance.Friends [voter].Name.Remove (1);
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			} 
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

	public class KeyToThumbUrlConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var key = value as string;
				if (string.IsNullOrEmpty (key))
					return null;
				return Persist.Instance.GetPlace (key).thumb_url;
			} catch (Exception ex) {
				Insights.Report (ex);			
				return null;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

	public class CommentToPrettyStringConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var comment = value as string;
				if (string.IsNullOrEmpty (comment))
					return "";
				return String.Format ("\"{0}\"", comment);
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

	public class WhenToPrettyStringConverter: IValueConverter
	{
		private String MakeString (Double n, String unit)
		{
			try {
				int intn = (int)Math.Truncate (n);
				string plural = "";
				if (intn > 1)
					plural = "s";
				return String.Format ("{0} {1}{2} ago", intn, unit, plural);
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			}
		}

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			DateTime? when;
			try {
				when = value as DateTime?;
				if (when == null) {
					return null;
				}
			

				TimeSpan d = DateTime.UtcNow - (DateTime)when;
				if (d.TotalDays > 1.0) {
					// days
					return MakeString (d.TotalDays, "day");
				}
				if (d.TotalHours > 1.0) {
					// hours
					return MakeString (d.TotalHours, "hour");
				}
				if (d.TotalMinutes > 1.0) {
					// days
					return MakeString (d.TotalMinutes, "min");
				}
				//seconds
				return "a few seconds ago";
			} catch (Exception ex) {
				Insights.Report (ex);
				return "";
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

	public class VoterToRandomColorConverter: IValueConverter
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
			var voter = value as string;
			if (string.IsNullOrEmpty (voter))
				return null;
			try {
				string name = Persist.Instance.Friends [voter].Name.ToLower ();
				return stringToColor (name);
			} catch (Exception ex) {
				Insights.Report (ex);
				return Color.Black;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

	#endregion
}

