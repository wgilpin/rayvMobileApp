using System;
using Xamarin.Forms.Maps;

namespace RayvMobileApp
{
	public struct FilterParameters
	{
		public string Text;
		public string Who;
		public string Cuisine;
		public VoteFilterKind Kind;
		public MealKind MealKind;
		public PlaceStyle Style;
		public Position? Centre;
	}
}

