using System;
using Xamarin.Forms;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using Xamarin;

namespace RayvMobileApp
{
	
	public class BoolToNotBoolConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				Boolean? booleanValue = System.Convert.ToBoolean (value);
				if (booleanValue == null) {
					return false;
				}
				return !booleanValue;
			} catch (Exception ex) {
				Insights.Report (ex);
				return false;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ("BoolToNotBoolConverter");
		}
	}




	public class AddressToShortAddressConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var address = value as string;
				if (String.IsNullOrEmpty (address))
					return null;

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

}

