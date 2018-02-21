using System;
using System.Diagnostics.Contracts;
using SkiaSharp;

namespace Happimeter.Helpers
{
    public static class ColorHelper
    {

        public static SKColor GetColorRelatingToScale(int value, int max, SKColor colorStart, SKColor colorEnd) {
            var coefficient = (float) value / max;

            var redStart = Convert.ToInt32(colorStart.Red);
            var greenStart = Convert.ToInt32(colorStart.Green);
            var blueStart = Convert.ToInt32(colorStart.Blue);

            var redEnd = Convert.ToInt32(colorEnd.Red);
            var greenEnd = Convert.ToInt32(colorEnd.Green);
            var blueEnd = Convert.ToInt32(colorEnd.Blue);

            var redDifference = redEnd - redStart;
            var greenDifference = greenEnd - greenStart;
            var blueDifference = blueEnd - blueStart;

            var redFinal = redDifference * coefficient + redStart;
            var greenFinal = greenDifference * coefficient + greenStart;
            var blueFinal = blueDifference * coefficient + blueStart;

            return new SKColor((byte)redFinal, (byte)greenFinal, (byte) blueFinal, 1);
        }
     }
}
