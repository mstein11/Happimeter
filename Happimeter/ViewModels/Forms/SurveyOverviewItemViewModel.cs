using System;
using System.Collections.Generic;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Helpers;
using Microcharts;
using SkiaSharp;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewItemViewModel : BaseViewModel
    {
        public SurveyOverviewItemViewModel()
        {
        }

        public SurveyOverviewItemViewModel(IGrouping<DateTime, SurveyMeasurement> data, SurveyHardcodedEnumeration type)
        {

            MoodChart = new LineChart
            {
                Entries = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => new Entry((float)y.Answer)
                {
                    Color = ColorHelper.GetColorRelatingToScale(y.Answer, 100,  SKColors.OrangeRed, SKColors.LimeGreen),
                    Label = y.SurveyMeasurement.Timestamp.ToLocalTime().ToString("HH:mm"),
                    ValueLabel = y.AnswerDisplay.ToString()
                })),
                LineMode = LineMode.Straight,
                Margin = 10,
                LineSize = 2,
                MaxValue = 100,
                MinValue = 0,
                BackgroundColor = SKColors.Transparent
            };
            Date = data.Key;
            NumberOfResponses = data.Count();

            MinMood = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => y.AnswerDisplay)).DefaultIfEmpty(0).Min();
            MaxMood = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => y.AnswerDisplay)).DefaultIfEmpty(0).Max();
            AvgMood = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => y.AnswerDisplay)).DefaultIfEmpty(0).Average();
        }

        private DateTime _date;
        public DateTime Date {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private int _numberOfResponses;
        public int NumberOfResponses
        {
            get => _numberOfResponses;
            set => SetProperty(ref _numberOfResponses, value);
        }

        private Chart _moodChart;
        public Chart MoodChart  
        {
            get => _moodChart;
            set => SetProperty(ref _moodChart, value);
        }

        private double _avgMood;
        public double AvgMood 
        {
            get => _avgMood;
            set => SetProperty(ref _avgMood, value);
        }

        private double _minMood;
        public double MinMood
        {
            get => _minMood;
            set => SetProperty(ref _minMood, value);
        }

        private double _maxMood;
        public double MaxMood
        {
            get => _maxMood;
            set => SetProperty(ref _maxMood, value);
        }
            
    }
}
