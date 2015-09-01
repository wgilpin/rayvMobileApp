using System;
using Xamarin.Forms;

namespace RayvMobileApp
{
	public class StarEditorEventArgs : EventArgs
	{
		public int Vote;
		public bool Untried;

		public StarEditorEventArgs (int vote, Boolean untried)
		{
			Vote = vote;
			Untried = untried ? true : false;
		}
	}



	public class StarEditor: Grid
	{
		static int UNTRIED_IMG_IDX = 0;
		int _height = 30;
		int _vote = 0;
		bool _untried = false;
		bool _showUntried;

		TapGestureRecognizer tapped;

		Image[] _stars;

		public int  Height {
			get{ return _height; }
			set {
				RowDefinitions [0].Height = value;
				foreach (var cd in ColumnDefinitions) {
					cd.Width = value;
				}
				_height = value;
			}
		}

		public static readonly BindableProperty VoteProperty = BindableProperty.Create<StarEditor, int> (
			                                                       p => p.Vote, 
			                                                       defaultValue: 0,
			                                                       defaultBindingMode: BindingMode.TwoWay,
			                                                       validateValue: (bindable, value) => {
				return 0 <= value && value <= 5;
			},
			                                                       propertyChanged: (bindable, oldValue, newValue) => {
				var thisView = (StarEditor)bindable;
				thisView.SetVote (newValue);
			});

		public static readonly BindableProperty UntriedProperty = BindableProperty.Create<StarEditor, bool> (
			                                                          p => p.Untried, 
			                                                          defaultValue: false,
			                                                          defaultBindingMode: BindingMode.TwoWay,
			                                                          validateValue: (bindable, value) => {
				return true;
			},
			                                                          propertyChanged: (bindable, oldValue, newValue) => {
				var thisView = (StarEditor)bindable;
				thisView.SetUntried (newValue);
			});

		public int Vote {
			get { return _vote; }
			set { SetVote (value); }
		}

		public bool Untried {
			get { return _untried; }
			set { SetUntried (value); }
		}

		public bool ReadOnly { get; set; }

		public EventHandler<StarEditorEventArgs> Changed;

		void DoStarTapped (Object sender, EventArgs e)
		{
			if (ReadOnly)
				return;
			Console.WriteLine ("Star tapped");
			_vote = Convert.ToInt32 ((sender as Image).StyleId);
			_untried = _vote == 0;
			Console.WriteLine ($"Star {_vote} tapped");
			if (_untried)
				SetUntried (_untried);
			else
				SetVote (_vote);
			Changed?.Invoke (sender, new StarEditorEventArgs (_vote, _untried));
		}

		void SetVote (int vote)
		{
			_vote = vote;
			for (int i = 1; i < 6; i++) {
				// if vote is 0 
				if (i <= vote && vote > 0) {
					_stars [i].Source = "star-selected.png";
				} else {
					_stars [i].Source = "star-empty.png";
				}
			}	
			if (vote > 0 && _showUntried)
				_stars [UNTRIED_IMG_IDX].Source = "wish_grey.png";
		}

		void SetUntried (bool untried)
		{
			_untried = untried;
			_stars [UNTRIED_IMG_IDX].Source = _untried ? "wish_blue.png" : "wish_grey.png";
			_vote = 0;
			SetVote (0);	
		}

		void AddImage (string imageSource, int columnIdx)
		{
			ColumnDefinitions.Add (new ColumnDefinition {
				Width = new GridLength (1, GridUnitType.Auto)
			});
			Image _star = new Image {
				Source = imageSource,
				WidthRequest = _height,
				Aspect = Aspect.AspectFit
			};
			_star.StyleId = (columnIdx).ToString ();
			_star.GestureRecognizers.Add (tapped);
			_stars [columnIdx] = _star;
			Children.Add (_star, columnIdx, 0);
		}

		public StarEditor (bool showUntried) : base ()
		{
			ReadOnly = false;
			_showUntried = showUntried;
			Padding = 2;
			_stars = new Image[6];
			RowDefinitions.Add (new RowDefinition {
				Height = new GridLength (_height)
			});
			tapped = new TapGestureRecognizer ();
			tapped.Tapped += DoStarTapped;
			AddImage (showUntried ? "wish_grey.png" : "", 0);
			//five star columns
			for (int i = 1; i < 6; i++) {
				AddImage ("star-empty.png", i);
			}

		}
	}
}

