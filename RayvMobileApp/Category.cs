using System;
using SQLite;

namespace RayvMobileApp
{
	public class Category
	{
		[PrimaryKey, Unique]
		public string Title { get; set; }

		public Category ()
		{
		}
	}
}

