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
using System.Runtime.Remoting.Messaging;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewViewModel : BaseViewModel
    {
        private List<SurveyMeasurement> _measurements;
        private List<ProximityEntry> _proximity;


        private DateTime LastDateLoaded = DateTime.UtcNow.Date;

        public int CurrentType = (int)Core.Helpers.SurveyHardcodedEnumeration.Pleasance;

        public string _currentTypeName;
        public string CurrentTypeName
        {
            get => _currentTypeName;
            set => SetProperty(ref _currentTypeName, value);
        }

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


        private int CurrentPredictionAreFor { get; set; }
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

        private MyTabMenuViewModel _tabMenuViewModel;
        public MyTabMenuViewModel TabMenuViewModel
        {
            get => _tabMenuViewModel;
            set => SetProperty(ref _tabMenuViewModel, value);
        }

        private Command<int> _onTabChangedCommand;
        public Command<int> OnTabChangedCommand
        {
            get => _onTabChangedCommand;
            set => SetProperty(ref _onTabChangedCommand, value);
        }

        public SurveyOverviewViewModel()
        {
            TabMenuViewModel = ServiceLocator.Instance.Get<IMeasurementService>().GetQuestionsToDisplayInTabMenu();
            var first = TabMenuViewModel.Items.FirstOrDefault();
            first.IsActive = true;
            CurrentType = first.Id;
            RefreshData();
            OnTabChangedCommand = new Command<int>((index) =>
            {
                CurrentType = index;
                RefreshData();
            });
            ServiceLocator.Instance.Get<ISharedDatabaseContext>().WhenEntryAdded<GenericQuestion>().Subscribe(x =>
            {
                foreach (var newQuestionObj in x.Entites)
                {
                    var newQuestion = (GenericQuestion)newQuestionObj;
                    if (newQuestion.QuestionShort != null && TabMenuViewModel.Items.All(item => item.Id != newQuestion.QuestionId))
                    {
                        TabMenuViewModel.Items.Add(new TabMenuItemViewModel { Text = newQuestion.QuestionShort, Id = newQuestion.QuestionId });
                    }
                }
            });

            TabMenuViewModel.Items.FirstOrDefault(x => x.Id == (int)CurrentType).IsActive = true;
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
            _proximity = ServiceLocator.Instance.Get<IProximityService>().GetProximityEntries();
            Items = new ObservableCollection<SurveyOverviewItemViewModel>();
            LastDateLoaded = DateTime.UtcNow.Date;
            Initialize(CurrentType);
        }

        private void SetPredictionInView(IList<PredictionEntry> predictions)
        {
            var lastPrediction = predictions?.FirstOrDefault(x => x != null && x.QuestionId == (int)CurrentType) ?? null;
            if (lastPrediction != null)
            {
                HasPredictions = true;
                DisplayLastResponse = false;
                PredictionValue = UtilHelper.GetNewScaleFromOldAsString(lastPrediction.PredictedValue, CurrentType);
                PredictionDateTime = UtilHelper.TimeAgo(lastPrediction.Timestamp);
                CurrentPredictionAreFor = CurrentType;
            }
            else if (lastPrediction == null && CurrentType != CurrentPredictionAreFor)
            {
                HasPredictions = false;
                DisplayLastResponse = true;
                CurrentPredictionAreFor = 0;
            }
        }

        public void Initialize(int type)
        {
            //reset lastDateLoaded, so that on tab change, the right data is loaded
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
                                                  .Average(measurements => measurements?.SurveyItemMeasurement
                                                       ?.FirstOrDefault(item => item.QuestionId == (int)type)?.Answer ?? 0);

            NumberOfResponses = _measurements.Count;
            LastResponse = _measurements.OrderByDescending(x => x.Timestamp).FirstOrDefault()?.Timestamp.ToLocalTime() ?? new DateTime();
            LoadMoreData();
        }

        private void SetActiveType(int type)
        {

            CurrentType = type;
            CurrentTypeName = ServiceLocator.Instance.Get<ISharedDatabaseContext>().Get<GenericQuestion>(x => x.QuestionId == type)?.QuestionShort ?? "";
        }

        public void LoadMoreData()
        {
            var groupedByDate = _measurements.GroupBy(x => x.Timestamp.ToLocalTime().Date).OrderByDescending(x => x.Key);
            var groupedByDateProximity = _proximity.GroupBy(x => x.Timestamp.ToLocalTime().Date).OrderByDescending(x => x.Key);

            var days = new List<DateTime>();
            for (var i = 0; i < 14; i++)
            {
                days.Add(LastDateLoaded.Subtract(TimeSpan.FromDays(i)).Date);
            }
            LastDateLoaded = days.LastOrDefault();
            if (LastDateLoaded == default(DateTime))
            {
                return;
            }

            foreach (var day in days)
            {
                var surveysOnDay = groupedByDate.FirstOrDefault(x => x.Key == day)?.ToList() ?? new List<SurveyMeasurement>();
                var proximityOnDay = groupedByDateProximity.FirstOrDefault(x => x.Key == day)?.ToList() ?? new List<ProximityEntry>();
                Items.Add(new SurveyOverviewItemViewModel(day, surveysOnDay, proximityOnDay, CurrentType));
            }
        }
    }
}

