using System;
using SQLite;

namespace RayvMobileApp.iOS
{
	public class Friend
	{
		[PrimaryKey]
		public string id { get; set; }

		[MaxLength (50)]
		public string name { get; set; }
	}
}

