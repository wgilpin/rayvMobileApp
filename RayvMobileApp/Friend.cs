using System;
using SQLite;
using Xamarin.Forms;
using System.Text;

namespace RayvMobileApp
{
	public class Friend
	{
		[PrimaryKey]
		public string Key { get; set; }

		[MaxLength (50)]
		public string Name { get; set; }



		public Friend ()
		{
		}

		public Friend (String name, String key) : this ()
		{
			Key = key;
			Name = name;
		}
	}
}

