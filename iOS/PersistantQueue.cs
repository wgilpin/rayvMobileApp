using System;
using System.Collections;

namespace RayvMobileApp.iOS
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
				for (int idx = 0; idx < _size; idx++) {
					if (GetItem (idx) == "")
						return idx;
				}
				return 0;
			}
		}

		public void Add (string item)
		{
			// ripple
			Console.WriteLine ("length: {0}", Length);
			for (int idx = Length; idx > 0; idx--) {
				// thingy1 is set to val(thingy0)
				Console.WriteLine ("Moving {0} at {1} to {2}", GetItem (idx - 1), idx - 1, idx);
				Persist.Instance.SetConfig (
					String.Format ("{0}{1}", _kind, idx), 
					GetItem (idx - 1)
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
	}
}

