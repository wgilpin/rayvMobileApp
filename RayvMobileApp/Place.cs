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

namespace RayvMobileApp
{
	public class Place: INotifyPropertyChanged, IComparable<Place>
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

		// boiler-plate
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged (string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}

		protected bool SetField<T> (ref T field, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals (field, value))
				return false;
			field = value;
			OnPropertyChanged (propertyName);
			return true;
		}

		#endregion

		#region Properties

		[PrimaryKey]
		public string key { 
			get { return _key; } 
			set { SetField (ref _key, value, "key"); } 
		}

		public double lat { 
			get { return _lat; } 
			set { SetField (ref _lat, value, "lat"); } 
		}

		public double lng { 
			get { return _lng; } 
			set { SetField (ref _lng, value, "lng"); } 
		}

		public string website { 
			get { return _website; } 
			set { SetField (ref _website, value, "website"); } 
		}

		public string address { 
			get { return _address; } 
			set { SetField (ref _address, value, "address"); } 
		}

		[MaxLength (50)]
		public string place_name { 
			get { return _place_name; } 
			set { SetField (ref _place_name, value, "place_name"); } 
		}

		public string place_id { 
			get { return _place_id; } 
			set { SetField (ref _place_id, value, "place_id"); } 
		}

		public string category { 
			get { return _category; } 
			set { SetField (ref _category, value, "category"); } 
		}

		public string telephone { 
			get { return _telephone; } 
			set { SetField (ref _telephone, value, "telephone"); } 
		}

		public bool untried { 
			get { return _untried; } 
			set { SetField (ref _untried, value, "untried"); } 
		}

		public string vote { 
			get { return _vote; } 
			set { SetField (ref _vote, value, "vote"); } 
		}

		public string descr { 
			get { return _descr; } 
			set { SetField (ref _descr, value, "descr"); } 
		}

		public string img { 
			get { 
				if (string.IsNullOrEmpty (_img))
					return "";
				if (_img [0] == 'h')
					return _img;
				return Persist.Instance.GetConfig (settings.SERVER) + _img; 
			} 
			set { SetField (ref _img, value, "img"); } 
		}

		public Int64 edited { 
			get { return _edited; } 
			set { SetField (ref _edited, value, "edited"); } 
		}

		public string thumbnail { 
			get { 
				if (string.IsNullOrEmpty (_thumbnail))
					return "";
				if (_thumbnail [0] == 'h')
					return _thumbnail;
				return Persist.Instance.GetConfig (settings.SERVER) + _thumbnail;
			} 
			set { SetField (ref _thumbnail, value, "thumbnail"); } 
		}

		public int up { 
			get { return _up; } 
			set { SetField (ref _up, value, "up"); } 
		}

		public int down { 
			get { return _down; } 
			set { SetField (ref _down, value, "down"); } 
		}

		public string owner { 
			get { return _owner; } 
			set { SetField (ref _owner, value, "owner"); } 
		}

		public bool is_map { 
			get { return _is_map; } 
			set { SetField (ref _is_map, value, "is_map"); } 
		}

		public bool adjusted { 
			get { return _adjusted; } 
			set { SetField (ref _adjusted, value, "adjusted"); } 
		}

		public double distance_double { 
			get { return _distance_double; } 
			set { SetField (ref _distance_double, value, "distance_double"); } 
		}

		public string postcode { 
			get { return _postcode; } 
			set { SetField (ref _postcode, value, "postcode"); } 
		}

		public string pretty_dist { 
			get { 
				if (_isDraft)
					return "";
				return _pretty_dist; 
			}
			set { SetField (ref _pretty_dist, value, "key"); } 
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
				SetField (ref _isDraft, value, "isDraft");
			}
		}

		public string DraftComment {
			get;
			set;
		}



		public string distance {
			get { 
				if (this.pretty_dist == null) {
					if (this.distance_double >= 0.5) {
						this.pretty_dist = String.Format ("{0:0.0} miles", this.distance_double);
					} else {
						var yds = Math.Floor (this.distance_double * 90) * 20;
						this.pretty_dist = String.Format ("{0} yds", yds);
					}
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
				this.pretty_dist = null;
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
		public int CompareTo (Place comparePlace)
		{
			// A null value means that this object is greater. 
			if (comparePlace == null)
				return 1;
			else {
				// draft > not draft
				if (this.IsDraft && !comparePlace.IsDraft)
					return -1;
				if (!this.IsDraft && comparePlace.IsDraft)
					return 1;
				return this.distance_double.CompareTo (comparePlace.distance_double);
			}
		}


	}

}

