using System;
using System.Collections;
using Xamarin.Forms.Maps;

namespace RayvMobileApp
{
	public class PersistantQueue
	{
		private int _size;
		private string _kind;

		// usage: new PersistantQueue (nSize, "Name Identfying this queue")
		public PersistantQueue (int size, string queueName)
		{
			_size = size;
			_kind = queueName;
		}

		public int Length {
			get {
				int len = 0;
				for (int idx = _size - 1; idx >= 0; idx--) {
					if (GetItem (idx).Length > 0) {
						len = idx + 1;
						break;
					}
				}
				return len;
			}
		}

		public void Add (string item, bool unique = false)
		{
			// ripple
			Console.WriteLine ("Queue Add: {0}", item);
			for (int idx = Length; idx > 0; idx--) {
				// thingy1 is set to val(thingy0)
				string item_i = GetItem (idx - 1);
				if (unique) {
					if (item_i == item) {
						return;
					}
				}
				if (unique) {
					if (GetItem (0) == item)
						return;
				}
				Console.WriteLine ("Moving {0} at {1} to {2}", item_i, idx - 1, idx);
				Persist.Instance.SetConfig (
					String.Format ("{0}{1}", _kind, idx), 
					item_i
				);
			}
			//now 0
			Persist.Instance.SetConfig (
				String.Format ("{0}0", _kind), 
				item);
		}

		public string GetItem (int n)
		{
			try {
				string val = Persist.Instance.GetConfig (String.Format ("{0}{1}", _kind, n));
				return val;
			} catch {
				return "";
			}
		}

		public bool Contains (string item)
		{
			for (int idx = 0; idx <= Length; idx++) {
				// thingy1 is set to val(thingy0)
				string item_i = GetItem (idx - 1);
				if (item_i == item) {
					return true;
				}
			}
			return false;
		}
	}

	public class PersistantQueueWithPosition
	{
		PersistantQueue _namesQ;
		PersistantQueue _latQ;
		PersistantQueue _lngQ;
		string _namesTag = "Name ";
		string _latTag = "Lat ";
		string _lngTag = "Lng ";

		//		private int _size;

		// usage: new PersistantQueue (nSize, "Name Identfying this queue")
		public PersistantQueueWithPosition (int size, string queueName)
		{
//			_size = size;
			_namesQ = new PersistantQueue (size,$"{queueName}.{_namesTag}");
			_latQ = new PersistantQueue (size,$"{queueName}.{_latTag}");
			_lngQ = new PersistantQueue (size,$"{queueName}.{_lngTag}");
		}

		public bool Contains (string name)
		{
			return _namesQ.Contains (name);
		}

		public int Count ()
		{
			return _namesQ.Length;
		}

		public GeoLocation GetItem (int idx)
		{
			try {
				var geo = new GeoLocation ();
				geo.Name = _namesQ.GetItem (idx);
				geo.Lat = Convert.ToDouble (_latQ.GetItem (idx));
				geo.Lng = Convert.ToDouble (_lngQ.GetItem (idx));
				return geo;
			} catch (Exception) {
				return null;
			}
		}

		public void Add (GeoLocation posn)
		{
			if (_namesQ.Contains (posn.Name))
				return;
			_namesQ.Add (posn.Name);
			_latQ.Add (posn.Lat.ToString ());
			_lngQ.Add (posn.Lng.ToString ());
		}

	}
}
