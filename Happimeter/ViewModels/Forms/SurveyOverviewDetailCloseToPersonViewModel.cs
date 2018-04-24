using System;
using System.Collections.Generic;
using Happimeter.Core.Database;
using System.Runtime.Remoting.Messaging;
using Microcharts;
using System.Linq;
using SkiaSharp;
using System.Diagnostics;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewDetailCloseToPersonViewModel : BaseViewModel
    {
        public SurveyOverviewDetailCloseToPersonViewModel(DateTime day, string personName, List<ProximityEntry> proximityData)
        {
            PersonName = personName;

            var chartEntries = new List<Entry>();
            var intervalList = new List<DateTime>();
            var currentDate = day;
            var end = currentDate.AddHours(24);
            while (currentDate <= end)
            {
                intervalList.Add(currentDate);
                currentDate = currentDate.AddMinutes(30);
            }

            var first = proximityData.OrderBy(x => x.Timestamp).FirstOrDefault()?.Timestamp.ToLocalTime() - TimeSpan.FromMinutes(30) ?? null;
            var last = proximityData.OrderByDescending(x => x.Timestamp).FirstOrDefault()?.Timestamp.ToLocalTime() + TimeSpan.FromMinutes(30) ?? null;

            if (first == null || last == null)
            {
                return;
            }

            foreach (var timepoint in intervalList)
            {
                if (timepoint < first || timepoint > last)
                {
                    continue;
                }
                var shouldHaveLabel = timepoint.Minute == 0 || timepoint.Minute == 30;
                var proximityPoints = proximityData.Where(x => x.Timestamp.ToLocalTime() >= timepoint && x.Timestamp.ToLocalTime() <= timepoint.AddMinutes(30)).ToList();
                var value = GetProximityLevel(proximityPoints);
                var entry = new Entry((float)value)
                {
                    //Color = ColorHelper.GetColorRelatingToScale(y.Answer, 100, SKColors.OrangeRed, SKColors.LimeGreen),
                    //Label = shouldHaveLabel ? timepoint.ToString("HH:mm") : null,
                    ValueLabel = shouldHaveLabel ? timepoint.ToString("HH:mm") : null
                };
                chartEntries.Add(entry);
            }
            ClosenessChart = new LineChart
            {
                Entries = chartEntries,
                LineMode = LineMode.Straight,
                Margin = 5,
                LineSize = 1,
                MaxValue = 3,
                MinValue = 0,
                PointSize = 0,
                //LabelTextSize = 3,
                BackgroundColor = SKColors.Transparent,

            };
        }

        private string _personName;
        public string PersonName
        {
            get => _personName;
            set => SetProperty(ref _personName, value);
        }

        private Chart _closenessChart;
        public Chart ClosenessChart
        {
            get => _closenessChart;
            set => SetProperty(ref _closenessChart, value);
        }

        private int GetProximityLevel(List<ProximityEntry> entries)
        {
            if (!entries.Any())
            {
                return 0;
            }
            var avergae = entries.Average(x => x.Average);
            if (avergae > -75)
            {
                return 3;
            }
            if (avergae > -85)
            {
                return 2;
            }
            return 1;
        }
    }
}
