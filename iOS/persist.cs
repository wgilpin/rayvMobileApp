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
using Xamarin;
using Xamarin.Forms;
using RestSharp;
using System.Net;
using System.Threading;

namespace RayvMobileApp.iOS
{
	public class Persist
	{
		public TimeSpan DbTimeout = new TimeSpan (0, 0, 5);

		#region Fields

		private static string DbPath;
		public List<Vote> Votes;

		public List<Place> Places { get; set; }

		public bool DataIsLive;

		public List<SearchHistory> SearchHistoryList;
		private List<string> _categories;
		public Dictionary<string,string> Friends;
		public Position GpsPosition;
		private static Persist _instance;

		public Object Lock = new Object ();
		private bool _unsyncedPlaces;

		#endregion

		const string DB_VERSION = "DB_VERSION";

		#region Properties

		public bool UnsyncedPlaces {
			get { return _unsyncedPlaces; }
			set { 
				_unsyncedPlaces = value; 
				if (value)
					TrickleUpdate ();
			}
		}

		public bool HaveAdded { get; set; }

		public List<string> Categories {
			get {
				if (_categories == null || _categories.Count () == 0)
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


		bool UdpdateNextUnsynced ()
		{
			Console.WriteLine ("Persist UdpdateNextUnsynced Timer Event");
			Place p = Places.Where (x => x.IsSynced == false).FirstOrDefault ();
			if (p == null) {
				return false;
			} else {
				string msg;
				p.Save (out msg);
				return true;
			}
		}

		void TrickleUpdate ()
		{	
			if (settings.OFFLINE_UPDATE_ENABLED) {
				Console.WriteLine ("Persist starting TrickleUpdate");
				Device.StartTimer (new TimeSpan (0, 0, 3), UdpdateNextUnsynced);
			}
		}

		void StoreFullUserRecord (IRestResponse resp)
		{
			try {
				Console.WriteLine ("GetFullData: lock get full");
				JObject obj = JObject.Parse (resp.Content);
				MyId = obj ["id"].Value<Int64> ();
				string placeStr = obj ["places"].ToString ();
				Dictionary<string, Place> place_list = JsonConvert.DeserializeObject<Dictionary<string, Place>> (placeStr);
				lock (Lock) {
					try {
						Places = place_list.Values.ToList ();
						Places.Sort ();
						Votes.Clear ();
						foreach (JObject fr in obj ["friendsData"]) {
							string fr_id = fr ["id"].ToString ();
							string name = fr ["name"].ToString ();
							Friends [fr_id] = name;
							Dictionary<string, Vote> vote_list = fr ["votes"].ToObject<Dictionary<string, Vote>> ();
							if (vote_list != null) {
								foreach (KeyValuePair<string, Vote> v in vote_list) {
									v.Value.voter = fr_id;
								}
								Votes.AddRange (vote_list.Values);
							}
						}
						//sort
						updatePlaces ();
					} catch (Exception ex) {
						Insights.Report (ex);
						restConnection.LogErrorToServer ("ListPage.GetFullData lock Exception {0}", ex);
					}
				}
				DataIsLive = true;
				Console.WriteLine ("ListPage.Setup loaded");
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("GetFullData Exception {0}", ex);
			}
		}

		// for an incremental query - since a time
		void StoreUpdatedUserRecord (IRestResponse resp)
		{
			try {
				Console.WriteLine ("StoreUpdatedUserRecord: lock ");
				JObject obj = JObject.Parse (resp.Content);
				MyId = obj ["id"].Value<Int64> ();
				string placeStr = obj ["places"].ToString ();
				Dictionary<string, Place> place_list = JsonConvert.DeserializeObject<Dictionary<string, Place>> (placeStr);
				lock (Lock) {
					try {
						bool Added = false;
						foreach (KeyValuePair<String,Place> kvp in place_list) {
							Added = false;
							for (int PlacesIdx = 0; PlacesIdx < Places.Count (); PlacesIdx++) {
								if (Places [PlacesIdx].key == kvp.Key) {
									Places [PlacesIdx] = kvp.Value;
									Added = true;
									break;
								}
							}
							if (!Added)
								Places.Add (kvp.Value);
						}
						List<Vote> NewVotes = new List<Vote> ();
						foreach (JObject fr in obj ["friendsData"]) {
							string fr_id = fr ["id"].ToString ();
							string name = fr ["name"].ToString ();
							Friends [fr_id] = name;
							Dictionary<string, Vote> vote_list = fr ["votes"].ToObject<Dictionary<string, Vote>> ();
							if (vote_list != null)
								foreach (KeyValuePair<string, Vote> v in vote_list) {
									v.Value.voter = fr_id;
									NewVotes.Add (v.Value);
								}
						}
						foreach (Vote v in NewVotes) {
							Added = false;
							for (int VoteIdx = 0; VoteIdx < Votes.Count (); VoteIdx++) {
								if (Votes [VoteIdx].key == v.key && Votes [VoteIdx].voter == v.voter) {
									Votes [VoteIdx] = v;
									Added = true;
									break;
								}
							}
							if (!Added)
								Votes.Add (v);
						}

						//sort
						updatePlaces ();
					} catch (Exception ex) {
						Insights.Report (ex);
						restConnection.LogErrorToServer ("ListPage.GetFullData lock Exception {0}", ex);
					}
				}
				DataIsLive = true;
				Console.WriteLine ("ListPage.Setup loaded");
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("GetFullData Exception {0}", ex);
			}
		}


		public void GetUserData (Page caller, DateTime? since = null)
		{
			restConnection webReq = restConnection.Instance;
			string server = GetConfig ("server");
			if (server.Length == 0) {
				Console.WriteLine ("GetUserData: No server");
				return;
			} else {
				webReq.setBaseUrl (server);
				webReq.setCredentials (GetConfig ("username"), GetConfig ("pwd"), "");
				IRestResponse resp;
				Console.WriteLine ("GetUserData Login");
				resp = webReq.get ("/api/login", null);
				if (resp == null) {
					Console.WriteLine ("GetUserData: Response NULL");
					return;
				}
				if (resp.StatusCode == HttpStatusCode.Unauthorized) {
					//TODO: This doesn't work
					Device.BeginInvokeOnMainThread (() => {
						Console.WriteLine ("GetUserData: Need to login - push LoginPage");
						caller.Navigation.PushModalAsync (new LoginPage ());
					});
					Console.WriteLine ("GetFullData: No login");
					return;
				}
				Dictionary<String, String> paramList = new Dictionary<String, String> ();
				if (since != null) {
					paramList.Add ("since", ((DateTime)since).ToString ("s"));
				}
				resp = webReq.get ("/getFullUserRecord", paramList);
				if (resp.ResponseStatus == ResponseStatus.Error) {
					//unable to contact server
					Console.WriteLine ("GetUserData - NO RESPONSE");
					return;
				}
				if (since != null) {
					// incremental
					StoreUpdatedUserRecord (resp);
				} else {
					StoreFullUserRecord (resp);
				}
			}
		}


		public void Wipe ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BeginTransaction ();
				try {
					Places.Clear ();
					Db.DeleteAll<Place> ();
					Votes.Clear ();
					Db.DeleteAll<Vote> ();
					Friends.Clear ();
					Db.DeleteAll<Friend> ();
					Db.Commit ();
				} catch (Exception ex) {
					Insights.Report (ex);
					Db.Rollback ();
				}
			}
		}

		void LoadCategories ()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string> ();
			string result = restConnection.Instance.get ("/getCuisines_ajax", parameters).Content;
			lock (Lock) {
				try {
					JObject obj = JObject.Parse (result);
					_categories = JsonConvert.DeserializeObject<List<string>> (obj.SelectToken ("categories").ToString ());
				} catch (Exception ex) {
					Insights.Report (ex);
					restConnection.LogErrorToServer ("Persist.LoadCategories Exception {0}", ex);
					_categories = new List<string> ();
				}
			}
		}

		static void createDb ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.CreateTable<Vote> ();
				Db.CreateTable<Place> ();
				Db.CreateTable<Friend> ();
				Db.CreateTable<SearchHistory> ();
				Db.CreateTable<Configuration> ();
			}
		}

		/**
		 * Sort Places by Distance from me and update DB
		 */
		public void updatePlaces ()
		{
			using (SQLiteConnection db = new SQLiteConnection (DbPath)) {
				db.BusyTimeout = DbTimeout;
				foreach (Place p in Places) {
					p.CalculateDistanceFromPlace ();
					db.InsertOrReplace (p);
					// it is synced because it has just come from the server
					p.IsSynced = true;
				}
			
				Places.Sort ();
				foreach (Vote v in Votes) {
					try {
						var found_v = (from fv in db.Table<Vote> ()
						               where fv.key == v.key
						               select fv);
						if (found_v.Count () == 0)
							db.Insert (v);
						//Db.InsertOrReplace (v);
					} catch (Exception E) {
						Insights.Report (E);
						restConnection.LogErrorToServer ("updatePlaces Exception: {0}", E.Message);
					}
				}
			
				foreach (KeyValuePair<string, string> f in Friends) {
					try {
						db.InsertOrReplace (new Friend { id = f.Key, name = f.Value });
					} catch (Exception ex) {
						Insights.Report (ex);
						Console.WriteLine ("Persist.updatePlaces: Friends {0}", ex.Message);
					}
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

			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					Db.BusyTimeout = DbTimeout;
					Db.BeginTransaction ();
					var cmd = Db.CreateCommand (String.Format ("delete from Place where key='{0}'", place.key));
					cmd.ExecuteNonQuery ();
					Db.Insert (place);
					Db.Commit ();
					if (removePlace != null) {
						Places.Remove (removePlace);
					}
					Places.Add (place);
				} catch (Exception ex) {
					Db.Rollback ();
					Insights.Report (ex);
				}
			}
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
						return;
					} catch (Exception e) { 
						Insights.Report (e);
						restConnection.LogErrorToServer ("** UpdatePlace ROLLBACK : '{0}'", e);
					}
				}
			}
			StorePlace (place);
		}

		public void DeletePlace (Place place)
		{
			try {
				Place StoredPlace = (from p in Places
				                     where p.key == place.key
				                     select p).FirstOrDefault ();
				if (StoredPlace != null) {
					Places.Remove (StoredPlace);
					using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
						try {
							Db.BeginTransaction ();
							var cmd = Db.CreateCommand (String.Format ("delete from Place where key='{0}'", StoredPlace.key));
							cmd.ExecuteNonQuery ();
							Db.Commit ();
						} catch (Exception ex) {
							Db.Rollback ();
							Insights.Report (ex);
						}
					}
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DeletePlace", ex);
			}
		}

		public void updateVotes ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BusyTimeout = DbTimeout;
				foreach (Vote v in Votes) {
					Db.InsertOrReplace (v);
				}
			}
		}

		private void SaveSearchHistoryToDB ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					Db.BusyTimeout = DbTimeout;
					Db.BeginTransaction ();
					var cmd = Db.CreateCommand ("delete from SearchHistory");
					cmd.ExecuteNonQuery ();
					Db.InsertAll (SearchHistoryList);
					Db.Commit ();
				} catch (Exception ex) { 
					Db.Rollback ();
					Insights.Report (ex);
					restConnection.LogErrorToServer ("** SaveSearchHistoryToDB ROLLBACK {0}", ex);
				}
			}
		}

		public void LoadSearchHistoryFromDb ()
		{
			SearchHistoryList.Clear ();
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				var searches_q = Db.Table<SearchHistory> ();
				foreach (var search in searches_q) {
					if (search.PlaceName != null) {
						SearchHistoryList.Add (search);
					}
				}
			}
		}

		public void AddSearchHistoryItem (string item)
		{
			try {
				var found = SearchHistoryList.FirstOrDefault (h => h.PlaceName == item);
				if (found != null) {
					SearchHistoryList.Remove (found);
					SearchHistoryList.Insert (0, found);
					SaveSearchHistoryToDB ();
					return; // already in list
				}
				SearchHistoryList.Add (new SearchHistory (item));
				if (SearchHistoryList.Count > 3) {
					SearchHistoryList.Remove (SearchHistoryList [0]);
				}
				SaveSearchHistoryToDB ();
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("Persist.AddSearchHistoryItem: {0}", ex);
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
			return Places.FirstOrDefault (p => p.key == key);
		}

		public Place GetPlaceFromDb (string key)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				return (from p in Db.Table<Place> ()
				        where p.key == key
				        select p).FirstOrDefault ();
			}
		}

		public string GetConfig (string key)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					var ConfList = (from s in Db.Table<Configuration> ()
					                where s.Key == key
					                select s);
					if (ConfList.Count () > 0)
						return ConfList.First ().Value;
				} catch (Exception ex) {
					Insights.Report (ex);
					restConnection.LogErrorToServer ("GetConfig: {0} not found", key);
				}
				return "";
			}
		}

		public Double GetConfigDouble (string key)
		{
			try {
				return Convert.ToDouble (GetConfig (key));
			} catch {
				return 0.0;
			}
		}

		public void SetConfig (string key, string value)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BusyTimeout = DbTimeout;
				try {
					Db.InsertOrReplace (new Configuration (key, value));
				} catch (Exception ex) {
					Insights.Report (ex);
				}
			}
		}

		public void SetConfig (string key, int value)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BusyTimeout = DbTimeout;
				Db.InsertOrReplace (new Configuration (key, value.ToString ()));
			}
		}

		public void SetConfigDouble (string key, Double value)
		{
			SetConfig (key, Convert.ToString (value));
		}

		private void UpdateSchema ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					int db_version;
					if (!int.TryParse (GetConfig (DB_VERSION), out db_version)) {
						db_version = 0;
					}
					if (db_version == 0) {
						Db.BeginTransaction ();
						try {
							Db.DropTable<SearchHistory> ();
							Db.CreateTable<SearchHistory> ();
							db_version = 1;
							SetConfig (DB_VERSION, db_version);
							Console.WriteLine ("Schema updated to 1");
							Db.Commit ();
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("UpdateSchema to 1 {0}", ex);
							Db.Rollback ();
							return;
						}
					}
					if (db_version == 1) {
						//Migration 2 - add When field to votes
						Db.BeginTransaction ();
						try {
							Db.DropTable<Vote> ();
							Db.CreateTable<Vote> ();
							db_version = 2;
							SetConfig (DB_VERSION, db_version);
							Console.WriteLine ("Schema updated to 2");
							Db.Commit ();
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("UpdateSchema to 2 {0}", ex);
							Db.Rollback ();
							return;
						}
					}
					Console.WriteLine ("Schema Up To Date");
				} catch (Exception ex) {
					restConnection.LogErrorToServer ("UpdateSchema {0}", ex);
				}
			}
		}

		public void LoadFromDb (String onlyWithCuisineType = null)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					//load the data from the db
					Console.WriteLine ("Persist.LoadFromDb loading");

					UpdateSchema ();
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
					LoadSearchHistoryFromDb ();
			
					Console.WriteLine ("Persist.LoadFromDb loaded");
					var friends_q = Db.Table<Friend> ();
					foreach (var friend in friends_q)
						Friends [friend.id] = friend.name;
				} catch (Exception ex) {
					Insights.Report (ex);
					Console.WriteLine ("Persist.LoadFromDb {0}", ex.Message);
				}
			}
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
				Console.WriteLine ("Persist: New Db");
				Insights.Track ("New Db");
				createDb ();
			} 
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				SearchHistoryList = Db.Query<SearchHistory> ("select DISTINCT * from SearchHistory order by ID limit 3");
			}
			Double Lat = GetConfigDouble ("LastLat");
			Double Lng = GetConfigDouble ("LastLng");
			GpsPosition = new Position (Lat, Lng);
		}
	}
}

