using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Happimeter.Events;
using Happimeter.Interfaces;
using System;

namespace Happimeter.ViewModels.Forms
{
    public class SettingsPageViewModel : BaseViewModel
    {

        private List<int> _surveyIdsNotSynched = new List<int>();
        private List<int> _sensorIdsNotSynched = new List<int>();

        public SettingsPageViewModel()
        {
            UserEmail = ServiceLocator
                .Instance
                .Get<IAccountStoreService>()
                .GetAccount()?.Username ?? "";

            var hasBtPairing = ServiceLocator.Instance.Get<ISharedDatabaseContext>().Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive) != null;
            ShowPushQuestionsToWatchButton = hasBtPairing;

            NumberOfGenericQuestions = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyQuestions().SurveyItems.Count() - 2;
            SaveGenericGroupButtonEnabled = true;
            Logout = new Command(() =>
            {
                ServiceLocator
                    .Instance
                    .Get<IAccountStoreService>()
                    .DeleteAccount();

                ServiceLocator
                    .Instance
                    .Get<ISharedDatabaseContext>()
                    .ResetDatabase();

                ServiceLocator
                    .Instance
                    .Get<INativeNavigationService>()
                    .NavigateToLoginPage();
            });

            PushGenericQuestionToWatchButtonText = "Push Questions to Watch";
            PushQuestionsToWatchButtonEnabled = true;
            PushGenericQuestionsToWatchCommand = new Command(() =>
            {
                PushQuestionsToWatchButtonEnabled = false;
                PushGenericQuestionToWatchButtonText = "Loading...";
                ServiceLocator.Instance.Get<IBluetoothService>().SendGenericQuestions((connectionUpdate) => {
                    Timer timer = null;
                    switch (connectionUpdate) {
                        case BluetoothWriteEvent.Initialized:            
                            break;
                        case BluetoothWriteEvent.Connected:
                            PushGenericQuestionToWatchButtonText = "Loading... (connected to device)";
                            break;

                        case BluetoothWriteEvent.Complete:
                            PushGenericQuestionToWatchButtonText = "Successfully pushed Questions";
                            timer = null;
                            timer = new Timer((obj) =>
                            {
                                PushGenericQuestionToWatchButtonText = "Push Questions to Watch";
                                PushQuestionsToWatchButtonEnabled = true;
                                timer.Dispose();
                            }, null, 2000, System.Threading.Timeout.Infinite);
                            break;
                        case BluetoothWriteEvent.ErrorOnConnectingToDevice:
                            PushGenericQuestionToWatchButtonText = "Error!";
                            timer = null;
                            timer = new Timer((obj) =>
                            {
                                PushGenericQuestionToWatchButtonText = "Push Questions to Watch";
                                PushQuestionsToWatchButtonEnabled = true;
                                if (timer != null)
                                {
                                    timer.Dispose();
                                }
                            }, null, 2000, System.Threading.Timeout.Infinite);
                            break;
                        case BluetoothWriteEvent.ErrorOnWrite:
                            PushGenericQuestionToWatchButtonText = "Error!";
                            timer = null;
                            timer = new Timer((obj) =>
                            {
                                PushGenericQuestionToWatchButtonText = "Push Questions to Watch";
                                PushQuestionsToWatchButtonEnabled = true;
                                if (timer != null)
                                {
                                    timer.Dispose();
                                }
                            }, null, 2000, System.Threading.Timeout.Infinite);
                            break;
                    }
                });
            });

            ChangeGenericQuestionGroupId = new Command(async () =>
            {
                SaveGenericGroupButtonEnabled = false;
                GenericGroupButtonText = "Loading...";
                var questions = await ServiceLocator
                    .Instance
                    .Get<IMeasurementService>()
                    .DownloadAndSaveGenericQuestions();
                Timer timer = null;
                if (questions == null) {
                    //Error while downloading questions
                    GenericGroupButtonText = "Error Downloading Questinos";
                } else {
                    NumberOfGenericQuestions = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyQuestions().SurveyItems.Count() - 2;   
                    GenericGroupButtonText = $"Successfully downloaded  {questions.Count} questions";
                }

                timer = new Timer((obj) =>
                {
                    SaveGenericGroupButtonEnabled = true;
                    GenericGroupButtonText = "Save & Download";
                    timer.Dispose();
                }, null, 2000, System.Threading.Timeout.Infinite);
            });

            UploadCommand = new Command(() =>
            {
                var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
                apiService.UploadMood();
                apiService.UploadSensor();
            });

            ServiceLocator.Instance.Get<IHappimeterApiService>().UploadMoodStatusUpdate += HandleUploadStatusUpdate;
            ServiceLocator.Instance.Get<IHappimeterApiService>().UploadSensorStatusUpdate += HandleUploadStatusUpdate;

            var needSync = ServiceLocator.Instance.Get<IMeasurementService>().HasUnsynchronizedChanges();
            if (needSync) {
                UnsyncronizedChangedVisible = true;
            }

            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            context.WhenEntryChanged<SharedBluetoothDevicePairing>().Subscribe(eventInfo =>
            {
                if (eventInfo.Entites.Cast<SharedBluetoothDevicePairing>().Any(x => x.IsPairingActive) 
                    && eventInfo.TypeOfEvent != Core.Events.DatabaseChangedEventTypes.Deleted
                    && eventInfo.TypeOfEvent != Core.Events.DatabaseChangedEventTypes.DeleteAll) {
                    ShowPushQuestionsToWatchButton = true;
                } else {
                    ShowPushQuestionsToWatchButton = false;
                }
            });
            context.WhenEntryAdded<SurveyMeasurement>().Subscribe(eventInfos =>
            {
                var surveyMeasurements = eventInfos.Entites.Cast<SurveyMeasurement>();
                var notUploaded = surveyMeasurements.Where(x => !x.IsUploadedToServer);
                foreach (var entry in notUploaded) {
                    _surveyIdsNotSynched.Add(entry.Id);
                }
                if (_sensorIdsNotSynched.Any() || _surveyIdsNotSynched.Any())
                {
                    UnsyncronizedChangedVisible = true;
                }
                else
                {
                    UnsyncronizedChangedVisible = false;
                }
            });
            context.WhenEntryAdded<SensorMeasurement>().Subscribe(eventInfos =>
            {
                var surveyMeasurements = eventInfos.Entites.Cast<SensorMeasurement>();
                var notUploaded = surveyMeasurements.Where(x => !x.IsUploadedToServer);
                foreach (var entry in notUploaded)
                {
                    _sensorIdsNotSynched.Add(entry.Id);
                }
                if (_sensorIdsNotSynched.Any() || _surveyIdsNotSynched.Any())
                {
                    UnsyncronizedChangedVisible = true;
                }
                else
                {
                    UnsyncronizedChangedVisible = false;
                }
            });
            context.WhenEntryUpdated<SurveyMeasurement>().Subscribe(eventInfos =>
            {
                var surveyMeasurements = eventInfos.Entites.Cast<SurveyMeasurement>();

                foreach (var entry in surveyMeasurements)
                {
                    if (!entry.IsUploadedToServer) {
                        if (!_surveyIdsNotSynched.Contains(entry.Id)) {
                            _surveyIdsNotSynched.Add(entry.Id);        
                        }
                    } else {
                        if (_surveyIdsNotSynched.Contains(entry.Id))
                        {
                            _surveyIdsNotSynched.Remove(entry.Id);
                        }
                    }
                }
                if (_sensorIdsNotSynched.Any() || _surveyIdsNotSynched.Any())
                {
                    UnsyncronizedChangedVisible = true;
                }
                else
                {
                    UnsyncronizedChangedVisible = false;
                }
            });
            context.WhenEntryUpdated<SensorMeasurement>().Subscribe(eventInfos =>
            {
                var sensorMeasurements = eventInfos.Entites.Cast<SensorMeasurement>();

                foreach (var entry in sensorMeasurements)
                {
                    if (!entry.IsUploadedToServer)
                    {
                        if (!_sensorIdsNotSynched.Contains(entry.Id))
                        {
                            _sensorIdsNotSynched.Add(entry.Id);
                        }
                    }
                    else
                    {
                        if (_sensorIdsNotSynched.Contains(entry.Id))
                        {
                            _sensorIdsNotSynched.Remove(entry.Id);
                        }
                    }
                }
                if (_sensorIdsNotSynched.Any() || _surveyIdsNotSynched.Any())
                {
                    UnsyncronizedChangedVisible = true;
                }
                else
                {
                    UnsyncronizedChangedVisible = false;
                }
            });
        }

        private string _userEmail;
        public string UserEmail 
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        private bool _unsyncronizedChangedVisible;
        public bool UnsyncronizedChangedVisible
        {
            get => _unsyncronizedChangedVisible;
            set => SetProperty(ref _unsyncronizedChangedVisible, value);
        }

        private bool _synchronizingStatusIsVisible;
        public bool SynchronizingStatusIsVisible
        {
            get => _synchronizingStatusIsVisible;
            set => SetProperty(ref _synchronizingStatusIsVisible, value);
        }

        private string _synchronizingStatus;
        public string SynchronizingStatus
        {
            get => _synchronizingStatus;
            set => SetProperty(ref _synchronizingStatus, value);
        }

        private int _numberOfGenericQuestions;
        public int NumberOfGenericQuestions
        {
            get => _numberOfGenericQuestions;
            set => SetProperty(ref _numberOfGenericQuestions, value);
        }

        private bool _saveGenericGroupButtonEnabled;
        public bool SaveGenericGroupButtonEnabled
        {
            get => _saveGenericGroupButtonEnabled;
            set => SetProperty(ref _saveGenericGroupButtonEnabled, value);
        }
        private string _genericGroupButtonText = "Save & Download";
        public string GenericGroupButtonText 
        {
            get => _genericGroupButtonText;
            set => SetProperty(ref _genericGroupButtonText, value);
        }

        private bool _showPushQuestionsToWatchButton;
        public bool ShowPushQuestionsToWatchButton 
        {
            get => _showPushQuestionsToWatchButton;
            set => SetProperty(ref _showPushQuestionsToWatchButton, value);
        }

        private string _pushGenericQuestionToWatchButtonText;
        public string PushGenericQuestionToWatchButtonText
        {
            get => _pushGenericQuestionToWatchButtonText;
            set => SetProperty(ref _pushGenericQuestionToWatchButtonText, value);
        }

        private bool _pushQuestionsToWatchButtonEnabled;
        public bool PushQuestionsToWatchButtonEnabled
        {
            get => _pushQuestionsToWatchButtonEnabled;
            set => SetProperty(ref _pushQuestionsToWatchButtonEnabled, value);
        }

        public ICommand Logout { protected set; get; }
        public ICommand ChangeGenericQuestionGroupId { protected set; get; }
        public ICommand UploadCommand { protected set; get; }
        public ICommand PushGenericQuestionsToWatchCommand { protected set; get; }

        private void HandleUploadStatusUpdate(object sender, SynchronizeDataEventArgs e) {
            switch (e.EventType) {
                case SyncronizeDataStates.UploadingMood:
                    DisplayIndication($"Uploading Mood: {e.EntriesSent} of {e.TotalEntries} uploaded");
                    break;
                case SyncronizeDataStates.UploadingSensor:
                    DisplayIndication($"Uploading Sensor: {e.EntriesSent} of {e.TotalEntries} uploaded");
                    break;
                case SyncronizeDataStates.UploadingSuccessful:
                    DisplayIndication($"Upload successful", 2000);
                    break;
                case SyncronizeDataStates.UploadingError:
                    DisplayIndication($"Error while Uploading", 2000);
                    break;
            }
        }

        private void DisplayIndication(string text, int? milliseconds = null)
        {
            SynchronizingStatusIsVisible = true;
            SynchronizingStatus = text;
            Timer timer = null;


            if (milliseconds != null)
            {
                timer = new Timer((obj) =>
                {
                    SynchronizingStatus = null;
                    SynchronizingStatusIsVisible = false;
                    timer.Dispose();
                }, null, milliseconds.Value, System.Threading.Timeout.Infinite);
            }
        }

    }
}
