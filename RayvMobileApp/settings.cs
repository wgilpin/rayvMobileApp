using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class settings
	{
		public const string FILTER_WHEN = "FILTER_WHEN";
		public const string FILTER_STYLE = "FILTER_STYLE";
		public const string FILTER_KIND = "FILTER_KIND";
		public const string FILTER_WHAT = "CHOICE_BY_VOTE_KIND";
		public const string FILTER_WHO = "CHOICE_BY_WHO";
		public const string FILTER_WHO_LIST = "CHOICE_BY_WHO_LIST";
		public const string FILTER_CUISINE = "FILTER_CUISINE";
		public const string FILTER_WHERE_NAME = "FILTER_WHERE_NAME";
		public const string FILTER_WHERE_LAT = "FILTER_WHERE_LAT";
		public const string FILTER_WHERE_LNG = "FILTER_WHERE_LNG";
		public const string PLACE_TYPES = "food|restaurant|bar|cafe|meal_delivery|meal_takeaway";
		public const string SECRET_KEY = "=r-$b*8hglm+858&9t043hlm6-&6-3d3vfc4((7yd0dbrakhvi";
		public const string GOOGLE_API_KEY = "AIzaSyDiTThta8R7EFuFo8cGfPHxIGYoFkc77Bw";
		public const bool OFFLINE_UPDATE_ENABLED = false;
		public const Double GEO_FILTER_BOX_SIZE_DEG = 0.017;
		public const bool USE_XAMARIN_MAPS = true;
		// = 1mile
		//Config
		public const  string USERNAME = "username";
		public const string PASSWORD = "pwd";
		public const  string DB_VERSION = "db_version";
		public const  string LAST_LAT = "LastLat";
		public const  string LAST_LNG = "LastLng";
		public const  string SKIP_INTRO = "SkipIntro";
		public const  string SERVER = "server";
		public const  string DEFAULT_SERVER = "rayv-app.appspot.com/";
		public const  string LAST_SYNC = "last_sync";
		public const  string MY_ID = "myId";
		// 30 mins timeout
		public static  TimeSpan LIST_PAGE_TIMEOUT = new TimeSpan (0, 30, 0);
		public const int MAX_SERVER_TRIES = 1;
		public static Color BaseColor = Color.FromHex ("4A90E2");
		//		public static Color ColorLight = Color.FromHex ("718CBE");
		//		public static Color ColorDark = Color.FromHex ("6883B5");
		//		public static Color ColorVeryDark = Color.FromHex ("4863B5");
		public static Color ColorLightGray = Color.FromHex ("EEE");
		public static Color ColorMidGray = Color.FromHex ("AAA");
		public static Color ColorDarkGray = Color.FromHex ("444");
		public static Color ColorOffWhite = Color.FromHex ("E7E7E7");


		public static string DevicifyFilename (string filename)
		{
			switch (Device.OS) {
				case  TargetPlatform.Android:
					var temp = filename.Replace (" ", "_").Replace ("-", "_").Replace ("@", "_");
					if (temp [0] <= '9' && temp [0] >= '0')
						return 'i' + temp;
					return temp;
				default:
					return filename;
			}
		}
	}
}

