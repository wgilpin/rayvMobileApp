﻿using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using SQLite;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;
using System.Collections.ObjectModel;

namespace RayvMobileApp.iOS
{


	public class Persist
	{
		private static string DbPath;
		private static SQLiteConnection Db;
		public List<Vote> Votes;

		public List<Place> Places { get; set; }

		public List<SearchHistory> SearchHistory;
		private List<string> _categories;

		void LoadCategories ()
		{
			lock (restConnection.Instance.Lock) {
				Dictionary<string, string> parameters = new Dictionary<string, string> ();
				string result = restConnection.Instance.get ("/getCuisines_ajax", parameters).Content;
				JObject obj = JObject.Parse (result);
				_categories = JsonConvert.DeserializeObject<List<string>> (obj.SelectToken ("categories").ToString ());
			}
		}

		public List<string> Categories {
			get {
				if (_categories == null)
					this.LoadCategories ();
				return _categories;
			}
		}
		// key => name
		public Dictionary<string,string> Friends;
		public Position GpsPosition;

		public Persist ()
		{
			Console.WriteLine ("Persist()");
			Votes = new List<Vote> ();
			Places = new List<Place> ();
			Friends = new Dictionary<string, string> ();
			GpsPosition = new Position (51.5797, -0.1237);
			DbPath = Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				"database.db3");
			if (!File.Exists (DbPath)) {
				createDb ();
			} else {
				Db = new SQLiteConnection (DbPath);
			}
			SearchHistory = Db.Query<SearchHistory> ("select DISTINCT * from SearchHistory order by ID limit 3");


		}

		private static Persist _instance;

		public static Persist Instance {
			get {
				if (_instance == null) {
					_instance = new Persist ();
				}
				return _instance;
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

		public void updatePlaces ()
		{
			foreach (Place p in Places) {
				p.distance_from_place ();
				Db.InsertOrReplace (p);
			}
			Places.Sort ();
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

		public void  UpdatePlace (Place place)
		{
			// calc dist
			place.distance_from_place ();
			foreach (Place p in Places) {
				if (p.key == place.key) {
					try {
						Console.WriteLine ("UpdatePlace: Saving {0}", place.place_name);
						StorePlace (place, p);
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

		public void onWebGetItems (string data)
		{
			JObject jResult = JObject.Parse (data);
			Places = jResult ["items"].ToObject<List<Place>> ();
			updatePlaces ();

		}

		public Place get_place (string key)
		{
			foreach (Place p in Places) {
				if (p.key == key) {
					return p;
				}
			}
			return null;
		}

		public string GetConfig (string key)
		{
			try {
				Configuration ConfItem = (from s in Db.Table<Configuration> ()
				                          where s.Key == key
				                          select s).First ();
				Console.WriteLine ("GetConfig: {0}=[{1}]", key, ConfItem.Value);
				return ConfItem.Value;
			} catch (Exception) {
				Console.WriteLine ("GetConfig: {0} not found", key);
				return "";
			}
		}

		public void SetConfig (string key, string value)
		{
			Db.InsertOrReplace (new Configuration (key, value));
		}

		public void LoadFromDb ()
		{
			//load the data from the db
			Console.WriteLine ("Persist.LoadFromDb loading");

			// instead of clear() - http://forums.xamarin.com/discussion/19114/invalid-number-of-rows-in-section
			Places.Clear ();
			var place_q = Db.Table<Place> ();
			Places.AddRange (place_q);
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

	}
}

