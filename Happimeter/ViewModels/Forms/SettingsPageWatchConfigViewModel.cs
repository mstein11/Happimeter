using System;
using System.Threading;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Happimeter.Events;
using Happimeter.Interfaces;
using Happimeter.Services;

namespace Happimeter.ViewModels.Forms
{
    public class SettingsPageWatchConfigViewModel : BaseViewModel
    {
        public SettingsPageWatchConfigViewModel()
        {
            var configService = ServiceLocator.Instance.Get<IConfigService>();
            ContinousModeIsOn = configService.IsContinousMeasurementMode();

            PushMeasurementModeToWatchText = "Safe and Push To Watch";
            PushMeasurementModeToWatchIsEnabled = true;
            PushMeasurementModeToWatchCommand = new Command(() =>
            {
                App.BluetoothAlertIfNeeded();
                PushMeasurementModeToWatchText = "Loading...";
                PushMeasurementModeToWatchIsEnabled = false;

                int? valueToSend = ContinousModeIsOn ? null : (int?)UtilHelper.SecondsBatterySaverMeasurementPeriod;

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
                            if (valueToSend != null)
                            {
                                ServiceLocator.Instance.Get<IConfigService>().SetBatterySaferMeasurementMode(valueToSend.Value);
                            }
                            else
                            {
                                ServiceLocator.Instance.Get<IConfigService>().SetContinousMeasurementMode();
                            }

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

        public System.Windows.Input.ICommand PushMeasurementModeToWatchCommand { protected set; get; }
    }
}
