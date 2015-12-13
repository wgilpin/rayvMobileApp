using System;
using System.Linq;

namespace RayvMobileApp
{
	public class VoteComment
	{
		public long CommentId { get; set; }

		public string Vote { get; set; }

		public string Author { get; set; }

		public string Comment{ get; set; }

		public DateTime When { get; set; }

		public VoteComment ()
		{
			When = DateTime.UtcNow;
		}

		public string GetVoterName ()
		{
			if (Author == Persist.Instance.MyId.ToString ())
				return "Me";
			return Persist.Instance.Friends [Author].Name;
		}

		public string PrettyComment {
			get {
				if (Comment != null && Comment.Length > 0)
					return String.Format ("\"{0}\"", Comment);
				return "";
			}
		}

		String MakeString (Double n, String unit)
		{
			int intn = (int)Math.Truncate (n);
			string plural = "";
			if (intn > 1)
				plural = "s";
			return String.Format ("{0} {1}{2} ago", intn, unit, plural);
		}

		public string PrettyHowLongAgo ()
		{
			// e.g. "A few seconds ago","10 mins ago", "2 hours ago", "24 days ago"
			TimeSpan d = DateTime.UtcNow - When;
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
}

