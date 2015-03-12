using System;
using Xamarin.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RayvMobileApp.iOS
{
	#region Address-ShortAddress
	public class AddressToShortAddressConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var place = value as Place;
			if (place == null)
				return null;

			string res;
			// number then anything
			string pattern = @"^(\d+[-\d+]* )(.*)";
			MatchCollection matches = Regex.Matches (place.address, pattern);
			if (matches.Count < 1) {
				res = place.address;
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
	#endregion
}

