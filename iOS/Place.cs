using System;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RayvMobileApp.iOS
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
			get { return _img; } 
			set { SetField (ref _img, value, "img"); } 
		}

		public Int64 edited { 
			get { return _edited; } 
			set { SetField (ref _edited, value, "edited"); } 
		}

		public string thumbnail { 
			get { return _thumbnail; } 
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
			get { return _pretty_dist; } 
			set { SetField (ref _pretty_dist, value, "key"); } 
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
				if (_vote == "1")
					return "heart-lg.png";
				if (_vote == "-1")
					return "no-entry-lg.png";
				if (_untried)
					return "star-lg.png";
				return "two-smileys-lg.png";
			}
		}

		public string Comment ()
		{

			if (_commentSet != null)
				return _commentSet;
			for (int i = 0; i <= Persist.Instance.Votes.Count; i++) {
				Vote v = Persist.Instance.Votes [i];
				if (v.key == _key) {
					return v.comment;
				}
			}
			return null;
		}

		// for updating the comment
		public void setComment (string value)
		{ 
			_commentSet = value; 
		}


		public ImageSource thumb_url {
			get { 
				if (this.thumbnail.Length > 0)
					return UriImageSource.FromUri (new Uri (this.thumbnail));
				return null;
			}
		}

		public ImageSource img_url {
			get { 
				return UriImageSource.FromUri (new Uri (this.img));
			}
		}

		#endregion

		#region Methods

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
		public string distance_from_place (Position? point = null)
		{
			Position calc_dist_from = point == null ? Persist.Instance.GpsPosition : (Position)point;
			this.distance_double = approx_distance (
				new Position (this.lat, this.lng),
				calc_dist_from);
			this.pretty_dist = null;
			return this.distance;
		}

		public bool Save ()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string> ();

			parameters ["key"] = key;
			parameters ["lat"] = lat.ToString ();
			parameters ["lng"] = lng.ToString ();
			parameters ["addr"] = address;
			parameters ["place_name"] = place_name;
			parameters ["myComment"] = Comment ();
			parameters ["category"] = category;
			parameters ["descr"] = "";
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
			try {
				string result = restConnection.Instance.post ("/item", parameters);
				//			JObject obj = JObject.Parse (result);
				Place place = JsonConvert.DeserializeObject<Place> (result);
				Console.WriteLine ("DoSave: read distance as {0}", place.distance);
				lock (restConnection.Instance.Lock) {
					Persist.Instance.UpdatePlace (place);
				}
				return true;

			} catch (Exception ex) {
				Console.WriteLine ("EditPage.DoSave: Exception {0}", ex);
				return false;
			}
		}

		#endregion


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

		// Default comparer for Part type.
		public int CompareTo (Place comparePlace)
		{
			// A null value means that this object is greater. 
			if (comparePlace == null)
				return 1;
			else
				return this.distance_double.CompareTo (comparePlace.distance_double);
		}


	}

}

