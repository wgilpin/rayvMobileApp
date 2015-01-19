using System;
using Xamarin.Forms;

namespace RayvMobileApp.iOS
{

	public class PlacesListView : ListView
	{
		private const int IMAGE_SIZE = 63;

		public PlacesListView () : base ()
		{
			Console.WriteLine ("PlacesListView()");
			RowHeight = 65;

			ItemTemplate = new DataTemplate (() => {
				// Create views with bindings for displaying each property.
				Label nameLabel = new Label ();
				nameLabel.LineBreakMode = LineBreakMode.TailTruncation;
				nameLabel.SetBinding (Label.TextProperty, "place_name");

				Label catLabel = new Label ();
				catLabel.SetBinding (Label.TextProperty, "category");
				catLabel.Font = Font.SystemFontOfSize (NamedSize.Small);
				catLabel.TextColor = Color.FromHex ("FF6A00");

				Label distLabel = new Label ();
				distLabel.Font = Font.SystemFontOfSize (NamedSize.Small);
				distLabel.SetBinding (Label.TextProperty, "distance");

				Label DEBUGLbl = new Label ();
				DEBUGLbl.SetBinding (Label.TextProperty, "thumbnail");

				Image webImage = new Image { 
					Aspect = Aspect.AspectFill,
					WidthRequest = IMAGE_SIZE, 
					HeightRequest = IMAGE_SIZE
				};
//				Binding x = new Binding ("img");
				webImage.SetBinding (Image.SourceProperty, "thumb_url");


				// Return an assembled ViewCell.
				return new ViewCell {
					View = new StackLayout {
						Padding = new Thickness (1, 1),
						Spacing = 1,
						Orientation = StackOrientation.Horizontal,
						Children = {
							webImage,
							new StackLayout {
								Spacing = 0,
								Orientation = StackOrientation.Vertical,
								Children = {
									nameLabel,
									catLabel,
									distLabel,
								}
							}
						}

					}
				};
			});
			VerticalOptions = LayoutOptions.FillAndExpand;
		}
	}
}

