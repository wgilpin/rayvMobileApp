using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class settings
	{
		
		public static string[] TesterWhitelist = { "Will", "pegah", "georgia" };
		// milliseconds
		public const int WEB_TIMEOUT = 10000;
		public const int WEB_TIMEOUT_TEST = 60000;
		public const bool USE_OAUTH = false;
		public const string FACEBOOK_EMAIL = "FB_EMAIL";
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
		public const string FILTER_MIN_VOTE = "FILTER_MIN_VOTE";
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
		public const  string NOTIFICATIONS_TOKEN = "Notifications token";
		public const  string SERVER_DEFAULT = "rayv-app.appspot.com/";
		public const  string SERVER_LOCAL = "http://localhost:8080/";
		public const  string LAST_SYNC = "last_sync";
		public const  string LAST_OPENED = "last_opened";
		public const  string MY_ID = "myId";
		public const string DEFAULT_PLACE_IMAGE_LG = "Logo_78.png";
		// 30 mins timeout
		public static  TimeSpan LIST_PAGE_TIMEOUT = new TimeSpan (0, 30, 0);
		public const int MAX_SERVER_TRIES = 1;
		public const int MAX_INITIAL_LIST_LENGTH = 20;
		public const Double MAX_LIST_DISTANCE = 5.0;
		public const int MIN_PWD_LENGTH = 7;
		public const int NEWS_PAGE_TIMESPAN_DAYS = 14;
		public static string[] IgnoreWords = { "the", "and", "of", "in" };
		public static Char[] IgnoreChars = { '\'', ',', '.', ':' };
		public const int CurrentDbVersion = 10;


		public static Color BaseColor = Color.FromHex ("0F9D58");
		public static Color BaseDarkColor = ColorUtil.Darker (Color.FromHex ("0F9D58"));
		//		public static Color ColorLight = Color.FromHex ("718CBE");
		//		public static Color ColorDark = Color.FromHex ("6883B5");
		//		public static Color ColorVeryDark = Color.FromHex ("4863B5");
		public static Color ColorLightGray = Color.FromHex ("EEE");
		public static Color ColorDarkGray = Color.FromHex ("444");
		public static Color ColorMidGray = Color.FromHex ("AAA");
		public static Color ColorOffWhite = Color.FromHex ("E7E7E7");
		public static Color BaseTextColor = ColorLightGray;
		public static Color InvertTextColor = ColorDarkGray;
		public const  string PROFILE_SCREENNAME = "Pr_screenname";
		public const  string PROFILE_EMAIL = "Pr_email";
		public const  string PROFILE_GENDER = "Pr_gender";


		public static string DevicifyFilename (string filename)
		{
			// this is because iOS resource naming is relaxed, android requires a valid Java identifier, but a lot of 
			//   my images were already named for ios
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

		// font sizes - Large font too big on android

		public static double FontSizeLabelMedium {
			get {
				return Device.OnPlatform (
					Device.GetNamedSize (NamedSize.Medium, typeof(Label)),
					Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				);
			}
		}

		public static double FontSizeLabelSmall {
			get {
				return Device.OnPlatform (
					Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					Device.GetNamedSize (NamedSize.Small, typeof(Label))
				);
			}
		}

		public static double FontSizeLabelMicro {
			get {
				return Device.OnPlatform (
					Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
					Device.GetNamedSize (NamedSize.Micro, typeof(Label))
				);
			}
		}

		public static double FontSizeLabelLarge {
			get {
				return Device.OnPlatform (
					Device.GetNamedSize (NamedSize.Large, typeof(Label)),
					Device.GetNamedSize (NamedSize.Medium, typeof(Label)),
					Device.GetNamedSize (NamedSize.Large, typeof(Label))
				);
			}
		}

		public static double FontSizeButtonMedium {
			get {
				return Device.OnPlatform (
					Device.GetNamedSize (NamedSize.Medium, typeof(Button)),
					Device.GetNamedSize (NamedSize.Small, typeof(Button)),
					Device.GetNamedSize (NamedSize.Medium, typeof(Button))
				);
			}
		}

		public static double FontSizeButtonLarge {
			get {
				return Device.OnPlatform (
					Device.GetNamedSize (NamedSize.Large, typeof(Button)),
					Device.GetNamedSize (NamedSize.Medium, typeof(Button)),
					Device.GetNamedSize (NamedSize.Large, typeof(Button))
				);
			}
		}
	}
}

