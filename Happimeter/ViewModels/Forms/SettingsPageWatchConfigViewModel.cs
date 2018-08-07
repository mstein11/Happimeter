using System.Threading;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Happimeter.Events;
using Happimeter.Interfaces;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Happimeter.ViewModels.Forms
{
    public class SettingsPageWatchConfigViewModel : BaseViewModel
    {
        public SettingsPageWatchConfigViewModel()
        {
            var configService = ServiceLocator.Instance.Get<IConfigService>();
            ContinousModeIsOn = configService.IsContinousMeasurementMode();

            MeasurementModes = MeasurementMode.GetModes();
            //SelectedMode = MeasurementModes.LastOrDefault();
            PushMeasurementModeToWatchText = "Safe and Push To Watch";
            PushMeasurementModeToWatchIsEnabled = true;
            PushMeasurementModeToWatchCommand = new Command(() =>
            {
                if (SelectedMode == null)
                {
                    Application.Current.MainPage.DisplayAlert("Error", "Please select a mode", "Ok");
                    return;
                }
                App.BluetoothAlertIfNeeded();
                PushMeasurementModeToWatchText = "Loading...";
                PushMeasurementModeToWatchIsEnabled = false;

                int valueToSend = SelectedMode.Id;
                ServiceLocator.Instance.Get<IBluetoothService>().SendMeasurementMode(valueToSend, (connectionUpdate) =>
                {
                    Timer timer = null;
                    switch (connectionUpdate)
                    {
                        case BluetoothWriteEvent.Initialized:
                            break;
                        case BluetoothWriteEvent.Connected:
                            PushMeasurementModeToWatchText = "Loading... (connected to device)";
                            break;

                        case BluetoothWriteEvent.Complete:
                            PushMeasurementModeToWatchText = "Successfully changed mode";
                            timer = null;

                            ServiceLocator.Instance.Get<IConfigService>().SetMeasurementMode(valueToSend);


                            timer = new Timer((obj) =>
                            {
                                PushMeasurementModeToWatchText = "Safe and Push To Watch";
                                PushMeasurementModeToWatchIsEnabled = true;
                                timer.Dispose();
                            }, null, 2000, System.Threading.Timeout.Infinite);
                            break;
                        case BluetoothWriteEvent.ErrorOnConnectingToDevice:
                            PushMeasurementModeToWatchText = "Error!";
                            timer = null;
                            timer = new Timer((obj) =>
                            {
                                PushMeasurementModeToWatchText = "Push Questions to Watch";
                                PushMeasurementModeToWatchIsEnabled = true;
                                if (timer != null)
                                {
                                    timer.Dispose();
                                }
                            }, null, 2000, System.Threading.Timeout.Infinite);
                            break;
                        case BluetoothWriteEvent.ErrorOnWrite:
                            PushMeasurementModeToWatchText = "Error!";
                            timer = null;
                            timer = new Timer((obj) =>
                            {
                                PushMeasurementModeToWatchText = "Push Questions to Watch";
                                PushMeasurementModeToWatchIsEnabled = true;
                                if (timer != null)
                                {
                                    timer.Dispose();
                                }
                            }, null, 2000, System.Threading.Timeout.Infinite);
                            break;
                    }
                });

            });
        }

        private bool _continousModeIsOn;
        public bool ContinousModeIsOn
        {
            get => _continousModeIsOn;
            set => SetProperty(ref _continousModeIsOn, value);
        }

        private string _pushMeasurementModeToWatchText;
        public string PushMeasurementModeToWatchText
        {
            get => _pushMeasurementModeToWatchText;
            set => SetProperty(ref _pushMeasurementModeToWatchText, value);
        }

        private bool _pushMeasurementModeToWatchIsEnabled;
        public bool PushMeasurementModeToWatchIsEnabled
        {
            get => _pushMeasurementModeToWatchIsEnabled;
            set => SetProperty(ref _pushMeasurementModeToWatchIsEnabled, value);
        }

        private IList<MeasurementMode> _measurementModes = new List<MeasurementMode>();
        public IList<MeasurementMode> MeasurementModes
        {
            get => _measurementModes;
            set => SetProperty(ref _measurementModes, value);
        }

        private MeasurementMode _selectedMode;
        public MeasurementMode SelectedMode
        {
            get => _selectedMode;
            set => SetProperty(ref _selectedMode, value);
        }

        public System.Windows.Input.ICommand PushMeasurementModeToWatchCommand { protected set; get; }
    }

    public class MeasurementMode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? IntervalSeconds { get; set; }
        public int? FactorMeasurementOfInterval { get; set; }
        public bool IsSelected { get; set; }

        public static IList<MeasurementMode> GetModes()
        {
            return new List<MeasurementMode> {
                new MeasurementMode {
                    Id = 1,
                    Name = "Super Battery Safer",
                    IntervalSeconds = 1800, //30 minutes
                    FactorMeasurementOfInterval = 60,
                    Description = "Every 30 Minutes the sensors run for 30 seconds."
                },
                new MeasurementMode {
                    Id = 2,
                    Name = "Battery Safer",
                    IntervalSeconds = 900, //15 minutes
                    FactorMeasurementOfInterval = 30,
                    Description = "Every 15 Minutes the sensors run for 30 seconds."
                },
                new MeasurementMode {
                    Id = 3,
                    Name = "Normal Mode",
                    IntervalSeconds = 300, // 5 minutes
                    FactorMeasurementOfInterval = 10,
                    Description = "Every 5 Minutes the sensors run for 30 seconds."
                },
                new MeasurementMode {
                    Id = 4,
                    Name = "Continuous mode",
                    IntervalSeconds = null, // every minute with continous measurements
                    FactorMeasurementOfInterval = null,
                    Description = "The sensors run continuously and a aggregated measurement is saved every minute"
                }
            };
        }
    }
}
