using Android.App;
using Android.Widget;
//using Android.OS;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.OS;
using Happimeter.Watch.Droid.Services;
using Happimeter.Watch.Droid.Database;
using Happimeter.Core.Database;
using Happimeter.Watch.Droid.Activities;
using System;
using System.Linq;
using Android.Views;
using Happimeter.Watch.Droid.DependencyInjection;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Happimeter.Watch.Droid.Workers;
using Happimeter.Watch.Droid.openSMILE;

namespace Happimeter.Watch.Droid
{
    [Activity(Label = "Happimeter.Watch.Droid", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Container.RegisterElements();
            AppCenter.Start("3c4f7090-2cb6-4eed-abc5-ac5d0aa63e45",
                typeof(Analytics), typeof(Crashes));

            RequestWindowFeature(WindowFeatures.NoTitle);
            //Remove notification bar
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var textbox = FindViewById<TextView>(Resource.Id.main_info_subtext_paired_status);
            var btWorker = BluetoothWorker.GetInstance();
            if (btWorker == null || btWorker.SubscribedDevices.Count == 0)
            {
                textbox.Text = "Status: Not in Range";
            }
            else
            {
                textbox.Text = "Status: Connected";
            }
            btWorker.WhenSubscripbedDevicesChanges.Subscribe(x =>
            {
                RunOnUiThread(() =>
                {
                    if (x.Count == 0)
                    {
                        textbox.Text = "Status: Not in Range";
                    }
                    else
                    {
                        textbox.Text = "Status: Connected";
                    }
                });
            });
            // activates audio feature extraction in this session if user activated it in a previous session
            //ServiceLocator.Instance.Get<IAudioFeaturesService>().OnApplicationStartup();
            Smile.Initialize();
            var pairing = ServiceLocator.Instance.Get<IDatabaseContext>().GetCurrentBluetoothPairing();
            if (pairing != null)
            {

            }
            else
            {
                var intent = new Intent(this, typeof(PairingActivity));
                StartActivity(intent);
                Finish();
            }

            FindViewById<Button>(Resource.Id.main_info_button).Click += delegate
            {
                var intent = new Intent(this, typeof(InfoActivity));
                StartActivity(intent);
            };

            FindViewById<Button>(Resource.Id.removePairingButton).Click += delegate
            {
                var dialog = new AlertDialog.Builder(this)
                               .SetTitle("Remove Pairing")
                               .SetMessage("Are you sure that you want to remove the pairing?")
                               .SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
                               {
                                   ServiceLocator.Instance.Get<IDeviceService>().RemovePairing();
                                   var intent = new Intent(this, typeof(PairingActivity));
                                   StartActivity(intent);
                                   Finish();
                               })
                               .SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
                               {

                               });
                dialog.Show();
            };
            FindViewById<Button>(Resource.Id.surveyButton).Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(PreSurveyActivity));
                StartActivity(intent);
            };

            if (!IsMyServiceRunning(typeof(BackgroundService)))
            {
                StartService(new Intent(this, typeof(BackgroundService)));
            }
            if (!IsMyServiceRunning(typeof(BeaconService)))
            {
                StartService(new Intent(this, typeof(BeaconService)));
            }
        }

        private bool IsMyServiceRunning(Type serviceClass)
        {
            var manager = (ActivityManager)GetSystemService(Context.ActivityService);
            foreach (var service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.GetType() == serviceClass)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

