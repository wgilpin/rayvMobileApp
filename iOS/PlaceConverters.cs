using System;
using Xamarin.Forms;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{
	public class AddressToShortAddressConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
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
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine (value.ToString (), new []{ "AddressToShortAddressConverter.ConvertBack" });
			throw new NotImplementedException ();
		}
	}

}

