using System;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace RayvMobileApp.iOS
{
	public class Place
	{
		[PrimaryKey]
		public string key { get; set; }

		public double lat { get; set; }

		public double lng { get; set; }

		public string website { get; set; }

		public string address { get; set; }

		[MaxLength (50)]
		public string place_name { get; set; }

		public string place_id { get; set; }

		public string category { get; set; }

		public string telephone { get; set; }

		public bool untried { get; set; }

		public string vote { get; set; }

		public string descr { get; set; }

		public string img { get; set; }

		public Int64 edited { get; set; }

		public string thumbnail { get ; set; }

		public int up { get; set; }

		public int down { get; set; }

		public string owner { get; set; }

		public bool is_map { get; set; }

		public bool adjusted { get; set; }

		public double distance_double { get; set; }

		public string postcode { get; set; }

		public string pretty_dist { get; set; }

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
	}

}

