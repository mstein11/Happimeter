using System;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Events;
using Happimeter.Interfaces;
using System.Collections.ObjectModel;
using Happimeter.Services;

namespace Happimeter.ViewModels.Forms
{
    public class SettingsGenericQuestionPageViewModel : BaseViewModel
    {
        public SettingsGenericQuestionPageViewModel()
        {

            var hasBtPairing = ServiceLocator.Instance.Get<ISharedDatabaseContext>().Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive) != null;
            ShowPushQuestionsToWatchButton = hasBtPairing;

            NumberOfGenericQuestions = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyQuestions().SurveyItems.Count() - 2;
            if (NumberOfGenericQuestions < 0)
            {
                NumberOfGenericQuestions = 0;
            }
            SaveGenericGroupButtonEnabled = true;

            PushGenericQuestionToWatchButtonText = "Push Questions to Watch";
            PushQuestionsToWatchButtonEnabled = true;
            PushGenericQuestionsToWatchCommand = new Command(() =>
            {
                App.BluetoothAlertIfNeeded();
                PushQuestionsToWatchButtonEnabled = false;
                PushGenericQuestionToWatchButtonText = "Loading...";
                ServiceLocator.Instance.Get<IBluetoothService>().SendGenericQuestions((connectionUpdate) =>
                {
                    Timer timer = null;
                    switch (connectionUpdate)
                    {
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

            DownloadGenericQuestions = new Command(async () =>
            {
                SaveGenericGroupButtonEnabled = false;
                GenericGroupButtonText = "Loading...";
                var questions = await ServiceLocator
                    .Instance
                    .Get<IGenericQuestionService>()
                    .DownloadAndSaveGenericQuestions();
                Timer timer = null;
                if (questions == null)
                {
                    //Error while downloading questions
                    GenericGroupButtonText = "Error Downloading Questinos";
                }
                else
                {
                    NumberOfGenericQuestions = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyQuestions().SurveyItems.Count() - 2;
                    GenericGroupButtonText = $"Successfully downloaded  {questions.Count - 2} questions";
                }

                timer = new Timer((obj) =>
                {
                    SaveGenericGroupButtonEnabled = true;
                    GenericGroupButtonText = "Download Questions";
                    timer.Dispose();
                }, null, 2000, System.Threading.Timeout.Infinite);
            });

            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            context.WhenEntryChanged<SharedBluetoothDevicePairing>().Subscribe(eventInfo =>
            {
                if (eventInfo.Entites.Cast<SharedBluetoothDevicePairing>().Any(x => x.IsPairingActive)
            && eventInfo.TypeOfEvent != Core.Events.DatabaseChangedEventTypes.Deleted
            && eventInfo.TypeOfEvent != Core.Events.DatabaseChangedEventTypes.DeleteAll)
                {
                    ShowPushQuestionsToWatchButton = true;
                }
                else
                {
                    ShowPushQuestionsToWatchButton = false;
                }
            });

            var genericQuestions = ServiceLocator.Instance.Get<IGenericQuestionService>().GetGenericQuestions();
            GenericQuestions = new ObservableCollection<GenericQuestionViewModel>(genericQuestions.Select(x =>
            {
                return new GenericQuestionViewModel
                {
                    Id = x.Id,
                    IsActivated = x.Activated,
                    Name = x.QuestionShort
                };
            }).ToList());


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
        private string _genericGroupButtonText = "Download Questions";
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

        public ICommand DownloadGenericQuestions { protected set; get; }
        public ICommand PushGenericQuestionsToWatchCommand { protected set; get; }

        public ObservableCollection<GenericQuestionViewModel> GenericQuestions { get; set; }
    }

    public class GenericQuestionViewModel : BaseViewModel
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        private bool _isActivated;
        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                SetProperty(ref _isActivated, value);
                HandleActivatedChanged(value);
            }
        }

        private void HandleActivatedChanged(bool IsActivatedInner)
        {
            //ServiceLocator.Instance.Get<IMeasurementService>().ToggleGenericQuestionActivation(Id, IsActivatedInner);
        }
    }
}
