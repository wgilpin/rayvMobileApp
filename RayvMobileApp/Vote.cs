using System;
using SQLite;
using Xamarin.Forms;
using System.Text;
using Xamarin;
using System.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
	
	public enum VoteFilterWho
	{
		Mine,
		All,
		Chosen
	}


	public enum VoteFilterKind
	{
		All,
		Stars,
		Wish,
		Try
		// Wish plus Like
	}


	[Flags]
	public enum MealKind
	{
		None = 0x0,
		Breakfast = 0x1,
		Lunch = 0x2,
		Dinner = 0x4,
		Coffee = 0x8,
		Bar = 0x10
	}


	public enum PlaceStyle
	{
		None = 0,
		QuickBite = 1,
		Relaxed = 2,
		Fancy = 3,
	}

	public static class PlaceStyleExtensions
	{
		public static string ToFriendlyString (this PlaceStyle me)
		{
			switch (me) {
				case PlaceStyle.Fancy:
					return Vote.STYLE_FANCY;
				case PlaceStyle.QuickBite:
					return Vote.STYLE_QUICK;
				case PlaceStyle.Relaxed:
					return Vote.STYLE_RELAXED;
			}
			return "None";
		}
	}

	public class Vote: IComparable<Vote>
	{
		public static string STYLE_FANCY = "fancy - £££";
		public static string STYLE_QUICK = "quick bite - £";
		public static string STYLE_RELAXED = "relaxed - ££";
		public static int VoteNotSetValue = 0;
		public const int MAX_MEALKIND = 0 + 1 + 2 + 4 + 8 + 16;

		#region sqlLite columns

		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		// the key of the place
		public string key { get; set; }

		public string voter { get; set; }

		public string place_name { get; set; }

		public bool untried { get; set; }

		public int vote { get; set; }

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
				try {
					if (Persist.Instance.MyId.ToString () == voter)
						return "Me";
					return Persist.Instance.Friends [voter].Name;
				} catch (KeyNotFoundException) {
					if (Persist.Instance.InviteNames.ContainsKey (voter))
						return Persist.Instance.InviteNames [voter];
					else
						return "?";
				}
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

		public static Color RandomColor (string name)
		{
			try {
				string lname = name.ToLower ();
				if (lname.Length > 1) {
					int i1 = ((Encoding.ASCII.GetBytes (lname) [0] - 97) % 26) * 10;
					int i2 = ((Encoding.ASCII.GetBytes (lname) [1] - 97) % 26) * 10;
					int i3 = 255;
					if (lname.Length > 2)
						i3 = ((Encoding.ASCII.GetBytes (lname) [2] - 97) % 26) * 10;
					Color c = Color.FromRgb (i1, i2, i3);
					return c;
				}
				return Color.Black;
			} catch (Exception ex) {
				Insights.Report (ex);
				return Color.Black;
			}
		}

		public static string FirstLetter (string name)
		{
			try {
				return name.Remove (1); 
			} catch (Exception) {
				return "?";
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
			this.vote = 0;
			this.untried = false;
		}

	}
}

