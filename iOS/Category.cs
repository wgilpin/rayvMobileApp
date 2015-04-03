using System;
using SQLite;

namespace RayvMobileApp.iOS
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

