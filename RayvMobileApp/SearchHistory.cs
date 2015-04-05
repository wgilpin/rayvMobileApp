using System;
using SQLite;

namespace RayvMobileApp
{
	public class SearchHistory
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }

		public string PlaceName { get; set; }

		public SearchHistory (string item)
		{
			PlaceName = item;
		}

		public SearchHistory ()
		{
		}
	}
}

