using System;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using Xamarin;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace RayvMobileApp
{
	[Serializable]
	public class Place: INotifyPropertyChanged, IComparable
	{
		#region Fields

		private string _key;
		private double _lat;
		private double _lng;
		private string _website;
		private string _address;
		private string _place_name;
		private string _place_id;
		private string _category;
		private string _telephone;
		private bool _untried;
		private string _vote;
		private string _descr;
		private string _img;
		private Int64 _edited;
		private string _thumbnail;
		private int _up;
		private int _down;
		private string _owner;
		private bool _is_map;
		private bool _adjusted;
		private double _distance_double;
		private string _postcode;
		private string _pretty_dist;
		private string _commentSet;
		private bool _isSynced;
		private bool _isDraft;

		public event PropertyChangedEventHandler PropertyChanged = delegate {};

		private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
		{
			PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}

		#endregion

		#region Properties

		[PrimaryKey]
		public string key { 
			get { return _key; } 
			set {
				_key = value;
				NotifyPropertyChanged ();
			}
		}

		public double lat { 
			get { return _lat; } 
			set {
				_lat = value;
				NotifyPropertyChanged ();
			}
		}

		public double lng { 
			get { return _lng; } 
			set {
				_lng = value;
				NotifyPropertyChanged ();
			}
		}

		public string website { 
			get { return _website; } 
			set {
				_website = value;
				NotifyPropertyChanged ();
			}
		}

		public string address { 
			get { return _address; } 
			set {
				_address = value;
				NotifyPropertyChanged ();
			}
		}

		[MaxLength (50)]
		public string place_name { 
			get { return _place_name; } 
			set {
				_place_name = value;
				NotifyPropertyChanged ();
			}
		}

		public string place_id { 
			get { return _place_id; } 
			set {
				_place_id = value;
				NotifyPropertyChanged ();
			}
		}

		public string category { 
			get { return _category; } 
			set {
				_category = value;
				NotifyPropertyChanged ();
			}
		}

		public string telephone { 
			get { return _telephone; } 
			set {
				_telephone = value;
				NotifyPropertyChanged ();
			}
		}

		public bool untried { 
			get { return _untried; } 
			set {
				_untried = value;
				NotifyPropertyChanged ();
			}
		}

		public string vote { 
			get { return _vote; } 
			set {
				_vote = value;
				NotifyPropertyChanged ();
			}
		}

		public string descr { 
			get { return _descr; } 
			set {
				_descr = value;
				NotifyPropertyChanged ();
			}
		}

		public string img { 
			get { 
				if (string.IsNullOrEmpty (_img))
					return "";
				if (_img [0] == 'h')
					return _img;
				return Persist.Instance.GetConfig (settings.SERVER) + _img; 
			} 
			set {
				_img = value;
				NotifyPropertyChanged ();
			}
		}

		public Int64 edited { 
			get { return _edited; } 
			set {
				_edited = value;
				NotifyPropertyChanged ();
			}
		}

		public string thumbnail { 
			get { 
				if (string.IsNullOrEmpty (_thumbnail))
					return "";
				if (_thumbnail [0] == 'h')
					return _thumbnail;
				return Persist.Instance.GetConfig (settings.SERVER) + _thumbnail;
			} 
			set {
				_thumbnail = value;
				NotifyPropertyChanged ();
			}
		}

		public int up { 
			get { return _up; } 
			set {
				_up = value;
				NotifyPropertyChanged ();
			}
		}

		public int down { 
			get { return _down; } 
			set {
				_down = value;
				NotifyPropertyChanged ();
			}
		}

		public string owner { 
			get { return _owner; } 
			set {
				_owner = value;
				NotifyPropertyChanged ();
			}
		}

		public bool is_map { 
			get { return _is_map; } 
			set {
				_is_map = value;
				NotifyPropertyChanged ();
			}
		}

		public bool adjusted { 
			get { return _adjusted; } 
			set {
				_adjusted = value;
				NotifyPropertyChanged ();
			}
		}

		public double distance_double { 
			get { return _distance_double; } 
			set {
				_distance_double = value;
				SetPrettyDistance ();
				NotifyPropertyChanged ();
			}
		}

		public string postcode { 
			get { return _postcode; } 
			set {
				_postcode = value;
				NotifyPropertyChanged ();
			}
		}

		public string pretty_dist { 
			get { 
				if (_isDraft)
					return "";
				return _pretty_dist; 
			}
			set {
				_pretty_dist = value;
				NotifyPropertyChanged ();
			}
		}

		[Ignore]
		public string CategoryLowerCase {
			get {
				try {
					return _category.ToLower ();
				} catch (Exception ex) {
					Insights.Report (ex);
					return "";
				}
			}
		}

		//TODO: All of these view properties should be value converters in the template binding
		//		[Ignore]
		//		public string ShortAddress {
		//			get {
		//				// number then anything
		//				string pattern = @"^(\d+[-\d+]* )(.*)";
		//				MatchCollection matches = Regex.Matches (_address, pattern);
		//				return (matches.Count < 1) ?
		//					_address :
		//					matches [0].Groups [2].ToString ();
		//			}
		//		}

		public bool IsDraft {
			get{ return _isDraft; }
			set {
				_isDraft = value;
				NotifyPropertyChanged ();
			
			}
		}

		public string DraftComment {
			get;
			set;
		}

		void SetPrettyDistance ()
		{
			if (this.distance_double >= 0.5) {
				this.pretty_dist = String.Format ("{0:0.0} miles", this.distance_double);
			} else {
				var yds = Math.Floor (this.distance_double * 90) * 20;
				this.pretty_dist = String.Format ("{0} yds", yds);
			}
		}


		public string distance {
			get { 
				if (this.pretty_dist == null) {
					SetPrettyDistance ();
				} 
				return this.pretty_dist;
			}
		}

		// iVoted ; true if I voted - for the template
		public bool iVoted {
			get {
				// vote +1 or -1 is a vote, untried is a vote
				return (_vote == "1" || _vote == "-1" || untried == true);
			}
		}

		public bool noVote {
			get { return !iVoted; }
		}

		public string voteImage {
			get {
				String imageUrl = "";
				if (_vote == "1")
					imageUrl = "Liked";
				if (_vote == "-1")
					imageUrl = "Disliked";
				if (_untried)
					imageUrl = "Saved";
				if (imageUrl == "")
					imageUrl = "Liked   Disliked";
				return imageUrl;
			}
		}

		public string Comment ()
		{
			if (IsDraft)
				return DraftComment;
			if (!string.IsNullOrEmpty (_commentSet))
				return _commentSet;
			if (!String.IsNullOrEmpty (_descr))
				return _descr;
			var myVote = (from v in Persist.Instance.Votes
			              where v.key == _key &&
			                  v.voter == Persist.Instance.MyId.ToString ()
			              select v).FirstOrDefault ();
			if (myVote != null)
				return myVote.comment;
			return null;
		}

		// for updating the comment
		public void setComment (string value)
		{ 
			_commentSet = value; 
		}


		public ImageSource thumb_url {
			get { 
				if (String.IsNullOrEmpty (this.thumbnail))
					return null;
				return UriImageSource.FromUri (new Uri (this.thumbnail));
			}
		}

		public ImageSource img_url {
			get { 
				if (string.IsNullOrEmpty (this.img))
					return null;
				return UriImageSource.FromUri (new Uri (this.img));
			}
		}

		#endregion

		#region Methods



		public void CopyFromAnother (Place source)
		{
			PropertyInfo[] sourceProps = source.GetType ().GetProperties ();
			foreach (PropertyInfo property in sourceProps) {
				object value = property.GetValue (source);
				if (property.GetSetMethod () != null)
					property.SetValue (this, value);
			}
		}

		public void Delete ()
		{
			Vote vote = (from v in Persist.Instance.Votes
			             where v.key == _key
			             select v).FirstOrDefault ();
			if (vote != null) {
				string res = restConnection.Instance.post (
					             "api/delete",
					             new Dictionary<string, string> () {
						{ "key", _key }
					});
				if (res != null) {
					Persist.Instance.Places.Remove (this);

				}

			}
		}

		public Position GetPosition ()
		{
			return new Position (lat, lng);
		}

		/**
         * one in 60 rule distance calc
         * @param point {LatLng}
         * @param origin {LatLng}
         * @returns {number} distance between points
         */
		public static double approx_distance (Position point, Position origin)
		{
			//based on 1/60 rule
			//delta lat. Degrees * 69 (miles)
			double d_lat = (origin.Latitude - point.Latitude) * 69;
			//cos(lat) approx by 1/60
			double cos_lat = Math.Min (1, (90 - point.Latitude) / 60);
			//delta lng = degrees * cos(lat) *69 miles
			double d_lng = (origin.Longitude - point.Longitude) * 69 * cos_lat;
			return Math.Sqrt (d_lat * d_lat + d_lng * d_lng);
		}

		/**
		 * Set the point from which all points are measured
		 */
		public string CalculateDistanceFromPlace (Position? point = null)
		{
			try {
				if (IsDraft) {
					this.distance_double = 0;
					return "Draft";
				}
				Position calc_dist_from = point == null ? Persist.Instance.GpsPosition : (Position)point;
				this.distance_double = approx_distance (
					new Position (this.lat, this.lng),
					calc_dist_from);
//				this.pretty_dist = null;
			} catch (Exception ex) {
				Insights.Report (ex);
			}
			return this.distance;
		}

		public bool Save (out String errorMessage)
		{
			try {
				if (Persist.Instance.Online) {
					Dictionary<string, string> parameters = new Dictionary<string, string> ();
					
					parameters ["key"] = IsDraft ? "" : key;
					parameters ["lat"] = lat.ToString ();
					parameters ["lng"] = lng.ToString ();
					parameters ["address"] = address;
					parameters ["place_name"] = place_name;
					parameters ["myComment"] = Comment ();
					parameters ["category"] = category;
					parameters ["descr"] = "";
					parameters ["website"] = website;
					parameters ["telephone"] = telephone;
					switch (vote) {
					case "-1":
						parameters ["voteScore"] = "dislike";
						break;
					case "1":
						parameters ["voteScore"] = "like";
						break;
					default:
						parameters ["untried"] = "true";
						break;
					}

					bool wasDraft = IsDraft;
					string wasDraftKey = _key;
					string result = restConnection.Instance.post ("/item", parameters);
					//			JObject obj = JObject.Parse (result);
					Place place = JsonConvert.DeserializeObject<Place> (result);
					place.IsDraft = false;
					this.key = place.key;
					lock (Persist.Instance.Lock) {
						// no try..catch as it's inside one
						if (!Persist.Instance.UpdatePlace (place)) {
							errorMessage = "Failed to update";
							return false;
						}
						if (wasDraft) {
							// delete the old one
							Persist.RemovePlaceKeyFromDb (wasDraftKey);
						}
					}
				} else {
					lock (Persist.Instance.Lock) {
						// no try..catch as it's inside one
						if (!Persist.Instance.UpdatePlace (this)) {
							errorMessage = "Failed to save draft";
							return false;
						}
					}
				}
				errorMessage = "";
				return true;
			} catch (Exception ex) {
				Insights.Report (ex);
				// store the rrorMessage for the out param
				errorMessage = String.Format ("Place.Save: Exception {0}", ex);
				restConnection.LogErrorToServer (errorMessage);
				return false;
			}
		}

		public bool SaveVote (out String errorMessage)
		{
			if (IsDraft) {
				throw new NotSupportedException ("Can't save a vote on a draft ");
			}
			try {
				if (Persist.Instance.Online) {
					Dictionary<string, string> parameters = new Dictionary<string, string> ();

					parameters ["key"] = key;
					parameters ["myComment"] = Comment ();
					switch (vote) {
					case "-1":
						parameters ["voteScore"] = "dislike";
						break;
					case "1":
						parameters ["voteScore"] = "like";
						break;
					default:
						parameters ["untried"] = "true";
						break;
					}

					string result = restConnection.Instance.post ("/api/vote", parameters);
					//			JObject obj = JObject.Parse (result);
					if (result == "OK") {
						lock (Persist.Instance.Lock) {
							// no try..catch as it's inside one
							if (!Persist.Instance.UpdateVote (this)) {
								errorMessage = "Failed to update";
								return false;
							}
						
						}
					}
				} else {
					lock (Persist.Instance.Lock) {
						// no try..catch as it's inside one
						if (!Persist.Instance.UpdateVote (this)) {
							errorMessage = "Can't vote offline";
							return false;
						}
					}
				}
				errorMessage = "";
				return true;
			} catch (Exception ex) {
				Insights.Report (ex);
				// store the rrorMessage for the out param
				errorMessage = String.Format ("Place.Save: Exception {0}", ex);
				restConnection.LogErrorToServer (errorMessage);
				return false;
			}
		}

		#endregion

		// Default comparer for Place type.
		public int CompareTo (object comparePlace)
		{
			// A null value means that this object is greater. 
			if (comparePlace == null || !(comparePlace is Place))
				return 1;
			else {
				Place p = comparePlace as Place;
				// draft > not draft
				if (this.IsDraft && !p.IsDraft)
					return -1;
				if (!this.IsDraft && p.IsDraft)
					return 1;
				return this.distance_double.CompareTo (p.distance_double);
			}
		}


	}

}

