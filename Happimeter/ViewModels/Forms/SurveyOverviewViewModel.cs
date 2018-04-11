using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Helpers;
using Happimeter.Interfaces;
using System.Diagnostics;
using Happimeter.Core.Helpers;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewViewModel : BaseViewModel
    {
        private List<SurveyMeasurement> _measurements;

        public Core.Helpers.SurveyHardcodedEnumeration CurrentType = Core.Helpers.SurveyHardcodedEnumeration.Pleasance;

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

        public bool _displayLastResponse;
        public bool DisplayLastResponse
        {
            get => _displayLastResponse;
            set => SetProperty(ref _displayLastResponse, value);
        }

        public string _predictionValue;
        public string PredictionValue
        {
            get => _predictionValue;
            set => SetProperty(ref _predictionValue, value);
        }
        public string _predictionDateTime;
        public string PredictionDateTime
        {
            get => _predictionDateTime;
            set => SetProperty(ref _predictionDateTime, value);
        }
        public bool _hasPredictions;
        public bool HasPredictions
        {
            get => _hasPredictions;
            set => SetProperty(ref _hasPredictions, value);
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
            ServiceLocator.Instance.Get<ISharedDatabaseContext>().WhenEntryAdded<PredictionEntry>().Subscribe(x =>
            {
                SetPredictionInView(x.Entites.Cast<PredictionEntry>().ToList());
            });
            if (HasPredictions)
            {
                DisplayLastResponse = false;
            }
            else
            {
                DisplayLastResponse = true;
            }
        }

        public void RefreshData()
        {
            _measurements = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyData();
            Items = new ObservableCollection<SurveyOverviewItemViewModel>();
            Initialize(CurrentType);
        }

        private void SetPredictionInView(IList<PredictionEntry> predictions)
        {
            var lastPrediction = predictions?.FirstOrDefault(x => x != null && x.QuestionId == (int)CurrentType) ?? null;
            if (lastPrediction != null)
            {
                HasPredictions = true;
                PredictionValue = UtilHelper.GetNewScaleFromOldAsString(lastPrediction.PredictedValue, CurrentType);
                PredictionDateTime = UtilHelper.TimeAgo(lastPrediction.Timestamp);
            }
        }

        public void Initialize(Core.Helpers.SurveyHardcodedEnumeration type)
        {
            SetActiveType(type);
            var predictions = ServiceLocator.Instance.Get<IPredictionService>().GetLastPrediction();
            SetPredictionInView(predictions);

            if (!_measurements.Any())
            {
                HasData = false;
                return;
            }
            HasData = true;



            OverallAverageResponse = _measurements.Where(x => x.SurveyItemMeasurement.Any(y => y.QuestionId == (int)type))
                                                  .DefaultIfEmpty()
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

        private void SetActiveType(Core.Helpers.SurveyHardcodedEnumeration type)
        {
            if (type == SurveyHardcodedEnumeration.Activation)
            {
                ActivationIsActive = true;
                PleasanceIsActive = false;
            }
            else
            {
                ActivationIsActive = false;
                PleasanceIsActive = true;
            }

            CurrentType = type;
        }
    }
}

