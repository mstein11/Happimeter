using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Happimeter.Core.Database;
using Happimeter.Events;
using Happimeter.Interfaces;
using Happimeter.Services;

namespace Happimeter.ViewModels.Forms
{
    public class SettingsPageViewModel : BaseViewModel
    {

        private List<SurveyMeasurement> _surveysNotSynched = new List<SurveyMeasurement>();
        private List<SensorMeasurement> _sensorsNotSynched = new List<SensorMeasurement>();

        public SettingsPageViewModel()
        {
            UserEmail = ServiceLocator
                .Instance
                .Get<IAccountStoreService>()
                .GetAccount().Username;
            
            GenericQuestionGroupId = ServiceLocator
                .Instance
                .Get<IConfigService>()
                .GetConfigValueByKey(ConfigService.GenericQuestionGroupIdKey);

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

            ChangeGenericQuestionGroupId = new Command(() =>
            {
                ServiceLocator
                    .Instance
                    .Get<IConfigService>()
                    .AddOrUpdateConfigEntry(ConfigService.GenericQuestionGroupIdKey, GenericQuestionGroupId);
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

            ServiceLocator.Instance.Get<ISharedDatabaseContext>().ModelChanged += (sender, e) => {
                var survey = sender as SurveyMeasurement;
                var sensor = sender as SurveyMeasurement;
                if (survey != null && !survey.IsUploadedToServer) {
                    UnsyncronizedChangedVisible = true;
                    _surveysNotSynched.Add(survey);
                } else if (survey != null && survey.IsUploadedToServer) {
                    _surveysNotSynched.Remove(survey);
                }
                if (sensor != null && !sensor.IsUploadedToServer)
                {
                    UnsyncronizedChangedVisible = true;
                    _surveysNotSynched.Add(sensor);
                } 
                else if (sensor != null && sensor.IsUploadedToServer)
                {
                    _surveysNotSynched.Remove(sensor);
                }

                if (!_surveysNotSynched.Any() && !_sensorsNotSynched.Any())
                {
                    UnsyncronizedChangedVisible = false;
                }
            };
        }

        private string _userEmail;
        public string UserEmail 
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        private string _genericQuestionGroupId;
        public string GenericQuestionGroupId 
        {
            get => _genericQuestionGroupId;
            set => SetProperty(ref _genericQuestionGroupId, value);
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

        public ICommand Logout { protected set; get; }
        public ICommand ChangeGenericQuestionGroupId { protected set; get; }
        public ICommand UploadCommand { protected set; get; }

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
