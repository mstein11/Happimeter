using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Helpers;
using Happimeter.Interfaces;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewViewModel : BaseViewModel
    {
        private List<SurveyMeasurement> _measurements;

        public SurveyHardcodedEnumeration CurrentType = SurveyHardcodedEnumeration.Pleasance;

        private bool _pleasanceIsActive;
        public bool PleasanceIsActive 
        {
            get => _pleasanceIsActive;
            set => SetProperty(ref _pleasanceIsActive, value);
        }

        private bool _activationIsActive;
        public bool ActivationIsActive
        {
            get => _activationIsActive;
            set => SetProperty(ref _activationIsActive, value);
        }

        private bool _hasData;
        public bool HasData
        {
            get => _hasData;
            set => SetProperty(ref _hasData, value);
        }

        private int _numberOfResponses;
        public int NumberOfResponses
        {
            get => _numberOfResponses;
            set => SetProperty(ref _numberOfResponses, value);
        }

        private double _overallAverageResponse;
        public double OverallAverageResponse
        {
            get => _overallAverageResponse;
            set => SetProperty(ref _overallAverageResponse, value);
        }

        private DateTime _lastResponse;
        public DateTime LastResponse
        {
            get => _lastResponse;
            set => SetProperty(ref _lastResponse, value);
        }

        private ObservableCollection<SurveyOverviewItemViewModel> _items;
        public ObservableCollection<SurveyOverviewItemViewModel> Items 
        { 
            get => _items;
            set => SetProperty(ref _items, value);
        }


        public SurveyOverviewViewModel()
        {
            RefreshData();
        }

        public void RefreshData() {
            _measurements = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyData();
            Items = new ObservableCollection<SurveyOverviewItemViewModel>();
            Initialize(CurrentType);
        }

        public void Initialize(SurveyHardcodedEnumeration type) {
            SetActiveType(type);
            if (!_measurements.Any()) {
                HasData = false;
                return;
            }
            HasData = true;
            OverallAverageResponse = _measurements.Where(x => x.SurveyItemMeasurement.Any(y => y.QuestionId == (int)type))
                                              .Average(measurements => measurements.SurveyItemMeasurement
                                                       .FirstOrDefault(item => item.QuestionId == (int)type)?.Answer ?? 0);

            NumberOfResponses = _measurements.Count;
            LastResponse = _measurements.OrderByDescending(x => x.Timestamp).FirstOrDefault()?.Timestamp.ToLocalTime() ?? new DateTime();

            var groupedByDate = _measurements.GroupBy(x => x.Timestamp.Date).OrderByDescending(x => x.Key);

            Items.Clear();
            foreach (var group in groupedByDate)
            {
                Items.Add(new SurveyOverviewItemViewModel(group, type));
            }
        }

        private void SetActiveType(SurveyHardcodedEnumeration type) {
            if (type == SurveyHardcodedEnumeration.Activation) {
                ActivationIsActive = true;
                PleasanceIsActive = false;
            } else {
                ActivationIsActive = false;
                PleasanceIsActive = true;
            }

            CurrentType = type;
        }
    }
}

