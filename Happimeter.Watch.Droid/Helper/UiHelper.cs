using System;
using Android.Widget;
namespace Happimeter.Watch.Droid.Helper
{
	public class UiHelper
	{
		public UiHelper()
		{
		}

		public static void SetMoodImage(ImageView view, int pleasance, int activation)
		{
			if (activation == 0 && pleasance == 0)
			{
				view.SetImageResource(Resource.Drawable.a_0_h_0);
			}
			else if (activation == 1 && pleasance == 0)
			{
				view.SetImageResource(Resource.Drawable.a_1_h_0);
			}
			else if (activation == 2 && pleasance == 0)
			{
				view.SetImageResource(Resource.Drawable.a_2_h_0);
			}
			else if (activation == 0 && pleasance == 1)
			{
				view.SetImageResource(Resource.Drawable.a_0_h_1);
			}
			else if (activation == 1 && pleasance == 1)
			{
				view.SetImageResource(Resource.Drawable.a_1_h_1);
			}
			else if (activation == 2 && pleasance == 1)
			{
				view.SetImageResource(Resource.Drawable.a_2_h_1);
			}
			else if (activation == 0 && pleasance == 2)
			{
				view.SetImageResource(Resource.Drawable.a_0_h_2);
			}
			else if (activation == 1 && pleasance == 2)
			{
				view.SetImageResource(Resource.Drawable.a_1_h_2);
			}
			else if (activation == 2 && pleasance == 2)
			{
				view.SetImageResource(Resource.Drawable.a_2_h_2);
			}
		}

	}
}
