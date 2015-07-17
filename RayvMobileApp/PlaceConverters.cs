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
				Vote vote = (value as Vote);
				if (vote?.vote == VoteValue.Disliked)
					return Color.FromHex ("A22");
				if (vote?.vote == VoteValue.Liked) {
					return ColorUtil.Darker (settings.BaseColor);
				}
				return Color.Gray;
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


	// do we show my vote
	public class KeyToShowMyVoteConverter: IValueConverter
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
					// I voted so we show it
					return true;
				if (p.up > 0 || p.down > 0)
					// I didn't vote but others did so we don't show my vote
					return false;
				// I didn't vote, nor did anyone, so we show that
				return true;
			} catch (Exception ex) {
				Insights.Report (ex);
				Console.WriteLine ("KeyToShowMyVoteConverter Exception");
				return false;
			}

		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}

	// what to show for my vote
	public class KeyToMyVoteTextConverter: IValueConverter
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
					// I voted so we show it
					return p.voteAsText;
				if (p.up > 0 || p.down > 0)
					// I didn't vote but others did so we don't show my vote
					return false;
				// I  didn't vote, nor did anyone, so we show that
				var votes = from v in Persist.Instance.Votes
				            where v.key == key
				                && v.untried
				                && !string.IsNullOrWhiteSpace (v.VoterName)
				            select v;
				int count = votes.Count ();
				if (count == 1) {
					Vote vote = votes.FirstOrDefault ();
					return String.Format ("{0}\nsaved", vote.VoterName);
				} else
					return String.Format ("{0} saved", count);
			} catch (Exception ex) {
				Insights.Report (ex);
				Console.WriteLine ("KeyToMyVoteTextConverter Exception");
				return false;
			}

		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}

	// what to show for my vote
	public class KeyToMyVoteSizeConverter: IValueConverter
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
					// I voted so we show it
					return Device.GetNamedSize (NamedSize.Default, typeof(Label));
				return Device.GetNamedSize (NamedSize.Small, typeof(Label));

			} catch (Exception ex) {
				Insights.Report (ex);
				Console.WriteLine ("KeyToMyVoteTextConverter Exception");
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
					return "";
				}
				if (p.up != 1)
					return String.Format ("{0} liked", p.up);
				var myId = Persist.Instance.MyId.ToString ();
				Vote vote = (from v in Persist.Instance.Votes
				             where v.key == key
				                 && v.voter != myId
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
				Console.WriteLine (key);
				if (key == null) {
					return null;
				}
				Place p = Persist.Instance.GetPlace (key);
				if (p.down != 1 || p.up > 0)
					return String.Format ("{0} disliked", p.down);
				Vote vote = (from v in Persist.Instance.Votes
				             where v.key == key
				                 && v.vote == VoteValue.Disliked
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

