using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{
	class DetailLabel : Label
	{
		public DetailLabel (string text) : base ()
		{
			VerticalOptions = LayoutOptions.CenterAndExpand;
			HorizontalOptions = LayoutOptions.CenterAndExpand;
			Text = text;
		}
	}

	public class DetailPage : ContentPage
	{
		Place DisplayPlace;
		Label place_name;
		Image img;
		Label category;
		ButtonWide voteLike;
		ButtonWide voteDislike;
		Button voteWishlist;
		ButtonWide callBtn;
		ButtonWide webBtn;
		Label distance;
		Label address;
		Label descr;

		void LoadPage (string key)
		{
			foreach (Place p in Persist.Instance.Places) {
				if (p.key == key) {
					DisplayPlace = p;
					break;
				}
			}
			if (DisplayPlace == null) {
				Console.WriteLine ("LoadPage FAILED");
				return;
			}
			Console.WriteLine ("DetailsPage.LoadPage: dist is {0}", DisplayPlace.distance);

			place_name.Text = DisplayPlace.place_name;
			address.Text = DisplayPlace.address;
			if (DisplayPlace.img.Length > 0) {
				img.Source = ImageSource.FromUri (new Uri (DisplayPlace.img));
			}
			category.Text = DisplayPlace.category;
			descr.Text = DisplayPlace.descr;
			distance.Text = DisplayPlace.distance;
			webBtn.Text = DisplayPlace.website;
			callBtn.Text = DisplayPlace.telephone;
			voteLike.TextColor = Color.Black;
			voteDislike.TextColor = Color.Black;
			voteWishlist.TextColor = Color.Black;
			voteLike.BackgroundColor = Color.FromHex ("#444111111");
			voteDislike.BackgroundColor = Color.FromHex ("#444111111");
			voteWishlist.BackgroundColor = Color.FromHex ("#444111111");
			switch (DisplayPlace.vote) {
			case "-1":
				voteDislike.BackgroundColor = Color.Olive;
				voteDislike.TextColor = Color.White;
				break;
			case "1":
				voteLike.BackgroundColor = Color.Olive;
				voteLike.TextColor = Color.White;
				break;
			default:
				voteWishlist.BackgroundColor = Color.Olive;
				voteWishlist.TextColor = Color.White;
				break;
			}
		}

		public DetailPage (Place place)
		{
			DisplayPlace = place;
			this.Appearing += (object sender, EventArgs e) => {
				Console.WriteLine ("DetailPage: Pre-Appearing distance is {0}", DisplayPlace.distance);
				LoadPage (DisplayPlace.key);
				Console.WriteLine ("DetailPage: Post-Appearing distance is {0}", DisplayPlace.distance);
			};
			var absoluteLayout = new AbsoluteLayout ();

			place_name = new LabelWide ();
			img = new Image ();
			try {
				img.HorizontalOptions = LayoutOptions.CenterAndExpand;
				img.VerticalOptions = LayoutOptions.Start;
				img.Aspect = Aspect.AspectFill;
			} catch {
				img.Source = null;
			}
			category = new LabelWide {
				TextColor = Color.Red,
			};
			address = new LabelWide ();
			descr = new LabelWide ();
			distance = new LabelWide ();
			webBtn = new ButtonWide ();
			callBtn = new ButtonWide ();
			voteLike = new ButtonWide {
				Text = "Like",
			};
			voteDislike = new ButtonWide {
				Text = "Dislike",
			};
			voteWishlist = new ButtonWide {
				Text = "Wish",
			};
			Grid voteGrid = new Grid {
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				}
			};

			voteGrid.Children.Add (voteLike, 0, 0);
			voteGrid.Children.Add (voteWishlist, 1, 0);
			voteGrid.Children.Add (voteDislike, 2, 0);

			LoadPage (DisplayPlace.key);

			var stackLayout = new StackLayout {
				Children = {
					place_name,
					category,
					distance,
					address,
					descr,
					voteGrid,
					webBtn,
					callBtn,
				}
			};
			absoluteLayout.Children.Add (img, new Rectangle (0, 0, 1, 0.4), AbsoluteLayoutFlags.All);
			absoluteLayout.Children.Add (
				stackLayout, 
				new Rectangle (0, 0.4, 1, 1), 
				AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.WidthProportional);
			this.Content = new ScrollView {
				Content = absoluteLayout,
			};
			ToolbarItems.Add (new ToolbarItem {
				Name = "Edit",
				Icon = "187-pencil@2x.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command (() => Navigation.PushAsync (new EditPage (DisplayPlace)))
			});
		}
	}
}
