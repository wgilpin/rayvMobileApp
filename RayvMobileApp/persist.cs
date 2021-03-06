﻿using System;
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
	using CommentList = List<VoteComment>;

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
		// 5 seconds
		public TimeSpan DbTimeout = new TimeSpan (0, 0, 5);

		#region Fields

		private static string DbPath;
		public List<Vote> Votes;

		public List<Place> Places { get; set; }

		public RestResponse ServerResponseToBeProcessed { get; set; }

		public List<string> CuisineHistory;
		public List<Invite> InvitationsIn;
		public List<Invite> InvitationsOut;
		public List<Invite> Acceptances;
		public Dictionary<string,string> InviteNames;
		public PersistantQueueWithPosition SearchHistory;
		private List<Cuisine> _categories;
		private Dictionary<string, int> _categoryCounts;
		public Dictionary<string, Friend> Friends;
		public Position GpsPosition;
		private static Persist _instance;
		public string FilterWhoKey = "";
		public bool NotificationsReceived = false;
		public string OauthToken = "";
		public  Page OauthNavPage;

		// store the list of comments for a vote
		// voteId -> List<VoteComment>
		public Dictionary<long, CommentList> CommentCache;

		public Object Lock = new Object ();
		public List<Place> DisplayList;
		//		private bool _online = false;
		private System.Timers.Timer _onlineTimer;

		#endregion

		#region Properties

		public bool HaveActivity {
			get {
				return InvitationsIn.Count + Acceptances.Count > 0;
			}
		}

		public bool Online {
			get {
				return true;
//				try {
//					if (_online)
//						return true;
//					restConnection conn = GetWebConnection ();
//					var resp = conn.get ("/api/login", null, getRetries: 1);
//					if (resp == null) {
//						Console.WriteLine ("Online: Response NULL");
//						return false;
//					}
//					if (resp.ResponseStatus != ResponseStatus.Completed) {
//						Console.WriteLine ("Online: Bad Response {0}", resp.ResponseStatus);
//						return false;
//					}
//					_online = true;
//					return true;
//				} catch {
//					_online = false;
//					return false;
//				}
			}
			set { ; }

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
					if (db_version < settings.CurrentDbVersion) {
						//Migration  -  plus wipe
						Db.BeginTransaction ();
						try {
							deleteDb ();
							createDb ();
							//SetConfig (settings.SERVER, null);
							db_version = settings.CurrentDbVersion;
							Console.WriteLine ($"Schema updated to {settings.CurrentDbVersion}");
							Db.Commit ();
							SetConfig (settings.DB_VERSION, db_version);
							var server_url = "https://" +
							                 ServerPicker.GetServerVersionForAppVersion () +
							                 settings.SERVER_DEFAULT;
							Persist.Instance.SetConfig (settings.SERVER, server_url);
							return;
						} catch (Exception ex) {
							Insights.Report (ex);
							restConnection.LogErrorToServer ($"UpdateSchema to {settings.CurrentDbVersion} failed {ex}");
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
							SetConfig (settings.DB_VERSION, db_version);
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
							SetConfig (settings.DB_VERSION, db_version);
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
							SetConfig (settings.DB_VERSION, db_version);
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
							SetConfig (settings.DB_VERSION, db_version);
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
							SetConfig (settings.DB_VERSION, db_version);
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
			try {
				Instance.LoadCategoriesFromDb ();
				using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
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
						if (!string.IsNullOrEmpty (friend.Name))
							Friends [friend.Key] = new Friend (friend.Name, friend.Key);
					MyId = (long)GetConfigInt (settings.MY_ID);
				}
			} catch (UnauthorizedAccessException) {
				Console.WriteLine ("Persist.LoadFromDb - Not Logged In");
			} catch (Exception ex) {
				Insights.Report (ex);
				Console.WriteLine ("Persist.LoadFromDb {0}", ex.Message);
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

		public void SaveFriendsToDb ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BusyTimeout = DbTimeout;
				try {
					Db.BeginTransaction ();
					Db.DeleteAll<Friend> ();
					foreach (var f in Friends) {
						Db.Insert (f);
					}
					Db.Commit ();
				} catch (Exception ex) {
					Db.Rollback ();
					Insights.Report (ex);
				}

			}
		}

		//save a single vote into the Db
		public void SaveVoteToDb (Vote v)
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BusyTimeout = DbTimeout;
				try {
					Db.InsertOrReplace (v);
				} catch (Exception ex) {
					Debug.Fail ("SaveVoteToDb ** FAIL **", ex.Message);
					Insights.Report (ex);
				}
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
					Debug.Fail ("saveVotesToDb ** ROLLBACK **");
					Db.Rollback ();
					Insights.Report (ex);
				}

			}
		}

		public void savePlacesToDb ()
		{
			using (SQLiteConnection Db = new SQLiteConnection (DbPath)) {
				Db.BusyTimeout = DbTimeout;
				try {
					Db.BeginTransaction ();
					Db.DeleteAll<Place> ();
					foreach (Place p in Places) {
						Db.Insert (p);
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

		public List<VoteComment> LoadComments (Vote vote)
		{
			try {
				var p = new Dictionary<string, string> {
					{ "author",Persist.Instance.MyId.ToString () },
					{ "vote", vote.voteId.ToString () },
				};
				var resp = Persist.Instance.GetWebConnection ().get ("api/comments", p);
				if (resp == null || resp.ResponseStatus == ResponseStatus.Error) {
					return null;
				}
				JObject obj = JObject.Parse (resp.Content);
				var commentStr = obj ["comments"].ToString ();
				CommentList comments = JsonConvert.DeserializeObject<CommentList> (commentStr);
				if (comments == null) {
					if (vote.replies != 0)
						SaveVoteToDb (vote);
					vote.replies = 0;
					return new CommentList ();
				}
				if (vote.replies != comments.Count)
					SaveVoteToDb (vote);
				return comments;
			} catch (Exception ex) {
				Debug.Fail ($"LoadComments {ex.Message}");
				return new CommentList ();
			}
		}

		public bool SetComment (Vote Vote, string comment)
		{
			var p = new Dictionary<string, string> {
				{ "author",Persist.Instance.MyId.ToString () },
				{ "when",DateTime.UtcNow.ToString () },
				{ "comment", comment },
				{ "vote",Vote.voteId.ToString () }
			};
			var res = Persist.Instance.GetWebConnection ().post ("api/comment", p);
			return res == "OK";
		}

		public List<Place> GetData ()
		{
			if (ServerResponseToBeProcessed != null) {
				Console.WriteLine ("Persist GetData Do Process");
				StoreFullUserRecord (ServerResponseToBeProcessed);
			} else
				Console.WriteLine ("Persist GetData Existing");
			return Places;
		}

		public bool Unfriend (string friendKey)
		{
			Console.WriteLine ($"Unfriend {friendKey}");
			string serverResult = Persist.Instance.GetWebConnection ().post ("/api/friends/remove", "unfriend_id", friendKey);
			if (serverResult == "OK") {
				Persist.Instance.Friends.Remove (friendKey);
				var tempVotes = Persist.Instance.Votes.Where (v => v.voter != friendKey).ToList ();
				Persist.Instance.Votes = tempVotes;
				Console.WriteLine ("Friend data removed");
				return true;
			}
			return false;
		}

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

		static void IsServerVersionCorrect (JObject obj)
		{
			string appVersion = ServerPicker.GetServerVersion ();
			string serverVersion = "None";
			try {

				try {
					double minServerVersion = Convert.ToDouble (obj ["min_version"]);
					double currentAppVersion = Convert.ToDouble (appVersion);
					if (minServerVersion > 0 && currentAppVersion >= minServerVersion) {
						//good
						Console.WriteLine ("CheckServerVersionCorrect > Min {0}", serverVersion);
						return;
					}
				} catch (Exception ex) {
					// no min version
				}
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

		void ExtractInvites (JObject obj)
		{
			List<Invite> invOut = JsonConvert.DeserializeObject<List<Invite>> (obj ["sentInvites"].ToString ());
			List<Invite> invIn = JsonConvert.DeserializeObject<List<Invite>> (obj ["receivedInvites"].ToString ());
			if (invIn == null) {
				InvitationsIn = new List<Invite> ();
				Acceptances = new List<Invite> ();
			} else {
				InvitationsIn = invIn.Where (i => i.accepted == false).ToList ();
				Acceptances = invIn.Where (i => i.accepted == true).ToList ();
			}
			InvitationsOut = invOut == null ?
				new List<Invite> () :
				InvitationsOut = invOut.Where (i => i.accepted == false).ToList ();
			foreach (var inv in invIn)
				InviteNames [inv.inviter] = inv.name;
		}

		bool syncServerFriendsdata (JObject obj)
		{
			var oldFriends = new Dictionary<string, Friend> ();
			foreach (var kvp in Friends)
				oldFriends.Add (kvp.Key, kvp.Value);
			Friends.Clear ();
			bool NewFriend = false;
			foreach (JObject fr in obj ["friendsData"]) {
				var f_name = fr ["name"].ToString ();
				if (!string.IsNullOrEmpty (f_name)) {
					string fr_id = fr ["id"].ToString ();
					string name = f_name;
					Friends [fr_id] = new Friend (name, fr_id);
					if (!oldFriends.ContainsKey (fr_id))
						//new friend
						NewFriend = true;
				}
			}

			SaveFriendsToDb ();
			if (NewFriend)
				Console.WriteLine ("New Friend found");
			return NewFriend;
		}

		void StoreFullUserRecord (IRestResponse resp)
		{
			try {
				var now = DateTime.Now;
				Console.WriteLine ($"StoreFullUserRecord  {now} ");
				JObject obj = JObject.Parse (resp.Content);
				SetConfig ("is_admin", obj ["admin"].ToString ());
				MyId = obj ["id"].Value<Int64> ();
				SetConfig (settings.MY_ID, MyId);
				IsServerVersionCorrect (obj);
				string placeStr = obj ["places"].ToString ();
				Dictionary<string, Place> place_list = JsonConvert.DeserializeObject<Dictionary<string, Place>> (placeStr);
				lock (Lock) {
					try {
						Console.WriteLine ($"StoreFullUserRecord 1 {DateTime.Now - now} ");

						Places = place_list.Values.ToList ();
						Console.WriteLine ($"StoreFullUserRecord 2 {DateTime.Now - now} ");

						Places.Sort ();
						Console.WriteLine ($"StoreFullUserRecord 3 {DateTime.Now - now} ");

						Votes.Clear ();
						syncServerFriendsdata (obj);
						ExtractInvites (obj);
						Console.WriteLine ($"StoreFullUserRecord 4 {DateTime.Now - now} ");
						int count = obj ["votes"].Count ();
						for (int i = 0; i < count; i++) {
							try {
								Vote v = obj ["votes"] [i].ToObject<Vote> ();
								if (v.cuisine == null)
									Console.WriteLine ($"{v.place_name} no cuisine");
								var matchedVotes = Votes.Where (v2 => v2.key == v.key && v2.voter == v.voter);
								if (matchedVotes.Count () > 0)
									Insights.Track ("Duplicate vote", "key", v.key);
								else
									Votes.Add (v);
							} catch (Exception ex) {
								Debug.WriteLine ("StoreFullUserRecord {0} Bad Structure: {1}", i, ex);
							}
						}
//						List<Vote> vote_list = obj ["votes"].ToObject< List<Vote> > ();
						Console.WriteLine ($"StoreFullUserRecord 5 {DateTime.Now - now} ");

						if (Votes != null) {
							saveVotesToDb ();
						}
						Console.WriteLine ($"StoreFullUserRecord 6 {DateTime.Now - now} ");

						//sort
						updatePlaces (MyId.ToString ());
					} catch (Exception ex) {
						Console.WriteLine ("StoreFullUserRecord lock Exception {0}", ex);
						Insights.Report (ex);
						restConnection.LogErrorToServer ("StoreFullUserRecord lock Exception {0}", ex);
					}
				}
				Online = true;
				Console.WriteLine ($"StoreFullUserRecord 7 {DateTime.Now - now} ");

			} catch (ProtocolViolationException pvEx) {
				Debug.Fail ($"Wrong Server Version {pvEx.Message}");
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
				var now = DateTime.Now;
				Console.WriteLine ($"StoreUpdatedUserRecord {now} ");
				JObject obj = JObject.Parse (resp.Content);
				IsServerVersionCorrect (obj);
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
								// for loop as we want the index
								Place p = Places [PlacesIdx];
								if (string.IsNullOrEmpty (p.vote.cuisineName)) {
									Insights.Track ("Place with no cuisine", "PlaceName", p.place_name);
									continue;
								} 
								if (p.key == kvp.Key) {
									kvp.Value.IsDraft = false;
									Places [PlacesIdx] = kvp.Value;
									Added = true;
									break;
								}
							}
							if (!Added) {
								Places.Add (kvp.Value);
								Console.WriteLine ($"Added {kvp.Value.place_name} {kvp.Key}");
							}
						}
						Console.WriteLine ($"StoreUpdatedUserRecord 1 {DateTime.Now - now} ");

						if (syncServerFriendsdata (obj)) {
							Console.WriteLine ($"StoreUpdatedUserRecord 1a {DateTime.Now - now} ");
							throw new OperationCanceledException ("New friends found");
						}
						Console.WriteLine ($"StoreUpdatedUserRecord 1b {DateTime.Now - now} ");
						ExtractInvites (obj);
						Console.WriteLine ($"StoreUpdatedUserRecord 2 {DateTime.Now - now} ");

						List<Vote> vote_list = new List<Vote> ();
						List<string> vote_list_strings = JsonConvert.DeserializeObject<List<string>> (obj ["votes"].ToString ());
						foreach (string vote_string in vote_list_strings) {
							Vote v = JsonConvert.DeserializeObject<Vote> (vote_string);
							var p = GetPlace (v.key);
							var vote_q = Votes.Where (old_vote => 
			                     old_vote.key == v.key &&
							             old_vote.voter == v.voter);
							var existing_vote = vote_q.SingleOrDefault ();
							if (existing_vote == null) {
								Votes.Add (v);
								p.vote = v;
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
								p.vote = v;
								p.setComment (v.comment);
							}
						}
						Console.WriteLine ($"StoreUpdatedUserRecord 3 {DateTime.Now - now} ");

						var debugDrafts = false;
						Places.Where (p => p.vote.cuisine == null).ToList ().ForEach (p => {
							Insights.Track ("Place with no cuisine", "PlaceName", p.place_name);
							Console.WriteLine ($"Place with no cuisine {p.place_name}" );
							p.IsDraft = true;
							debugDrafts = true;
						});
						if (debugDrafts)
							Console.WriteLine (resp.Content);
						Console.WriteLine ($"StoreUpdatedUserRecord 4 {DateTime.Now - now} ");

						saveVotesToDb ();
					} catch (OperationCanceledException) {
						throw;
					} catch (Exception ex) {
						Insights.Report (ex);
						restConnection.LogErrorToServer ("StoreUpdatedUserRecord lock Exception {0}", ex);
					}
					//sort
					Console.WriteLine ($"StoreUpdatedUserRecord 5 {DateTime.Now - now} ");

					updatePlaces (MyId.ToString ());
					Console.WriteLine ($"StoreUpdatedUserRecord 7 {DateTime.Now - now} ");
				}
				Online = true;

				Console.WriteLine ("StoreUpdatedUserRecord loaded");
			} catch (OperationCanceledException) {
				throw;
			} catch (ProtocolViolationException) {
				throw;
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

		static IRestResponse InnerGetUserData (DateTime? since, restConnection webReq, int timeout = 0)
		{
			IRestResponse resp;
			Dictionary<String, String> paramList = new Dictionary<String, String> {
				{ "version", ServerPicker.GetServerVersion () }
			};
			if (since != null) {
				paramList.Add ("since", ((DateTime)since).ToString ("s"));
			}
			var timeNow = DateTime.Now;
			resp = webReq.get ("/getFullUserRecord", paramList, timeout: timeout);
			if (resp == null || resp.ResponseStatus == ResponseStatus.Error) {
				//unable to contact server
				Console.WriteLine ($"InnerGetUserData - NO RESPONSE after {DateTime.Now - timeNow}");
				return null;
			}
			return resp;
		}

		public restConnection GetWebConnection ()
		{
			restConnection conn = restConnection.Instance;
			string server = GetConfig (settings.SERVER);
			if (string.IsNullOrEmpty (server)) {
				server = $"https://{settings.SERVER_DEFAULT}";
			} 
			try {
				conn.setBaseUrl (server);
				conn.setCredentials (GetConfig (settings.USERNAME), GetConfig (settings.PASSWORD), "");
				return conn;
			} catch {
				return null;
			}
		}

		public delegate void StatusMessageDelegate (string message, double progress);

		public delegate void BasicDelegate ();

		// Throws Protocol Exception if server version doesn't match
		public void GetUserData (BasicDelegate onFail,
		                         BasicDelegate onSucceed,
		                         BasicDelegate onFailVersion,
		                         BasicDelegate onFailLogin = null,
		                         DateTime? since = null,
		                         bool incremental = false,
		                         StatusMessageDelegate setStatusMessage = null,
		                         int timeoutMs = 0)
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
				IRestResponse resp;
				try {
					Console.WriteLine ("GetUserData: Contacting server");
					setStatusMessage?.Invoke ("Contacting server", 0.7);
					resp = InnerGetUserData (since, webReq, timeoutMs);
					if (resp == null) {
						Debug.WriteLine ("GetUserData: NO RESPONSE");
						onFail?.Invoke ();
						return;
					}
					if (resp.Content == "BAD_VERSION") {
						Debug.WriteLine ("GetUserData: BAD_VERSION");
						onFailVersion?.Invoke ();
						return;
					}
					if (since != null) {
						// incremental
						Console.WriteLine ("GetUserData: Storing update");
						setStatusMessage?.Invoke ("Storing Update", 0.9);
						try {
							Console.WriteLine ("GetUserData call StoreUpdatedUserRecord");
							StoreUpdatedUserRecord (resp);
						} catch (OperationCanceledException) {
							Console.WriteLine ("GetUserData OperationCanceledException");
							if (Persist.Instance.Places.Count > 0) {
								//don't do this is places.count == 0 as that is caugh by the next test
								resp = InnerGetUserData (null, webReq);
								StoreFullUserRecord (resp);
							}
						}
						if (Persist.Instance.Places.Count == 0) {
							// assume the partial update has failed if there are no places
							resp = InnerGetUserData (null, webReq);
							StoreFullUserRecord (resp);
						}
					} else {
						Console.WriteLine ("GetUserData Storing full");
						setStatusMessage?.Invoke ("Storing data", 0.9);
						StoreFullUserRecord (resp);
					}
					SortPlaces ();
					SetConfig (settings.LAST_SYNC, DateTime.UtcNow);
				} catch (UnauthorizedAccessException) {
					// not logged in
					Device.BeginInvokeOnMainThread (() => {
						var handler = onFailLogin ?? onFail;
						handler?.Invoke ();
					});
					Console.WriteLine ("GetFullData: No login");
					return;
				} catch (ProtocolViolationException) {
					Device.BeginInvokeOnMainThread (() => {
						onFailVersion?.DynamicInvoke ();
					});
					return;
				}
				if (onSucceed != null)
					Device.BeginInvokeOnMainThread (() => {
						onSucceed?.DynamicInvoke ();
					});
			}
		}

		public void SetNotificationToken (string token)
		{
			// write the notification token to the server and to app config
			try {
				Persist.Instance.SetConfig (settings.NOTIFICATIONS_TOKEN, token);
				restConnection webReq = GetWebConnection ();
				var values = new Dictionary<string,string> {
					{ "token", token },
					{ "kind",Device.OnPlatform ("iOS", "Android", "WinPhone") }
				};
				webReq.post ("/apns/register", values);
				Console.WriteLine ("SetNotificationToken");
			} catch (Exception ex) {
				Console.WriteLine ($"ERROR SetNotificationToken {ex}");
				Insights.Report (ex);
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
			var now = DateTime.Now;
			Console.WriteLine ($"updatePlaces {now}");
			using (SQLiteConnection db = new SQLiteConnection (DbPath)) {
				db.BusyTimeout = DbTimeout;
				Console.WriteLine ($"updatePlaces 1 {DateTime.Now-now}");

				lock (Lock) {
					Dictionary<string, Place> PlacesByKey = new Dictionary<string, Place> ();
					try {
						//add all Places to PlacesByKey, with correct distance
						foreach (Place p in Places) {
							p.CalculateDistanceFromPlace (searchCentre);
							p.adjusted = false;
							if (!PlacesByKey.ContainsKey (p.key))
								PlacesByKey.Add (p.key, p);
						}
						Console.WriteLine ($"updatePlaces 2 {DateTime.Now-now}");
						foreach (Vote v in Votes) {
							try {
								if (PlacesByKey.ContainsKey (v.key)) {
									Place p = PlacesByKey [v.key];
									if (v.voter == myId) {
										// my vote
										p.vote = v;
									} else {
										//friend vote

										if (p.vote == null)
											p.vote = v;
									}
									p.adjusted = true;
								}
							} catch (Exception ex) {
								Insights.Report (ex, "Vote", v.Id.ToString ());
							}
						
						}
						Console.WriteLine ($"updatePlaces 2a {DateTime.Now-now}");

						Places.Clear ();
						try {
							db.BeginTransaction ();
							db.DeleteAll<Place> ();
							foreach (var kvp in PlacesByKey) {
								if (kvp.Value.adjusted) {
//									Places.Add (kvp.Value);
									db.Insert (kvp.Value);
								}
							}
							db.Commit ();
						} catch (Exception ex) {
							db.Rollback ();
							Console.WriteLine ("UpdatePlaces ROLLBACK");
							Insights.Report (ex);
						}
						foreach (var kvp in PlacesByKey) {
							if (kvp.Value.adjusted)
								Places.Add (kvp.Value);
						}

					} catch (Exception ex) {
						Console.WriteLine (ex);
						Insights.Report (ex);
					} finally {
						PlacesByKey.Clear ();
					}
					Console.WriteLine ($"updatePlaces 3 {DateTime.Now-now}");
					UpdateCategoryCounts ();
					Console.WriteLine ($"updatePlaces 4 {DateTime.Now-now}");
					Places.Sort ();
					Console.WriteLine ($"updatePlaces 5 {DateTime.Now-now}");
				}//					foreach (Vote v in Votes) {
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
						if (!string.IsNullOrEmpty (f.Value.Name)) {
							db.InsertOrReplace (f.Value);
							Debug.WriteLine (f.Value.Name);
						}
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
			var now = DateTime.Now;
			Console.WriteLine ($"SortPlaces {now}");
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
			Console.WriteLine ($"SortPlaces SORT {DateTime.Now - now}");
			placeList.Sort ();
			Console.WriteLine ($"SortPlaces SORTED {DateTime.Now - now}");

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
						return UpdateVoteForPlace (place);
					} catch (Exception e) { 
						Insights.Report (e);
						restConnection.LogErrorToServer ("** UpdatePlace ROLLBACK : '{0}'", e);
						return false;
					}
				}
			}
			StorePlace (place);
			return UpdateVoteForPlace (place);
		}

		public bool UpdateVoteForPlace (Place place)
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
			myVote.untried = place.vote.untried;
			myVote.kind = place.vote.kind;
			myVote.style = place.vote.style;
			myVote.cuisine = place.vote.cuisine;
			myVote.comment = place.Comment ();
			saveVotesToDb ();
			place.vote = myVote;
			return true;
		}

		public Place GetPlace (string key)
		{
			var place = Places.FirstOrDefault (p => p.key == key);
			if (place == null)
				Console.WriteLine ($"GetPlace {key} = NULL");
			return place;
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
			try {
				if (Application.Current.Properties.ContainsKey (key))
					return Application.Current.Properties [key] as string;
			} catch (Exception) {
			}
			return "";
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
		public int GetConfigInt (string key)
		{
			try {
				int r = 0;
				int.TryParse (GetConfig (key), out r);
				return r;
			} catch {
				return 0;
			}
		}

		void innerSetConfig (string key, string value)
		{
			try {
				if (value == null) {
					//delete
					if (Application.Current.Properties.ContainsKey (key))
						Application.Current.Properties.Remove (key);
				} else {
					Application.Current.Properties [key] = value;
//					var show_value = key == settings.PASSWORD ? "***" : value;
//					Debug.WriteLine ($"SetConfig {key}={show_value}");
				}
				Application.Current.SavePropertiesAsync ();
			} catch (System.NotSupportedException) {
				Console.WriteLine ("innerSetConfig NotSupportedException {0}", key);
			} catch (Exception ex) {
				Insights.Report (ex, key, value);
			}
		}

		public void SetConfig (string key, string value)
		{
			innerSetConfig (key, value);
		}

		public void SetConfig (string key, int value)
		{
			SetConfig (key, value.ToString ());
		}

		public void SetConfig (string key, Double value)
		{
			SetConfig (key, Convert.ToString (value));
		}

		public void SetConfig (string key, Int64 value)
		{
			SetConfig (key, Convert.ToString (value));
		}

		public void SetConfig (string key, DateTime value)
		{
			SetConfig (key, value.ToUniversalTime ().ToString ("s"));
		}

		public void SetConfig (string key, bool value)
		{
			SetConfig (key, value.ToString ());
		}

		#endregion

		public Persist ()
		{
			Console.WriteLine ("Persist()");
			ServerResponseToBeProcessed = null;
			Votes = new List<Vote> ();
			Places = new List<Place> ();
			Friends = new Dictionary<string, Friend> ();
			InvitationsIn = new List<Invite> ();
			InvitationsOut = new List<Invite> ();
			Acceptances = new List<Invite> ();
			InviteNames = new Dictionary<string, string> ();
			SearchHistory = new PersistantQueueWithPosition (3, "Search-History");
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
			CommentCache = new Dictionary<long, List<VoteComment>> ();
		}
	}
}

