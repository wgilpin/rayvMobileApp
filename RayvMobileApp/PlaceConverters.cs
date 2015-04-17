using System;
using Xamarin.Forms;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using Xamarin;

namespace RayvMobileApp
{
	
	public class BooleanToNotConverter: IValueConverter
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
			throw new NotImplementedException ();
		}
	}


	public class VoteToColorConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				string vote = (value as string);
				if (vote == null) {
					return Color.Gray;
				}
				if (vote == "-1")
					return Color.FromHex ("A22");
				if (vote == "1") {
					return ColorUtil.Darker (settings.BaseColor);
				}
				return settings.BaseColor;
			} catch (Exception ex) {
				Insights.Report (ex);
				return Color.Gray;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}

	// true if abs(down) > 0
	public class KeyToShowDownBoolConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				string key = (value as string);
				if (key == null) {
					return false;
				}
				Place p = Persist.Instance.GetPlace (key);
				if (p.iVoted)
					return false;
				return p.down != 0;
			} catch (Exception ex) {
				Insights.Report (ex);
				Console.WriteLine ("KeyToShowDownBoolConverter Exception");
				return false;
			}
				
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}




	// if up = 1, give the name. Else the number
	public class KeyToUpVotersConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				string key = (value as string);
				if (key == null) {
					return null;
				}
				Place p = Persist.Instance.GetPlace (key);
				if (p == null)
					return null;
				if (p.up == 0) {
					if (p.down != 1)
						return "No likes";
					return "";
				}
				if (p.up != 1)
					return String.Format ("{0} liked", p.up);
				Vote vote = (from v in Persist.Instance.Votes
				             where v.key == key
				                 && !string.IsNullOrWhiteSpace (v.VoterName)
				             select v).FirstOrDefault ();
				if (vote != null)
					return String.Format ("{0}\nlikes", vote.VoterName);
				return "1 liked";
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}

	// if up = 1, give the name. Else the number
	public class KeyToDownVotersConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				string key = (value as string);
				if (key == null) {
					return null;
				}
				Place p = Persist.Instance.GetPlace (key);
				if (p.down != 1 || p.up > 0)
					return String.Format ("{0} disliked", p.down);
				Vote vote = (from v in Persist.Instance.Votes
				             where v.key == key
				                 && v.VoterName.Length > 0
				             select v).FirstOrDefault ();
				if (vote != null)
					return String.Format ("{0}\ndislikes", vote.VoterName);
				return "1 disliked";
			} catch (Exception ex) {
				Insights.Report (ex);
				return null;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
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

