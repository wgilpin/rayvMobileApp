using System;
using Xamarin.Forms.Maps;

namespace RayvMobileApp
{
	public class MapPlace : IComparable<MapPlace>
	{
		Place _place;
		Double _distanceFromMapCentre;

		public Place place { 
			get { 
				return _place;
			}
			set {
				_place = value;
			}

		}

		void SetDistance (Position from)
		{
			//based on 1/60 rule
			//delta lat. Degrees * 69 (miles)
			double d_lat = (from.Latitude - place.lat) * 69;
			//cos(lat) approx by 1/60
			double cos_lat = Math.Min (1, (90 - place.lat) / 60);
			//delta lng = degrees * cos(lat) *69 miles
			double d_lng = (from.Longitude - place.lng) * 69 * cos_lat;
			_distanceFromMapCentre = Math.Sqrt (d_lat * d_lat + d_lng * d_lng);
		}

		public MapPlace (Place p, Position mapCenter)
		{
			place = p;
			SetDistance (mapCenter);
		}

		public int CompareTo (MapPlace other)
		{
			return Math.Sign (_distanceFromMapCentre - other._distanceFromMapCentre);
		}
	}
}

