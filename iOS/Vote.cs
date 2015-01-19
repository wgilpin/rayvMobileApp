using System;
using SQLite;

namespace RayvMobileApp.iOS
{
	public class Vote
	{
		[PrimaryKey]
		public string key { get; set; }

		public string voter { get; set; }

		public string place_name { get; set; }

		public int vote { get; set; }

		public bool untried { get; set; }

		public string comment { get; set; }
	}
}

