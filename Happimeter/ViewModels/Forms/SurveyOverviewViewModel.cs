using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Happimeter.Core.Database;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewViewModel : BaseViewModel
    {
        public SurveyOverviewViewModel()
        {
        }

        public SurveyOverviewViewModel(List<SurveyMeasurement> measurement) 
        {
            OverallAverageMood = measurement.Where(x => x.SurveyItemMeasurement.Any(y => y.HardcodedQuestionId == 1)).Average(measurements => measurements.SurveyItemMeasurement.FirstOrDefault(item => item.HardcodedQuestionId == 1)?.Answer ?? 0);   
            OverallAverageActivation = measurement.Where(x => x.SurveyItemMeasurement.Any(y => y.HardcodedQuestionId == 2)).Average(measurements => measurements.SurveyItemMeasurement.FirstOrDefault(item => item.HardcodedQuestionId == 2)?.Answer ?? 0);
            NumberOfResponses = measurement.Count;
            LastResponse = measurement.OrderByDescending(x => x.Timestamp).FirstOrDefault()?.Timestamp ?? new DateTime();

            var groupedByDate = measurement.GroupBy(x => x.Timestamp.Date);
            var items = new ObservableCollection<SurveyOverviewItemViewModel>();
            foreach (var group in groupedByDate) {
                items.Add(new SurveyOverviewItemViewModel(group));
            }

            Items = items;
        }

        private int _numberOfResponses;
        public int NumberOfResponses 
        {
            get => _numberOfResponses;
            set => SetProperty(ref _numberOfResponses, value);
        }

        private double _overallAverageMood;
        public double OverallAverageMood
        {
            get => _overallAverageMood;
            set => SetProperty(ref _overallAverageMood, value);
        }

        private double _overallAverageActivation;
        public double OverallAverageActivation
        {
            get => _overallAverageActivation;
            set => SetProperty(ref _overallAverageActivation, value);
        }

        private DateTime _lastResponse;
        public DateTime LastResponse
        {
            get => _lastResponse;
            set => SetProperty(ref _lastResponse, value);
        }

        public ObservableCollection<SurveyOverviewItemViewModel> Items { get; set; }
    }
}

