using System;
using SQLite;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	public class Vote: IComparable<Vote>
	{
		#region sqlLite columns

		[PrimaryKey]
		public string key { get; set; }

		public string voter { get; set; }

		public string place_name { get; set; }

		public int vote { get; set; }

		public bool untried { get; set; }

		public string comment { get; set; }

		public DateTime when { get; set; }

		#endregion

		#region properties

		[Ignore]
		public string VoterName {
			get {
				return Persist.Instance.Friends [voter];
			}
		}

		[Ignore]
		public ImageSource PlaceImage {
			get {
				return Persist.Instance.GetPlace (key).thumb_url;
			}
		}

		[Ignore]
		public string GetIconName {
			get {
				String imageUrl = "";
				if (vote == 1)
					imageUrl = "heart-lg.png";
				if (vote == -1)
					imageUrl = "no-entry-lg.png";
				if (untried)
					imageUrl = "star-lg.png";
				return imageUrl;
			}
		}

		[Ignore]
		public string PrettyComment {
			get {
				if (comment != null && comment.Length > 0)
					return String.Format ("\"{0}\"", comment);
				return "";
			}
		}

		[Ignore]
		public string PrettyHowLongAgo {
			// e.g. "A few seconds ago","10 mins ago", "2 hours ago", "24 days ago"
			get {
				TimeSpan d = DateTime.UtcNow - when;
				if (d.TotalDays > 1.0) {
					// days
					return MakeString (d.TotalDays, "days");
				}
				if (d.TotalHours > 1.0) {
					// hours
					return MakeString (d.TotalHours, "hours");
				}
				if (d.TotalMinutes > 1.0) {
					// days
					return MakeString (d.TotalMinutes, "minutes");
				}
				//seconds
				return "a few seconds ago";
			}
		}

		#endregion

		String MakeString (Double n, String unit)
		{
			return String.Format ("{0} {1} ago", Math.Truncate (n), unit);
		}

		// Default comparer for Vote type.
		public int CompareTo (Vote compareVote)
		{
			// A null value means that this object is lesser. 
			if (compareVote == null)
				return 1;
			else
				return -this.when.CompareTo (compareVote.when);
		}

	}
}

