using System;
using Android.App;
using Android.OS;
using Android.Views;
using Happimeter.Watch.Droid.Database;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Android.Widget;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Services;
using Java.Lang;

namespace Happimeter.Watch.Droid.Activities
{
    [Activity(Label = "InfoActivity")]
    public class InfoActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);

            //Remove notification bar
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            // Create your application here
            SetContentView(Resource.Layout.Info);

            ServiceLocator.Instance.Get<IMeasurementService>()
                                      .WhenInfoScreenMeasurementUpdated()
                                      .Subscribe(UpdateInfos);
            var model = ServiceLocator.Instance.Get<IMeasurementService>()
                          .GetInfoScreenMeasurements();
            UpdateInfos(model);

            var pairing = ServiceLocator.Instance.Get<IDeviceService>().GetBluetoothPairing();
            UpdateDataExchangeTime(pairing.LastDataSync);
            ServiceLocator.Instance.Get<IDatabaseContext>()
                          .WhenEntryChanged<BluetoothPairing>().Subscribe(x =>
            {
                var pairing2 = x.Entites.Cast<BluetoothPairing>().FirstOrDefault();
                if (pairing2 == null)
                {
                    return;
                }
                UpdateDataExchangeTime(pairing2.LastDataSync);
            });

            /*var button = FindViewById<Button>(Resource.Id.info_restart_app_button);
            button.Click += (object sender, EventArgs e) =>
            {
                //var i = BaseContext.PackageManager.GetLaunchIntentForPackage(BaseContext.PackageName);
                //i.AddFlags(Android.Content.ActivityFlags.ClearTop | Android.Content.ActivityFlags.NewTask);
                //StartActivity(i);
                Runtime.GetRuntime().Exit(0);
            };
            */
            var startsOnBootDeactivated = ServiceLocator.Instance.Get<IConfigService>().GetDeactivateAppStartsOnBoot();
            var startOnBootDeactivatedButton = FindViewById<Button>(Resource.Id.info_toggle_start_on_boot_button);
            if (startsOnBootDeactivated)
            {
                startOnBootDeactivatedButton.Text = "Activate start on boot";
            }
            else
            {
                startOnBootDeactivatedButton.Text = "Deactivate start on boot";
            }
            startOnBootDeactivatedButton.Click += (object sender, EventArgs e) =>
            {
                startsOnBootDeactivated = !startsOnBootDeactivated;

                if (startsOnBootDeactivated)
                {
                    startOnBootDeactivatedButton.Text = "Activate start on boot";
                }
                else
                {
                    startOnBootDeactivatedButton.Text = "Deactivate start on boot";
                }
                ServiceLocator.Instance.Get<IConfigService>().SetDeactivateAppStartsOnBoot(startsOnBootDeactivated);
            };


            /**
             * Audio Feature extraction start/stop button
             */
            /*
            UpdateAudioFeatureExtrctionButtonText();
            var AFEToggle = FindViewById<Button>(Resource.Id.info_toggle_audio_feature_extraction);
            AFEToggle.Click += (object sender, EventArgs e) =>
            {
                var AFService = ServiceLocator.Instance.Get<IAudioFeaturesService>();
                AFService.Toggle();
                UpdateAudioFeatureExtrctionButtonText();

            };
            */

        }
        /*
        private void UpdateAudioFeatureExtrctionButtonText()
        {
            var AFEService = ServiceLocator.Instance.Get<IAudioFeaturesService>();
            var AFEActive = AFEService.IsActive;
            /*var AFEToggle = FindViewById<Button>(Resource.Id.info_toggle_audio_feature_extraction);
            if (AFEActive)
            {
                AFEToggle.Text = "Deactivate Audio Feature Extraction";
            }
            else
            {
                AFEToggle.Text = "Activate Audio Feature extraction";
            }
    }
    */
        private void UpdateInfos(InfoScreenMeasurements model)
        {
            if (model == null)
            {
                return;
            }

            RunOnUiThread(() =>
            {
                var heartRateView = FindViewById<TextView>(Resource.Id.info_heartrate_value);
                heartRateView.Text = $"{(int)model.Heartrate} bpm";

                var stepsView = FindViewById<TextView>(Resource.Id.info_steps_value);
                stepsView.Text = $"{model.Steps}";

                var closeToView = FindViewById<TextView>(Resource.Id.info_close_to_value);
                closeToView.Text = $"{model.CloseTo}";

                var timestampView = FindViewById<TextView>(Resource.Id.info_timestamp);
                timestampView.Text = $"{model.Timestamp.ToLocalTime()}";
            });
        }

        private void UpdateDataExchangeTime(DateTime? timestamp)
        {
            if (timestamp == null)
            {
                return;
            }
            var exchangeDataValue = FindViewById<TextView>(Resource.Id.info_exchange_data_value);
            exchangeDataValue.Text = $"{timestamp.Value.ToLocalTime()}";
        }
    }
}
