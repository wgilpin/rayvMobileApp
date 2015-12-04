using System;
using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin.Forms.Maps;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RayvMobileApp
{
	using PlacesList = List<Place>;

	class ButtonCell:ViewCell
	{
		public ButtonCell (string text, EventHandler onClick) : base ()
		{
			View = new RayvButton (text){ OnClick = onClick };
		}
	}

	public class PlacesTableView: TableView
	{
		const int PAGE_SIZE = 20;
		int PlacesShown = 0;
		TableSection OurSection;
		TableSection StrangerSection;
		PlacesList MainList;
		PlacesList StrangerList;
		ButtonCell MoreBtn;
		ButtonCell StrangerBtn;
		TextCell NoPlacesMessage;

		public EventHandler OnPlaceTapped;
		public EventHandler<EventArgsMessage> OnErrorMessage;

		public Position? SearchCentre;
		public string ShowFriend = "";

		public bool IsShowingDistance = true;
		public bool IsShowingVotes = true;

		public string SummaryText { 
			get { return OurSection.Title; }
			set { OurSection.Title = value; }
		}

		public void DoItemTapped (Object sender, EventArgs e)
		{
			OnPlaceTapped?.Invoke (sender, e);
		}


		public void DoShowStrangers (Object sender = null, EventArgs e = null)
		{
			// server side search at thje chosen location (or here if there is no search position selected)
			Double lat = SearchCentre == null ? Persist.Instance.GpsPosition.Latitude : ((Position)SearchCentre).Latitude;
			Double lng = SearchCentre == null ? Persist.Instance.GpsPosition.Longitude : ((Position)SearchCentre).Longitude;
			var parms = new Dictionary<string, string> () {
				{ "lat", lat.ToString () },
				{ "lng", lng.ToString () }
			};
			var result = Persist.Instance.GetWebConnection ().get ("/api/items/all", parms).Content;
			if (string.IsNullOrEmpty (result)) {
				// nothing found on the server
				Console.WriteLine ("DoShowStrangers None Found");
				OnErrorMessage?.Invoke (this, new EventArgsMessage ("No more places found"));
				Root.Remove (StrangerSection);
			} else {
				//ShowStrangerBtn.IsVisible = false;
				JObject obj = JObject.Parse (result);
				string placesStr = obj ["points"].ToString ();
				List<Place> place_list = JsonConvert.DeserializeObject<List<Place>> (placesStr);
				List<Place> strangerPlaces = new List<Place> ();
				foreach (Place p in place_list) {
					if (Persist.Instance.GetPlace (p.key) != null) {
						// we already had it
						continue;
					}
					p.CalculateDistanceFromPlace (SearchCentre);
					strangerPlaces.Add (p);
				}
				if (strangerPlaces.Count == 0) {
					Console.WriteLine ("DoShowStrangers NONE FOUND");
					OnErrorMessage?.Invoke (this, new EventArgsMessage ("No more places found"));
				} else {
					//SecondListHeaderLbl.Text = "Other nearby places";
					strangerPlaces.Sort ();
					StrangerSection.Remove (StrangerBtn);
					for (int i = 0; (i < PAGE_SIZE) && (i < strangerPlaces.Count); i++) {
						Place p = strangerPlaces [i];
						StrangerSection.Add (new PlaceCell (p, IsShowingDistance, IsShowingVotes, DoItemTapped));
					}

				}
			}
		}

		public void DoShowMore (Object sender = null, EventArgs e = null)
		{
			int startPlacesShown = PlacesShown + 1;
			OurSection.Remove (MoreBtn);
			for (int i = startPlacesShown; (i < startPlacesShown + PAGE_SIZE) && (i < MainList.Count); i++) {
				Place p = MainList [i];
				OurSection.Add (new PlaceCell (p, IsShowingDistance, IsShowingVotes, DoItemTapped));
				PlacesShown = i;
			}
			if (PlacesShown < MainList.Count - 1) {
				OurSection.Add (MoreBtn);
			}
		}

		public void SetMainList (PlacesList list)
		{
			MainList = list;
			PlacesShown = 0;
			OurSection.Remove (NoPlacesMessage);
			DoShowMore ();
			if (list.Count < 10) {
				Root.Remove (StrangerSection);
				Root.Add (StrangerSection);
			}
			if (list.Count == 0)
				OurSection.Add (NoPlacesMessage);
		}

		public PlacesTableView (bool showVotes = true, bool showDistance = true)
		{
			IsShowingVotes = showVotes;
			IsShowingDistance = showDistance;
			MoreBtn = new ButtonCell ("More...", DoShowMore);
			StrangerBtn = new ButtonCell ("Show Strangers' Places", DoShowStrangers);
			Intent = TableIntent.Data;
			OurSection = new TableSection ("All Places");
//			{
//				HorizontalOptions = LayoutOptions.FillAndExpand, 
//				BackgroundColor = settings.BaseColor, 
//				TextColor = Color.White, 
//				FontAttributes = FontAttributes.Bold,
//				HorizontalTextAlignment = TextAlignment.Center
//			};
			StrangerSection = new TableSection ("Strangers' Places");
			StrangerSection.Add (StrangerBtn);
			Root = new TableRoot {
				OurSection,
			};
			HasUnevenRows = true;
			RowHeight = showVotes ? 100 : 90;
			NoPlacesMessage = new TextCell{ Text = "No Places Found", TextColor = settings.ColorDarkGray };
		}
	}
}

