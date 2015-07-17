using System;
using SQLite;
using Xamarin.Forms;
using System.Text;
using Xamarin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace RayvMobileApp
{
	public enum VoteFilterWho
	{
		Mine,
		All,
		Chosen
	}

	public enum VoteFilterWhat
	{
		All,
		Like,
		Wish,
		Try
		// Wish plus Like
	}

	public enum VoteValue
	{
		None = 0,
		Liked = 1,
		Disliked = -1,
		Untried = 2
	}


	[Flags]
	public enum MealKind
	{
		None = 0x0,
		Breakfast = 0x1,
		Lunch = 0x2,
		Dinner = 0x4,
		Coffee = 0x8
	}


	public enum PlaceStyle
	{
		None = 0,
		QuickBite = 1,
		Relaxed = 2,
		Fancy = 3,
	}

	public class Vote: IComparable<Vote>
	{
		public const int MAX_MEALKIND = 0 + 1 + 2 + 4 + 8;

		#region sqlLite columns

		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		// the key of the place
		public string key { get; set; }

		public string voter { get; set; }

		public string place_name { get; set; }

		public VoteValue vote { get; set; }

		[Ignore]
		public bool untried { 
			get {
				return vote == VoteValue.Untried;
			} 
		}

		public string comment { get; set; }

		public DateTime when { get; set; }

		[Ignore]
		public Cuisine cuisine { get; set; }

		public string cuisineName { 
			get{ return cuisine?.ToString (); }
			set {
				Cuisine c = Persist.Instance.Cuisines.Where (cu => cu.Title == value).SingleOrDefault ();
				cuisine = c ?? new Cuisine  { Title = value };
			} 
		}

		public PlaceStyle style { get; set; }

		public MealKind kind { get; set; }

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
				try {
					return Persist.Instance.GetPlace (key).thumb_url;
				} catch (Exception ex) {
					Insights.Report (ex);
					return null;
				}
			}
		}

		[Ignore]
		public string GetVoteAsString {
			get {
				String imageUrl = "";
				if (vote == VoteValue.Liked)
					imageUrl = "Liked";
				if (vote == VoteValue.Disliked)
					imageUrl = "Disliked";
				if (untried)
					imageUrl = "Saved";
				return imageUrl;
			}
		}

		[Ignore]
		public string VoteVerb {
			get {
				if (vote == VoteValue.Liked)
					return "Liked";
				if (vote == VoteValue.Disliked)
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

		//		[Ignore]
		//		public string ShortAddress {
		//			get {
		//				return Persist.Instance.GetPlace (key).ShortAddress;
		//			}
		//		}

		[Ignore]
		public Place Place {
			get {
				return Persist.Instance.GetPlace (key);
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

		public Vote ()
		{
			this.comment = "";
			this.vote = VoteValue.None;
		}

	}
}

