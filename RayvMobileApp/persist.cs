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

namespace RayvMobileApp
{
	public class StatusMessageEventArgs : EventArgs
	{
		public string Message;
		public double Progress;

		public StatusMessageEventArgs (string message, double progress)
		{
			Message = message;
			Progress = progress;
		}
	}

	public class Persist
	{
		public TimeSpan DbTimeout = new TimeSpan (0, 0, 5);

		#region Fields

		private static string DbPath;
		public List<Vote> Votes;

		public List<Place> Places { get; set; }

		public List<string> CuisineHistory;

		public PersistantQueue SearchHistory;
		private List<Cuisine> _categories;
		private Dictionary<string, int> _categoryCounts;
		public Dictionary<string, Friend> Friends;
		public Position GpsPosition;
		private static Persist _instance;

		public Object Lock = new Object ();
		private bool _online = false;
		private System.Timers.Timer _onlineTimer;

		#endregion

		#region Properties

		public bool Online {
			get {
				if (_online)
					return true;
				restConnection conn = GetWebConnection ();
				var resp = conn.get ("/api/login", null, getRetries: 1);
				if (resp == null) {
					Console.WriteLine ("Online: Response NULL");
					return false;
				}
				if (resp.ResponseStatus != ResponseStatus.Completed) {
					Console.WriteLine ("Online: Bad Response {0}", resp.ResponseStatus);
					return false;
				}
				_online = true;
				return true;
			}
			set { _online = value; }

		}

		private Position? _displayPosition;

		public Position DisplayPosition { 
			get {
				if (_displayPosition == null)
					_displayPosition = GpsPosition;
				return (Position)_displayPosition;
			}
			set { _displayPosition = value; }
		}


		public bool HaveAdded { get; set; }

		public List<Cuisine> Cuisines {
			get {
				if (_categories == null || _categories.Count () == 0)
					this.LoadCategoriesFromDb ();
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

		public bool IsAdmin {
			get { return this.GetConfigBool ("is_admin"); }
			set { this.SetConfig ("is_admin", true); }
		}

		public Dictionary<string, int> CategoryCounts {
			get { return _categoryCounts; }
		}

		#endregion

		#region Db Methods

		public static bool TableExists<T> (SQLiteConnection connection)
		{    
			const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
			var cmd = connection.CreateCommand (cmdText, typeof(T).Name);
			return cmd.ExecuteScalar<string> () != null;
		}

		public void UpdateSchema ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					int db_version;
					if (!int.TryParse (GetConfig (settings.DB_VERSION), out db_version)) {
						db_version = 0;
					}
					if (db_version < 8) {
						//Migration  -  plus wipe
						Db.BeginTransaction ();
						try {
							deleteDb ();
							createDb ();
							SetConfig (settings.SERVER, null);
							db_version = 8;
							Console.WriteLine ("Schema updated to 8");
							Db.Commit ();
							SetConfig (settings.DB_VERSION, db_version, Db);
							var server_url = "https://" +
							                 ServerPicker.GetServerVersionForAppVersion () +
							                 settings.DEFAULT_SERVER;
							Persist.Instance.SetConfig (settings.SERVER, server_url);
							return;
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("UpdateSchema to 8 failed {0}", ex);
							Db.Rollback ();
							return;
						}
					}
					if (db_version == 0) {
						Db.BeginTransaction ();
						try {
							Db.DropTable<SearchHistory> ();
							db_version = 1;
							Console.WriteLine ("Schema updated to 1");
							Db.Commit ();
							SetConfig (settings.DB_VERSION, db_version, Db);
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
							Console.WriteLine ("Schema updated to 2");
							Db.Commit ();
							SetConfig (settings.DB_VERSION, db_version, Db);
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("UpdateSchema to 2 {0}", ex);
							Db.Rollback ();
							return;
						}
					}
					if (db_version == 2) {
						//Migration 3 - Freind is now a class
						Db.BeginTransaction ();
						try {
							Db.DropTable<Friend> ();
							Db.CreateTable<Friend> ();
							db_version = 3;
							Console.WriteLine ("Schema updated to 3");
							Db.Commit ();
							SetConfig (settings.DB_VERSION, db_version, Db);
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("UpdateSchema to 3 {0}", ex);
							Db.Rollback ();
							return;
						}
					}
					if (db_version == 3) {
						//Migration 4 - Category table
						Db.BeginTransaction ();
						try {
							Db.DropTable<Cuisine> ();
							Db.CreateTable<Cuisine> ();
							db_version = 4;
							Console.WriteLine ("Schema updated to 4");
							Db.Commit ();
							SetConfig (settings.DB_VERSION, db_version, Db);
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("UpdateSchema to 4 failed {0}", ex);
							Db.Rollback ();
							return;
						}
					}
					if (db_version == 4) {
						//Migration 5 - Category table plus wipe
						Db.BeginTransaction ();
						try {
							deleteDb ();
							createDb ();
							db_version = 5;
							Console.WriteLine ("Schema updated to 5");
							Db.Commit ();
							SetConfig (settings.DB_VERSION, db_version, Db);
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ("UpdateSchema to 5 failed {0}", ex);
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

		public void LoadFromDb (String onlyWithCuisineType = null, LoadingPage loader = null)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					//load the data from the db
					Console.WriteLine ("Persist.LoadFromDb loading");

					if (loader != null)
						loader.SetMessage ("Loading from database", 0.1);
					Votes.Clear ();
					var votes_q = Db.Table<Vote> ();
					foreach (var vote in votes_q)
						Votes.Add (vote);
					
					//TODO: instead of clear() - http://forums.xamarin.com/discussion/19114/invalid-number-of-rows-in-section
					Places.Clear ();
					var place_q = Db.Table<Place> ();
					if (onlyWithCuisineType == null) {
						// all cuisine types
						Places.AddRange (place_q);
						Console.WriteLine ("Persist.LoadFromDb Range Added");
						if (loader != null)
							loader.SetMessage ("Calculating distances", 0.2);
						foreach (var p in Places) {
							if (string.IsNullOrEmpty (p.vote.cuisineName))
								p.IsDraft = true;
							p.CalculateDistanceFromPlace ();
						}
					} else
						//TODO: LINQ
						foreach (Place p in place_q) {
							if (p.vote.cuisineName == onlyWithCuisineType) {
								if (string.IsNullOrEmpty (p.vote.cuisineName))
									p.IsDraft = true;
								Places.Add (p);
								p.CalculateDistanceFromPlace ();
							}
						}
					Console.WriteLine ("LoadFromDb SORT");
					Places.Sort ();
					if (loader != null)
						loader.SetMessage ("Loading votes", 0.3);
					
					Console.WriteLine ("Persist.LoadFromDb loaded");
					if (loader != null)
						loader.SetMessage ("Loading friends", 0.4);
					var friends_q = Db.Table<Friend> ();
					foreach (var friend in friends_q)
						Friends [friend.Key] = new Friend (friend.Name, friend.Key);
					MyId = (long)GetConfigInt (settings.MY_ID);
				} catch (Exception ex) {
					Insights.Report (ex);
					Console.WriteLine ("Persist.LoadFromDb {0}", ex.Message);
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
					if (_categories != null)
						_categories.Clear ();
					Db.DeleteAll<Cuisine> ();
					Db.Commit ();
				} catch (Exception ex) {
					Insights.Report (ex);
					Db.Rollback ();
				}
			}
		}

		void LoadCategoriesFromDb ()
		{
			Console.WriteLine ("LoadCategoriesFromDb");
			_categories = new List<Cuisine> ();
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				var cat_q = Db.Table<Cuisine> ();
				foreach (Cuisine c in cat_q) {
					_categories.Add (c);
				}
			}
			if (_categories.Count == 0)
				LoadCategories ();
		}

		public void LoadCategories ()
		{
			var conn = GetWebConnection ();
			var response = conn.get ("/getCuisines_ajax", getRetries: 1);
			if (response == null) {
				//OFFLINE
				LoadCategoriesFromDb ();
				return;
			}
			string result = response.Content;
			lock (Lock) {
				try {
					JObject obj = JObject.Parse (result);
					var cat_strings = JsonConvert.DeserializeObject<List<string>> (obj.SelectToken ("categories").ToString ());
					if (cat_strings.Count == 0)
						LoadCategoriesFromDb ();
					using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
						Db.DeleteAll<Cuisine> ();
						_categories = new List<Cuisine> ();
						foreach (string c in cat_strings) {
							Cuisine cat = new Cuisine{ Title = c, };
							Db.InsertOrReplace (cat);
							_categories.Add (cat);
						} 
					}
					Console.WriteLine ("LoadCategories OK");
				} catch (Exception ex) {
					Insights.Report (ex);
					restConnection.LogErrorToServer ("Persist.LoadCategories Exception {0}", ex);
				}
			}
		}

		static void createDb ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.CreateTable<Cuisine> ();
				Db.CreateTable<Vote> ();
				Db.CreateTable<Place> ();
				Db.CreateTable<Friend> ();
				if (!TableExists<Configuration> (Db))
					Db.CreateTable<Configuration> ();
			}
		}

		static void deleteDb ()
		{
			// except config table
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.DropTable<Vote> ();
				Db.DropTable<Place> ();
				Db.DropTable<Friend> ();
				try {
					Db.DropTable<Cuisine> ();
				} catch (NotSupportedException) {
				}
				;
			}
		}


		public void saveVotesToDb ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BusyTimeout = DbTimeout;
				try {
					Db.BeginTransaction ();
					Db.DeleteAll<Vote> ();
					foreach (Vote v in Votes) {
						Db.Insert (v);
					}
					Db.Commit ();
				} catch (Exception ex) {
					Db.Rollback ();
					Insights.Report (ex);
				}

			}
		}

		#endregion

		#region Sync Methods

		void StartTimerIfOffline ()
		{
			if (Online) {
				_onlineTimer.Close ();
				return;
			}
			Console.WriteLine ("StartTimerIfOffline START");
			_onlineTimer = new System.Timers.Timer ();
			//Trigger event every 5 second
			_onlineTimer.Interval = 5000;
			_onlineTimer.Elapsed += OnOnlineTimerTrigger;
			_onlineTimer.Enabled = true;
		}

		private void OnOnlineTimerTrigger (object sender, System.Timers.ElapsedEventArgs e)
		{
			if (Online) {
				try {
					_onlineTimer.Close ();
					// update drafts
					Console.WriteLine ("Persist StartTimerIfOffline  ONLINE");

				} catch (Exception ex) {
					Insights.Report (ex);
					Console.WriteLine ("OnOnlineTimerTrigger OFFLINE again?");
					Online = false;
					_onlineTimer.Start ();
				}
			}
		}

		static void CheckServerVersionCorrect (JObject obj)
		{
			string appVersion = ServerPicker.GetServerVersion ();
			string serverVersion = "None";
			try {
				serverVersion = obj ["version"].ToString ();
				if (appVersion == serverVersion) {
					//good
					Console.WriteLine ("CheckServerVersionCorrect Good {0}", serverVersion);
					return;
				}
			} catch (Exception ex) {
				//ignore
				Console.WriteLine ("CheckServerVersionCorrect Exc. {0}", ex);
			}
			Console.WriteLine ("WRONG SERVER");
			Debug.WriteLine ("WRONG SERVER");
			throw new ProtocolViolationException (string.Format ("App: {0} != Server {1}", appVersion, serverVersion));
		}


		void StoreFullUserRecord (IRestResponse resp)
		{
			try {
				Console.WriteLine ("StoreFullUserRecord: lock get full");
				JObject obj = JObject.Parse (resp.Content);
				SetConfig ("is_admin", obj ["admin"].ToString ());
				MyId = obj ["id"].Value<Int64> ();
				SetConfig (settings.MY_ID, MyId);
				CheckServerVersionCorrect (obj);
				string placeStr = obj ["places"].ToString ();
				Dictionary<string, Place> place_list = JsonConvert.DeserializeObject<Dictionary<string, Place>> (placeStr);
				lock (Lock) {
					try {
						Places = place_list.Values.ToList ();
						Console.WriteLine ("StoreFullUserRecord SORT");
						Places.Sort ();
						Console.WriteLine ("StoreFullUserRecord sorted");
						Votes.Clear ();
						foreach (JObject fr in obj ["friendsData"]) {
							string fr_id = fr ["id"].ToString ();
							string name = fr ["name"].ToString ();
							Friends [fr_id] = new Friend (name, fr_id);
						}
						Console.WriteLine ("StoreFullUserRecord friends stored");
						int count = obj ["votes"].Count ();
						for (int i = 0; i < count; i++) {
							try {
								Vote v = obj ["votes"] [i].ToObject<Vote> ();
								Votes.Add (v);
							} catch (Exception ex) {
								Debug.WriteLine ("StoreFullUserRecord {0} Bad Structure: {1}", i, ex);
							}
						}
//						List<Vote> vote_list = obj ["votes"].ToObject< List<Vote> > ();
						if (Votes != null) {
							saveVotesToDb ();
						}
						Console.WriteLine ("StoreFullUserRecord votes stored");
						//sort
						updatePlaces (MyId.ToString ());
					} catch (Exception ex) {
						Console.WriteLine ("StoreFullUserRecord lock Exception {0}", ex);
						Insights.Report (ex);
						restConnection.LogErrorToServer ("StoreFullUserRecord lock Exception {0}", ex);
					}
				}
				Online = true;
				Console.WriteLine ("StoreFullUserRecord loaded");
			} catch (ProtocolViolationException ex) {
				throw;
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("StoreFullUserRecord Exception {0}", ex);
			}
		}

		// for an incremental query - since a time
		void StoreUpdatedUserRecord (IRestResponse resp)
		{
			try {
				Console.WriteLine ("StoreUpdatedUserRecord: lock ");
				JObject obj = JObject.Parse (resp.Content);
				CheckServerVersionCorrect (obj);
				MyId = obj ["id"].Value<Int64> ();
				SetConfig (settings.MY_ID, MyId);
				SetConfig ("is_admin", obj ["admin"].ToString ());
				string placeStr = obj ["places"].ToString ();
				Dictionary<string, Place> place_list = JsonConvert.DeserializeObject<Dictionary<string, Place>> (placeStr);
				lock (Lock) {
					try {
						bool Added = false;
						foreach (KeyValuePair<String,Place> kvp in place_list) {
							Added = false;
							for (int PlacesIdx = 0; PlacesIdx < Places.Count (); PlacesIdx++) {
								Place p = Places [PlacesIdx];
								if (string.IsNullOrEmpty (p.vote.cuisineName)) {
									p.IsDraft = true;
									continue;
								}
								if (p.key == kvp.Key) {
									kvp.Value.IsDraft = false;
									Places [PlacesIdx] = kvp.Value;
									Added = true;
									break;
								}
							}
							if (!Added)
								Places.Add (kvp.Value);
						}
						foreach (JObject fr in obj ["friendsData"]) {
							string fr_id = fr ["id"].ToString ();
							string name = fr ["name"].ToString ();
							Friends [fr_id] = new Friend (name, fr_id);
							Console.WriteLine ("StoreUpdatedUserRecord: Friend {0}", name);
						}
						List<Vote> vote_list = obj ["votes"].ToObject<List<Vote> > ();
						if (vote_list != null) {
							foreach (Vote v in vote_list) {
								var existing_vote = Votes
									.Where (old_vote => 
										old_vote.key == v.key &&
								                    old_vote.voter == v.voter)
									.SingleOrDefault ();
								if (existing_vote == null) {
									Votes.Add (v);
									existing_vote = v;
								} else {
									existing_vote.comment = v.comment;
									existing_vote.vote = v.vote;
									existing_vote.when = v.when;
									existing_vote.kind = v.kind;
									existing_vote.style = v.style;
									existing_vote.cuisine = v.cuisine;
									existing_vote.place_name = v.place_name;
								}
								if (v.voter == MyId.ToString ()) {
									var p =	GetPlace (v.key);
									if (v.vote == VoteValue.Liked)
										p.up -= 1;
									if (v.vote == VoteValue.Disliked)
										p.down -= 1;
									p.vote = v;
									p.setComment (v.comment);
								}
							}
						}
						saveVotesToDb ();
					} catch (Exception ex) {
						Insights.Report (ex);
						restConnection.LogErrorToServer ("StoreUpdatedUserRecord lock Exception {0}", ex);
					}
					//sort
					updatePlaces (MyId.ToString ());
					var updated_dict = new Dictionary<string,string> ();
					updated_dict.Add ("userId", MyId.ToString ());
					Console.WriteLine ("StoreUpdatedUserRecord clear updates");
					restConnection.Instance.post ("clear_user_updates", updated_dict);
				}
				Online = true;

				Console.WriteLine ("StoreUpdatedUserRecord loaded");
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("StoreUpdatedUserRecord Exception {0}", ex);
			}
		}

		void UpdateCategoryCounts ()
		{
			try {
				//			_categoryCounts
				var withACat = (from p in Places
				                where !string.IsNullOrEmpty (p.vote.cuisineName)
				                select p).ToList ();
				var x = withACat.GroupBy (p => p.vote.cuisineName);
				var y = x.Select (group => new { 
					Metric = group.Key, 
					Count = group.Count () 
				})
					.OrderBy (counted => counted.Metric);
				_categoryCounts = y.ToDictionary (item => item.Metric, item => item.Count);
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("UpdateCategoryCounts Exception {0}", ex);
			}
		}

		static IRestResponse InnerGetUserData (DateTime? since, restConnection webReq)
		{
			IRestResponse resp;
			Dictionary<String, String> paramList = new Dictionary<String, String> ();
			if (since != null) {
				paramList.Add ("since", ((DateTime)since).ToString ("s"));
			}
			resp = webReq.get ("/getFullUserRecord", paramList);
			if (resp == null || resp.ResponseStatus == ResponseStatus.Error) {
				//unable to contact server
				Console.WriteLine ("InnerGetUserData - NO RESPONSE");
				return null;
			}
			return resp;
		}

		public restConnection GetWebConnection ()
		{
			restConnection conn = restConnection.Instance;
			string server = GetConfig (settings.SERVER);
			if (string.IsNullOrEmpty (server)) {
				server = settings.DEFAULT_SERVER;
			} 
			conn.setBaseUrl (server);
			conn.setCredentials (GetConfig (settings.USERNAME), GetConfig (settings.PASSWORD), "");
			return conn;
		}

		public delegate void StatusMessageDelegate (string message, double progress);

		public delegate void BasicDelegate ();

		// Throws Protocol Exception if server version doesn't match
		public void GetUserData (BasicDelegate onFail,
		                         BasicDelegate onSucceed,
		                         DateTime? since = null,
		                         bool incremental = false,
		                         StatusMessageDelegate statusMessage = null)
		{
			if (incremental) {
				if (since == null) {
					DateTime? last = GetConfigDateTime (settings.LAST_SYNC);
					if (last != null) {
						since = last;
					}
				}
			}
			restConnection webReq = GetWebConnection ();
			if (webReq != null) {
				if (statusMessage != null)
					statusMessage ("Checking network", 0.5);
				IRestResponse resp;
				try {
					if (statusMessage != null)
						statusMessage ("Contacting server", 0.7);
					
					resp = InnerGetUserData (since, webReq);
					if (since != null) {
						// incremental
						if (statusMessage != null)
							statusMessage ("Storing update", 0.9);
						StoreUpdatedUserRecord (resp);
						if (Persist.Instance.Places.Count == 0) {
							resp = InnerGetUserData (null, webReq);
							StoreFullUserRecord (resp);
						}
					} else {
						if (statusMessage != null)
							statusMessage ("Storing data", 0.9);
						StoreFullUserRecord (resp);
					}
					SortPlaces ();
					SetConfig (settings.LAST_SYNC, DateTime.UtcNow);
				} catch (UnauthorizedAccessException) {
					// not logged in
					if (onFail != null)
						Device.BeginInvokeOnMainThread (() => {
							onFail.DynamicInvoke ();
						});
					Console.WriteLine ("GetFullData: No login");
					return;
				}
				if (onSucceed != null)
					Device.BeginInvokeOnMainThread (() => {
						onSucceed.DynamicInvoke ();
					});
			}
		}

		#endregion

		#region Place methods

		public static void RemovePlaceKeyFromDb (string deleteKey)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				try {
					Db.BeginTransaction ();
					var cmd = Db.CreateCommand (String.Format ("delete from Place where key='{0}'", deleteKey));
					cmd.ExecuteNonQuery ();
					Db.Commit ();
				} catch (Exception ex) {
					Db.Rollback ();
					Insights.Report (ex);
				}
			}
		}

		public void DeletePlace (Place place)
		{
			try {
				Place StoredPlace = (from p in Places
				                     where p.key == place.key
				                     select p).FirstOrDefault ();
				if (StoredPlace != null) {
					Places.Remove (place);
					RemovePlaceKeyFromDb (StoredPlace.key);
				}
			} catch (Exception ex) {
				Insights.Report (ex);
				restConnection.LogErrorToServer ("DeletePlace", ex);
			}
		}

		/**
		 * Sort Places by Distance from me and update DB
		 */
		public void updatePlaces (string myId, Position? searchCentre = null)
		{
			using (SQLiteConnection db = new SQLiteConnection (DbPath)) {
				db.BusyTimeout = DbTimeout;
				var removeList = new List<Place> ();
				foreach (Place p in Places) {
					p.CalculateDistanceFromPlace (searchCentre);
					db.InsertOrReplace (p);
					p.up = p.down = 0;
					var vote_list = Votes.Where (v => v.key == p.key && v.vote != VoteValue.None).ToList ();
					if (vote_list.Count == 0)
						removeList.Add (p);
					else
						vote_list.ForEach (v => {
							if (v.voter == myId) {
								// my vote
								p.vote = v;
							} else {
								//friend vote
								if (v.vote == VoteValue.Liked)
									p.up++;
								else if (v.vote == VoteValue.Disliked)
									p.down++;
							}
						});
				}
				foreach (Place p in removeList)
					Places.Remove (p);
				UpdateCategoryCounts ();
				Console.WriteLine ("updatePlaces SORT");
				Places.Sort ();
//					foreach (Vote v in Votes) {
//						try {
//							//Todo: does this allow n votes per place?
//							var found_v = (from fv in db.Table<Vote> ()
//							               where fv.key == v.key
//							               select fv).FirstOrDefault ();
//							if (found_v == null)
//								db.Insert (v);
//							//Db.InsertOrReplace (v);
//						} catch (Exception E) {
//							Insights.Report (E);
//							Console.WriteLine ("updatePlaces Exception: {0}", E.Message);
//							restConnection.LogErrorToServer ("updatePlaces Exception: {0}", E.Message);
//						}
//					}
					
				foreach (KeyValuePair<string, Friend> f in Friends) {
					try {
						db.InsertOrReplace (f.Value);
						Debug.WriteLine (f.Value.Name);
					} catch (Exception ex) {
						Insights.Report (ex);
						Console.WriteLine ("Persist.updatePlaces: Friends {0}", ex);
					}
				}
			}
		}

		/**
		 * Sort Place by Distance from me
		 */
		public void SortPlaces (List<Place> placeList = null, Position? updateDistancePosition = null)
		{
			Console.WriteLine ("SortPlaces");
			if (placeList == null)
				placeList = Places;
			if (updateDistancePosition != null) {
				if (GpsPosition.Latitude == 0.0) {
					GpsPosition = new Position (
						Persist.Instance.GetConfigDouble (settings.LAST_LAT),
						Persist.Instance.GetConfigDouble (settings.LAST_LNG));
				}
				foreach (Place p in placeList) {
					p.CalculateDistanceFromPlace (updateDistancePosition);
				}
			}
			Console.WriteLine ("SortPlaces SORT");
			placeList.Sort ();

			Console.WriteLine ("SortPlaces SORTED");

		}

		void StorePlace (Place place, Place removePlace = null)
		{
			if (string.IsNullOrEmpty (place.key)) {
				place.key = Guid.NewGuid ().ToString ();
				place.IsDraft = true;
			}

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
					Console.WriteLine ("StorePlace ROLLBACK");
					Db.Rollback ();
					Insights.Report (ex);
				}
			}
		}

		public bool UpdatePlace (Place place)
		{
			Debug.WriteLine ("UpdatePlaces");
			// calc dist
			place.CalculateDistanceFromPlace ();
			for (int i = 0; i < Places.Count (); i++) {
				if (Places [i].key == place.key) {
					try {
						StorePlace (place, removePlace: Places [i]);
						return UpdateVote (place);
					} catch (Exception e) { 
						Insights.Report (e);
						restConnection.LogErrorToServer ("** UpdatePlace ROLLBACK : '{0}'", e);
						return false;
					}
				}
			}
			StorePlace (place);
			return true;
		}

		public bool UpdateVote (Place place)
		{
			Debug.WriteLine ("UpdateVote");
			string myIdString = MyId.ToString ();
			var myVote = (from v in Votes
			              where v.voter == myIdString &&
			                  v.key == place.key
			              select v).FirstOrDefault ();
			if (myVote == null) {
				myVote = new Vote ();
				myVote.voter = myIdString;
				myVote.key = place.key;
				Votes.Add (myVote);
			}
			myVote.vote = place.vote.vote;
			myVote.kind = place.vote.kind;
			myVote.style = place.vote.style;
			myVote.cuisine = place.vote.cuisine;
			myVote.comment = place.descr;
			saveVotesToDb ();
			return true;
		}

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

		#endregion


		#region Config Methods

		/// <summary>
		/// returns "" by default
		/// </summary>
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
//					restConnection.LogErrorToServer ("GetConfig: {0} not found", key);
				}
				return "";
			}
		}

		/// <summary>
		/// returns 0/0 by default
		/// </summary>
		public Double GetConfigDouble (string key)
		{
			try {
				return Convert.ToDouble (GetConfig (key));
			} catch {
				return 0.0;
			}
		}

		/// <summary>
		/// returns false by default
		/// </summary>
		public bool GetConfigBool (string key)
		{
			try {
				string value = GetConfig (key);
				switch (value) {
					case "true":
					case "True":
					case "Yes":
					case "yes":
					case "1":
						return true;
					case "false":
					case "False":
					case "No":
					case "no":
					case "0":
					default:
						return false;
				}
			} catch {
				return false;
			}
		}

		/// <summary>
		/// returns null if not found or invalid
		/// </summary>
		public DateTime? GetConfigDateTime (string key)
		{
			try {
				return Convert.ToDateTime (GetConfig (key));
			} catch {
				return null;
			}
		}

		/// <summary>
		/// returns 0/0 by default
		/// </summary>
		public Double GetConfigInt (string key)
		{
			try {
				return Convert.ToInt64 (GetConfig (key));
			} catch {
				return 0;
			}
		}

		void innerSetConfig (string key, string value, SQLiteConnection Db)
		{
			
			Db.BusyTimeout = DbTimeout;
			try {
				if (value == null) {
					//delete
					Db.Delete (key);
				} else {
					Db.InsertOrReplace (new Configuration (key, value));
				}
			} catch (System.NotSupportedException ex) {
				Console.WriteLine ("innerSetConfig NotSupportedException {0}", key);
			} catch (Exception ex) {
				Insights.Report (ex, key, value);
			}
		}

		public void SetConfig (string key, string value, SQLiteConnection db = null)
		{
			if (db != null) {
				innerSetConfig (key, value, db);
			} else
				using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
					innerSetConfig (key, value, Db);
				}
		}

		public void SetConfig (string key, int value, SQLiteConnection db = null)
		{
			SetConfig (key, value.ToString (), db);
		}

		public void SetConfig (string key, Double value, SQLiteConnection db = null)
		{
			SetConfig (key, Convert.ToString (value));
		}

		public void SetConfig (string key, Int64 value, SQLiteConnection db = null)
		{
			SetConfig (key, Convert.ToString (value));
		}

		public void SetConfig (string key, DateTime value, SQLiteConnection db = null)
		{
			SetConfig (key, value.ToUniversalTime ().ToString ("s"));
		}

		public void SetConfig (string key, bool value, SQLiteConnection db = null)
		{
			SetConfig (key, value.ToString ());
		}

		#endregion

		public Persist ()
		{
			Console.WriteLine ("Persist()");
			Votes = new List<Vote> ();
			Places = new List<Place> ();
			Friends = new Dictionary<string, Friend> ();
			SearchHistory = new PersistantQueue (3, "Search-History");
			Online = false;
			DbPath = Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				"database.db3");
			if (!File.Exists (DbPath)) {
				Console.WriteLine ("Persist: New Db");
				Insights.Track ("New Db");
				createDb ();
			} 
			Double Lat = GetConfigDouble ("LastLat");
			Double Lng = GetConfigDouble ("LastLng");
			GpsPosition = new Position (Lat, Lng);
		}
	}
}

