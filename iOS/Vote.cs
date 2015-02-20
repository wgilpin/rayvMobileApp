using System;
using SQLite;
using Xamarin.Forms;
using System.Text;
using Xamarin;

namespace RayvMobileApp.iOS
{
	public class Vote: IComparable<Vote>
	{

		#region sqlLite columns

		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

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
		public string IsSynced {
			get;
			set;
		}

		[Ignore]
		public string VoterName {
			get {
				return Persist.Instance.Friends [voter].Name;
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
		public string VoteVerb {
			get {
				if (vote == 1)
					return "Liked";
				if (vote == -1)
					return "Disliked";
				return "Starred";
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
			}
		}

		[Ignore]
		public Color RandomColor {
			get {
				try {
					string name = Persist.Instance.Friends [voter].Name.ToLower ();
					if (name.Length > 3) {
						int i1 = ((Encoding.ASCII.GetBytes (name) [0] - 97) % 26) * 10;
						int i2 = ((Encoding.ASCII.GetBytes (name) [1] - 97) % 26) * 10;
						int i3 = ((Encoding.ASCII.GetBytes (name) [2] - 97) % 26) * 10;
						Color c = Color.FromRgb (i1, i2, i3);
						Console.WriteLine ("{0} {1}", name, c);
						return c;
					}
					return Color.Black;
				} catch (Exception ex) {
					Insights.Report (ex);
					return Color.Black;
				}
			}
		}

		[Ignore]
		public string FirstLetter {
			get {
				try {
					return Persist.Instance.Friends [voter].Name.Remove (1); 
				} catch (Exception ex) {
					return "?";
				}
			}
		}

		#endregion

		String MakeString (Double n, String unit)
		{
			int intn = (int)Math.Truncate (n);
			string plural = "";
			if (intn > 1)
				plural = "s";
			return String.Format ("{0} {1}{2} ago", intn, unit, plural);
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

