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

		[Ignore]
		public bool InFilter {
			get;
			set;
		}

		[Ignore]
		public string InFilterImage {
			get {
				const string checkedImg = "checkbox_checked.png";
				const string unCheckedImg = "checkbox_unchecked.png";
				return InFilter ? checkedImg : unCheckedImg;
			}
		}


		public Friend ()
		{
		}

		public Friend (String name, String key) : this ()
		{
			Key = key;
			Name = name;
			InFilter = true;
		}
	}
}

