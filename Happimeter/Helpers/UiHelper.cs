using System;
namespace Happimeter.Helpers
{
    public static class UiHelper
    {

        public static string GetImagePathForMood(double? activation, double? pleasance)
        {
            if (activation == null || pleasance == null)
            {
                return "QuestionMark.png";
            }
            //pleasance = 0
            if (activation < 1 && pleasance < 1)
            {
                return "TransparentMood_9.png";
            }
            if (activation < 2 && pleasance < 1)
            {
                return "TransparentMood_6.png";
            }
            if (activation <= 3 && pleasance < 1)
            {
                return "TransparentMood_3.png";
            }
            //pleasance = 1
            if (activation < 1 && pleasance < 2)
            {
                return "TransparentMood_8.png";
            }
            if (activation < 2 && pleasance < 2)
            {
                return "TransparentMood_5.png";
            }
            if (activation <= 3 && pleasance < 2)
            {
                return "TransparentMood_2.png";
            }
            //pleasance = 2
            if (activation < 1 && pleasance <= 3)
            {
                return "TransparentMood_7.png";
            }
            if (activation < 2 && pleasance <= 3)
            {
                return "TransparentMood_4.png";
            }
            return "TransparentMood_1.png";
        }
    }
}
