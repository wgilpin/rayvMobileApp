using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using SQLite;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RayvMobileApp.iOS
{


	public class Persist
	{
		#region Fields

		private static string DbPath;
		private static SQLiteConnection Db;
		public List<Vote> Votes;

		public List<Place> Places { get; set; }

		public bool DataIsLive;

		public List<SearchHistory> SearchHistory;
		private List<string> _categories;
		public Dictionary<string,string> Friends;
		public Position GpsPosition;
		private static Persist _instance;

		public Object Lock = new Object ();

		#endregion

		#region Properties

		public List<string> Categories {
			get {
				if (_categories == null)
					this.LoadCategories ();
				return _categories;
			}
		}

		public static Persist Instance {
			get {
				if (_instance == null) {
					_instance = new Persist ();
				}
				return _instance;
			}
		}

		public Int64 MyId {
			get;
			set;
		}

		#endregion

		#region Methods

		public void Wipe ()
		{
			Db.BeginTransaction ();
			Places.Clear ();
			Db.DeleteAll<Place> ();
			Votes.Clear ();
			Db.DeleteAll<Vote> ();
			Friends.Clear ();
			Db.DeleteAll<Friend> ();
			Db.Commit ();
		}

		void LoadCategories ()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string> ();
			string result = restConnection.Instance.get ("/getCuisines_ajax", parameters).Content;
			lock (Lock) {
				JObject obj = JObject.Parse (result);
				_categories = JsonConvert.DeserializeObject<List<string>> (obj.SelectToken ("categories").ToString ());
			}
		}

		static void createDb ()
		{
			Db = new SQLiteConnection (DbPath);
			Db.CreateTable<Vote> ();
			Db.CreateTable<Place> ();
			Db.CreateTable<Friend> ();
			Db.CreateTable<SearchHistory> ();
			Db.CreateTable<Configuration> ();
		}

		/**
		 * Sort Places by Distance from me and update DB
		 */
		public void updatePlaces ()
		{
			foreach (Place p in Places) {
				p.CalculateDistanceFromPlace ();
				Db.InsertOrReplace (p);
			}
			Places.Sort ();
			foreach (Vote v in Votes) {
				try {
					var found_v = (from fv in Db.Table<Vote> ()
					               where fv.key == v.key
					               select fv);
					if (found_v.Count () == 0)
						Db.Insert (v);
					//Db.InsertOrReplace (v);
				} catch (Exception E) {
					Console.WriteLine ("updatePlaces Exception: {0}", E.Message);
				}
			}
		}

		/**
		 * Sort Place by Distance from me
		 */
		public void SortPlaces (List<Place> placeList = null)
		{
			if (placeList == null)
				placeList = Places;
			foreach (Place p in placeList) {
				p.CalculateDistanceFromPlace ();
			}
			placeList.Sort ();
		}

		void StorePlace (Place place, Place removePlace = null)
		{
			Db.BeginTransaction ();
			var cmd = Db.CreateCommand (String.Format ("delete from Place where key='{0}'", place.key));
			cmd.ExecuteNonQuery ();
			Db.Insert (place);
			Db.Commit ();
			if (removePlace != null) {
				Places.Remove (removePlace);
			}
			Places.Add (place);
		}

		public void UpdatePlace (Place place)
		{
			Debug.WriteLine ("UpdatePlaces");
			// calc dist
			place.CalculateDistanceFromPlace ();
			for (int i = 0; i < Places.Count (); i++) {
				if (Places [i].key == place.key) {
					try {
						StorePlace (place, Places [i]);
						updatePlaces ();
						return;
					} catch (Exception e) { 
						Db.Rollback ();
						Console.WriteLine ("** UpdatePlace ROLLBACK : '{0}'", e);
					}
				}
			}
			StorePlace (place);
			updatePlaces ();
		}

		public void updateVotes ()
		{
			foreach (Vote v in Votes) {
				Db.InsertOrReplace (v);
			}
		}

		public void AddSearchHistoryItem (string item)
		{
			foreach (SearchHistory historyItem in SearchHistory) {
				if (historyItem.PlaceName == item) {
					//already exists
					return;
				}
			}
			SearchHistory.Add (new SearchHistory (item));
			if (SearchHistory.Count > 3) {
				SearchHistory.Remove (SearchHistory [0]);
			}
			try {
				Db.BeginTransaction ();
				var cmd = Db.CreateCommand ("delete from SearchHistory");
				cmd.ExecuteNonQuery ();
				Db.InsertAll (SearchHistory);
				Db.Commit ();
			} catch { 
				Db.Rollback ();
				Console.WriteLine ("** AddSearchHistoryItem ROLLBACK");
			}
		}

		//		public void onWebGetItems (string data)
		//		{
		//			JObject jResult = JObject.Parse (data);
		//			Places = jResult ["items"].ToObject<List<Place>> ();
		//			updatePlaces ();
		//
		//		}

		public Place GetPlace (string key)
		{
			foreach (Place p in Places) {
				if (p.key == key) {
					return p;
				}
			}
			return null;
		}

		public Place GetPlaceFromDb (string key)
		{
			return (from p in Db.Table<Place> ()
			        where p.key == key
			        select p).First ();
		}

		public string GetConfig (string key)
		{
			try {
				Configuration ConfItem = (from s in Db.Table<Configuration> ()
				                          where s.Key == key
				                          select s).First ();
				return ConfItem.Value;
			} catch (Exception) {
				Console.WriteLine ("GetConfig: {0} not found", key);
				return "";
			}
		}

		public Double GetConfigDouble (string key)
		{
			string StringValue = GetConfig (key);
			try {
				return Convert.ToDouble (StringValue);
			} catch {
				return 0.0;
			}
		}

		public void SetConfig (string key, string value)
		{
			Db.InsertOrReplace (new Configuration (key, value));
		}

		public void SetConfigDouble (string key, Double value)
		{
			SetConfig (key, Convert.ToString (value));
		}

		public void LoadFromDb (String onlyWithCuisineType = null)
		{
			//load the data from the db
			Console.WriteLine ("Persist.LoadFromDb loading");

			// instead of clear() - http://forums.xamarin.com/discussion/19114/invalid-number-of-rows-in-section
			Places.Clear ();
			var place_q = Db.Table<Place> ();
			if (onlyWithCuisineType == null) {
				// all cuisine types
				Places.AddRange (place_q);
				foreach (var p in Places)
					p.CalculateDistanceFromPlace ();
			} else
				//TODO: LINQ
				foreach (Place p in place_q) {
					if (p.category == onlyWithCuisineType) {
						Places.Add (p);
						p.CalculateDistanceFromPlace ();
					}
				}
			Places.Sort ();
			Votes.Clear ();
			var votes_q = Db.Table<Vote> ();
			foreach (var vote in votes_q)
				Votes.Add (vote);
			if (SearchHistory != null) {
				SearchHistory.Clear ();
				var searches_q = Db.Table<SearchHistory> ();
				foreach (var search in searches_q) {
					if (search.PlaceName != null) {
						SearchHistory.Add (search);
					}
				}
			}
			Console.WriteLine ("Persist.LoadFromDb loaded");
			//var friends_q = db.Table<Friend> ();
			//foreach (var friend in friends_q)
			//	friends.Add(friend);


		}

		#endregion

		public Persist ()
		{
			Console.WriteLine ("Persist()");
			Votes = new List<Vote> ();
			Places = new List<Place> ();
			Friends = new Dictionary<string, string> ();
			DataIsLive = false;
			DbPath = Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				"database.db3");
			if (!File.Exists (DbPath)) {
				createDb ();
			} else {
				Db = new SQLiteConnection (DbPath);
			}
			SearchHistory = Db.Query<SearchHistory> ("select DISTINCT * from SearchHistory order by ID limit 3");
			Double Lat = GetConfigDouble ("LastLat");
			Double Lng = GetConfigDouble ("LastLng");
			GpsPosition = new Position (Lat, Lng);
		}
	}
}

