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

		public string GetIconName ()
		{
			String imageUrl = "";
			if (vote == 1)
				imageUrl = "heart-lg.png";
			if (vote == -1)
				imageUrl = "no-entry-lg.png";
			if (untried)
				imageUrl = "star-lg.png";
			return imageUrl;
		}
	}
}

