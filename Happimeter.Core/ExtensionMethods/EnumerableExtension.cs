using System;
using System.Collections.Generic;
using System.Linq;

namespace Happimeter.Core.ExtensionMethods
{
    public static class EnumerableExtension
    {
        public static double StdDev(this IEnumerable<double> values)
        {
            // ref: http://warrenseen.com/blog/2006/03/13/how-to-calculate-standard-deviation/
            double mean = 0.0;
            double sum = 0.0;
            double stdDev = 0.0;
            int n = 0;
            foreach (double val in values)
            {
                n++;
                double delta = val - mean;
                mean += delta / n;
                sum += delta * (val - mean);
            }
            if (1 < n)
                stdDev = Math.Sqrt(sum / (n - 1));

            return stdDev;
        }

        public static double Quantile1(this IEnumerable<double> values)
        {
            var count = values.Count();
            var q1 = values.Count() / 4;
            return values.ToList().OrderBy(x => x).Take(q1).LastOrDefault();
        }

        public static double Quantile2(this IEnumerable<double> values)
        {
            var count = values.Count();
            var q1 = values.Count() / 2;
            return values.ToList().OrderBy(x => x).Take(q1).LastOrDefault();
        }

        public static double Quantile3(this IEnumerable<double> values)
        {
            var count = values.Count();
            var q1 = values.Count() / 4;
            return values.ToList().OrderBy(x => x).Take(count - q1).LastOrDefault();
        }
    }
}
