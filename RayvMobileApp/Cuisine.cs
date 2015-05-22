using System;
using SQLite;

namespace RayvMobileApp
{
	public class Cuisine
	{
		[PrimaryKey, Unique]
		public string Title { get; set; }

		public Cuisine ()
		{
		}

		public override string ToString ()
		{
			return Title;
		}
	}
}

