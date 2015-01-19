using System;
using SQLite;

namespace RayvMobileApp.iOS
{
	public class Configuration
	{
		[PrimaryKey, Unique]
		public string Key { get; set; }

		public string Value { get; set; }

		public Configuration ()
		{
			Key = "";
			Value = "";
		}

		public Configuration (string key, string value)
		{
			Key = key;
			Value = value;
		}

		
	}
}

