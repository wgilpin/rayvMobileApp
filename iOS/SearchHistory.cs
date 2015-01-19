using System;
using SQLite;

namespace RayvMobileApp.iOS
{
	public class SearchHistory
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }

		public string PlaceName;

		public SearchHistory (string item)
		{
			PlaceName = item;
		}

		public SearchHistory ()
		{
		}
	}
}

