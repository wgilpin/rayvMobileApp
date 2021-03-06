﻿using System;

using Xamarin.Forms;

namespace RayvMobileApp
{
	public class DoubleImageButton : ContentView
	{
		bool _isEnabledRight;

		public string LeftText { 
			get { return LeftBtn.Text; }
			set { LeftBtn.Text = value; }
		}

		public ImageSource LeftSource { 
			get { return LeftBtn.ImageSource; }
			set { LeftBtn.ImageSource = value; }
		}

		public string RightText { 
			get { return RightBtn.Text; }
			set { RightBtn.Text = value; }
		}

		public ImageSource RightSource { 
			get { return RightBtn.ImageSource; }
			set { RightBtn.ImageSource = value; }
		}

		public EventHandler LeftClick {
			set { LeftBtn.OnClick = value; }
		}

		public EventHandler RightClick {
			set { RightBtn.OnClick = value; }
		}

		public bool IsEnabledRight {
			set {
				_isEnabledRight = !_isEnabledRight;
				RightBtn.IsEnabled = _isEnabledRight;
				RightBtn.BackgroundColor = Color.FromRgba (255, 255, 255, 0);
				RightSource = "";
			}
		}

		ButtonWithImage LeftBtn;
		ButtonWithImage RightBtn;

		public DoubleImageButton ()
		{
			LeftBtn = new ButtonWithImage {
				Padding = 10,
				FontSize = settings.FontSizeButtonLarge,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};
			RightBtn = new ButtonWithImage {
				Padding = 10,
				FontSize = settings.FontSizeButtonLarge,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};
			Content = new StackLayout {
				Padding = 1,
				Children = { LeftBtn, RightBtn },
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Orientation = StackOrientation.Horizontal,
			};
		}
	}
}


