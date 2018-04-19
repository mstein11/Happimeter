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

        private void InitializeProximityData(DateTime date, List<ProximityEntry> data, int type)
        {
            CloseToPeople = data.GroupBy(x => x.CloseToUserId).Count();
        }

        private void InitializeSurveyData(DateTime date, List<SurveyMeasurement> data, int type)
        {
            HasSurveyData = true;
            DoesNotHaveSurveyData = false;
            var entries = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => new Entry((float)y.Answer)
            {
                Color = ColorHelper.GetColorRelatingToScale(y.Answer, 100, SKColors.OrangeRed, SKColors.LimeGreen),
                Label = y.SurveyMeasurement.Timestamp.ToLocalTime().ToString("HH:mm"),
                ValueLabel = y.AnswerDisplay.ToString()
            })).ToList();
            if (entries.Count() == 1)
            {
                //if we only have one, lets copy that one so that the graph has 2 points to work with
                entries.Add(entries.FirstOrDefault());
            }
            MoodChart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Straight,
                Margin = 10,
                LineSize = 2,
                MaxValue = 100,
                MinValue = 0,
                BackgroundColor = SKColors.Transparent
            };
            NumberOfResponses = data.Count();

            MinMood = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => y.AnswerDisplay)).DefaultIfEmpty(0).Min();
            MaxMood = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => y.AnswerDisplay)).DefaultIfEmpty(0).Max();
            AvgMood = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => y.AnswerDisplay)).DefaultIfEmpty(0).Average();
        }

        public SurveyOverviewItemViewModel(DateTime date, List<SurveyMeasurement> data, List<ProximityEntry> proximityData, int type)
        {
            Date = date;
            if (!data.Any())
            {
                HasSurveyData = false;
                DoesNotHaveSurveyData = true;
            }
            else
            {
                InitializeSurveyData(date, data, type);
                HasSurveyData = true;
                DoesNotHaveSurveyData = false;
            }

            if (!proximityData.Any())
            {
                HasProximityData = false;
                DoesNotHaveProximityData = true;
            }
            else
            {
                HasProximityData = true;
                DoesNotHaveProximityData = false;
                InitializeProximityData(date, proximityData, type);
            }
        }
        /*
        public SurveyOverviewItemViewModel(IGrouping<DateTime, SurveyMeasurement> data, int type)
        {
            Date = data.Key;
            if (!data.Any())
            {
                HasSurveyData = false;
                DoesNotHaveSurveyData = true;
                return;
            }
            HasSurveyData = true;
            DoesNotHaveSurveyData = false;

            var entries = data.SelectMany(x => x.SurveyItemMeasurement.Where(y => y.QuestionId == (int)type).Select(y => new Entry((float)y.Answer)
            {
                Color = ColorHelper.GetColorRelatingToScale(y.Answer, 100, SKColors.OrangeRed, SKColors.LimeGreen),
                Label = y.SurveyMeasurement.Timestamp.ToLocalTime().ToString("HH:mm"),
                ValueLabel = y.AnswerDisplay.ToString()
            })).ToList();
            if (entries.Count() == 1)
            {
                //if we only have one, lets copy that one so that the graph has 2 points to work with
                entries.Add(entries.FirstOrDefault());
            }
            MoodChart = new LineChart
            {
                Entries = entries,
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
        */

        private int _closeToPeople;
        public int CloseToPeople
        {
            get => _closeToPeople;
            set => SetProperty(ref _closeToPeople, value);
        }

        private bool _hasSurveyData;
        public bool HasSurveyData
        {
            get => _hasSurveyData;
            set => SetProperty(ref _hasSurveyData, value);
        }

        private bool _doesNotHaveSurveyData;
        public bool DoesNotHaveSurveyData
        {
            get => _doesNotHaveSurveyData;
            set => SetProperty(ref _doesNotHaveSurveyData, value);
        }

        private bool _hasProximityData;
        public bool HasProximityData
        {
            get => _hasProximityData;
            set => SetProperty(ref _hasProximityData, value);
        }
        private bool _doesNotHaveProximityData;
        public bool DoesNotHaveProximityData
        {
            get => _doesNotHaveProximityData;
            set => SetProperty(ref _doesNotHaveProximityData, value);
        }

        private DateTime _date;
        public DateTime Date
        {
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
