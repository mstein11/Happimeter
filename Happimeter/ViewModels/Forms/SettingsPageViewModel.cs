using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Events;
using Happimeter.Interfaces;
using System;
using Xamarin.Forms;
using Happimeter.Views;
using Happimeter.Core.Services;

namespace Happimeter.ViewModels.Forms
{
    public class SettingsPageViewModel : BaseViewModel
    {

        private List<int> _surveyIdsNotSynched = new List<int>();
        private List<int> _sensorIdsNotSynched = new List<int>();

        public SettingsPageViewModel()
        {

            var listMenuEntries = new List<ListMenuItemViewModel> {
                new ListMenuItemViewModel {
                    ItemTitle = "Generic Questions",
                    IconBackgroundColor = Color.FromHex("#c62828"),
                    IconText = "G",
                    OnClickedCommand = new Command(() => {
                        ListMenuItemSelected?.Invoke(new SettingsGenericQuestionPage(), null);
                    })
                }
            };

            var hasBtPairing = ServiceLocator.Instance.Get<ISharedDatabaseContext>().Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive) != null;
            //only if we are paired to a watch, we show the watch config
            if (hasBtPairing)
            {
                listMenuEntries.Add(new ListMenuItemViewModel
                {
                    ItemTitle = "Watch Config",
                    IconBackgroundColor = Color.FromHex("#6a1b9a"),
                    IconText = "W",
                    OnClickedCommand = new Command(() =>
                    {
                        ListMenuItemSelected?.Invoke(new SettingsWatchConfigPage(), null);
                    })
                });
            }

            listMenuEntries.Add(new ListMenuItemViewModel
            {
                ItemTitle = "Debug",
                IconBackgroundColor = Color.FromHex("#9a7a1b"),
                IconText = "D",
                OnClickedCommand = new Command(() =>
                {
                    ListMenuItemSelected?.Invoke(new SettingsDebugPage(), null);
                    //ServiceLocator.Instance.Get<ILoggingService>().CreateDebugSnapshot();
                    //Application.Current.MainPage.DisplayAlert("Debug Snapshot Saved", "You successfully saved the debug snapshot! It will help us make the happimeter a better experience, thank you!", "Ok");
                })
            });


            ListMenuItems = listMenuEntries;
            UserEmail = ServiceLocator
                .Instance
                .Get<IAccountStoreService>()
                    .GetAccount()?.Username ?? "";

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

            UploadCommand = new Command(() =>
            {
                var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
                apiService.UploadMood();
                apiService.UploadSensor();
            });

            ServiceLocator.Instance.Get<IHappimeterApiService>().UploadMoodStatusUpdate += HandleUploadStatusUpdate;
            ServiceLocator.Instance.Get<IHappimeterApiService>().UploadSensorStatusUpdate += HandleUploadStatusUpdate;

            var needSync = ServiceLocator.Instance.Get<IMeasurementService>().HasUnsynchronizedChanges();
            if (needSync)
            {
                UnsyncronizedChangedVisible = true;
            }

            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            context.WhenEntryChanged<SharedBluetoothDevicePairing>().Subscribe(eventInfo =>
            {
                if (eventInfo.Entites.Cast<SharedBluetoothDevicePairing>().Any(x => x.IsPairingActive)
                        && eventInfo.TypeOfEvent != Core.Events.DatabaseChangedEventTypes.Deleted
                        && eventInfo.TypeOfEvent != Core.Events.DatabaseChangedEventTypes.DeleteAll)
                {
                    listMenuEntries = new List<ListMenuItemViewModel> {
                        new ListMenuItemViewModel {
                        ItemTitle = "Generic Questions",
                        IconBackgroundColor = Color.FromHex("#c62828"),
                        IconText = "G",
                        OnClickedCommand = new Command(() => {
                            ListMenuItemSelected?.Invoke(new SettingsGenericQuestionPage(), null);
                            })
                        },new ListMenuItemViewModel
                        {
                            ItemTitle = "Watch Config",
                            IconBackgroundColor = Color.FromHex("#6a1b9a"),
                            IconText = "W",
                            OnClickedCommand = new Command(() =>
                            {
                                ListMenuItemSelected?.Invoke(new SettingsWatchConfigPage(), null);
                            })
                        }
                    };
                    ListMenuItems = listMenuEntries;
                }
                else
                {
                    listMenuEntries = new List<ListMenuItemViewModel> {
                        new ListMenuItemViewModel {
                        ItemTitle = "Generic Questions",
                        IconBackgroundColor = Color.FromHex("#c62828"),
                        IconText = "G",
                        OnClickedCommand = new Command(() => {
                            ListMenuItemSelected?.Invoke(new SettingsGenericQuestionPage(), null);
                            })
                        }
                    };
                    ListMenuItems = listMenuEntries;
                }
            });



            context.WhenEntryAdded<SurveyMeasurement>().Subscribe(eventInfos =>
                {
                    var surveyMeasurements = eventInfos.Entites.Cast<SurveyMeasurement>();
                    var notUploaded = surveyMeasurements.Where(x => !x.IsUploadedToServer);
                    foreach (var entry in notUploaded)
                    {
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
                    if (!entry.IsUploadedToServer)
                    {
                        if (!_surveyIdsNotSynched.Contains(entry.Id))
                        {
                            _surveyIdsNotSynched.Add(entry.Id);
                        }
                    }
                    else
                    {
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

        private List<ListMenuItemViewModel> _listMenuItems;
        public List<ListMenuItemViewModel> ListMenuItems
        {
            get => _listMenuItems;
            set => SetProperty(ref _listMenuItems, value);
        }

        public event EventHandler ListMenuItemSelected;

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

        public ICommand Logout { protected set; get; }
        public ICommand UploadCommand { protected set; get; }

        private void HandleUploadStatusUpdate(object sender, SynchronizeDataEventArgs e)
        {
            switch (e.EventType)
            {
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
                case SyncronizeDataStates.NoInternetError:
                    DisplayIndication($"No internet", 2000);
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
