﻿using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using RestSharp;
using System.Linq;
using System.Diagnostics;
using Xamarin;
using Xamarin.Forms.Maps;

namespace RayvMobileApp
{

	public class MapListPage : ContentPage
	{
		#region Fields

		static PlacesListView listView;

		ActivityIndicator Spinner;

		Label NothingFound;

		public static List<Place> ItemsSource {
			set {
				lock (listView) {
					var nearby = value.Where (p => p.distance_for_search < settings.MAX_LIST_DISTANCE).ToList ();
					listView.SetMainItemSource (nearby);
				}
			}
		}

		#endregion

		#region Constructors


		public MapListPage ()
		{
			Analytics.TrackPage ("MapListPage");
			Console.WriteLine ("ListView()");
			this.Title = "Map Places";
			this.Icon = settings.DevicifyFilename ("bars-black.png");

			listView = new PlacesListView {
				//ItemsSource = Persist.Instance.Places,
			};
			listView.OnItemTapped = DoSelectListItem;
			StackLayout tools = new BottomToolbar (this, "list");
			NothingFound = new LabelWide ("Nothing Found") {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
			Grid grid = new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (5, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Auto) }
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};
			Spinner = new ActivityIndicator ();
			StackLayout inner = new StackLayout {
				Children = {
					Spinner,
					listView,
					NothingFound,
				}
			};
			Spinner = new ActivityIndicator {
				IsRunning = true,
				Color = Color.Red,
			};
			grid.Children.Add (inner, 0, 0);
			grid.Children.Add (tools, 0, 1);
			this.Content = grid;

			SetList (Persist.Instance.Places);
			Console.WriteLine ("ListPage.FilterList Constructor set posn to {0},{1}", Persist.Instance.DisplayPosition.Latitude, Persist.Instance.DisplayPosition.Longitude);
		}




		/**
		 * Constructor when a cuisine is supplied
		 */
		public MapListPage (List<Place> placeList) : this ()
		{
			
		}


		#endregion

		#region Events

		void DoSelectListItem (object sender, ItemTappedEventArgs e)
		{
			Debug.WriteLine ("Listpage.ItemTapped: Push DetailPage");
			this.Navigation.PushAsync (new DetailPage (e.Item as Place));
		}

		#endregion

		#region Methods

		public void SetList (List<Place> list)
		{
			lock (listView) {
				try {
					Console.WriteLine ("SetList {0}", list.Count);
					if (list.Count == 0) {
						listView.IsVisible = false;
						NothingFound.IsVisible = true;
						return;
					}
					NothingFound.WidthRequest = this.Width;
					NothingFound.IsVisible = false;
					listView.IsVisible = true;
					ItemsSource = null;
					list.Sort ();
					Console.WriteLine ("SetList SORT");
					ItemsSource = list;
					Spinner.IsRunning = false;

				} catch (Exception ex) {
					Insights.Report (ex);
					restConnection.LogErrorToServer ("ListPage.SetList Exception {0}", ex);
				}
			}
		}

		#endregion

	}

}


