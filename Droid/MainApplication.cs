using System;

using Android.App;
using Android.OS;
using Android.Runtime;
using Happimeter.DependencyInjection;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.CurrentActivity;
using Plugin.BluetoothLE;
using Xfx;
using AltBeaconOrg.Bluetooth;
using FFImageLoading.Svg.Forms;
using FFImageLoading.Forms.Droid;
using Java.Security;
using Plugin.FirebasePushNotification;
using Happimeter.Core.Helper;
using Org.W3c.Dom;
using Happimeter.Services;

namespace Happimeter.Droid
{
    //You can specify additional application information in this attribute
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        private bool _isInitialized = false;
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
        : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            AppCenter.Start("a5a4fc8f-8bd6-4b43-a1a9-241453a38c5c",
                   typeof(Analytics), typeof(Crashes));
            RegisterActivityLifecycleCallbacks(this);


            Droid.DependencyInjection.Container.RegisterElements();
            BluetoothMedic.Instance.EnablePowerCycleOnFailures(this);
            BluetoothMedic.Instance.SetNotificationsEnabled(true, Resource.Drawable.notification_bg);
            BluetoothMedic.Instance.EnablePeriodicTests(this, BluetoothMedic.ScanTest | BluetoothMedic.TransmitTest);

        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
            if (!_isInitialized)
            {
                XfxControls.Init();
                Xamarin.Forms.Forms.Init(this, savedInstanceState);
                SetupNotifications();
                var ignore = typeof(SvgCachedImage);
                FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
                App.Initialize();
                ServiceLocator.Instance.Get<INotificationService>()
                          .SubscibeToChannel(Happimeter.Services.NotificationService.NotificationChannelAllDevices);
                _isInitialized = true;
            }
        }

        public void OnActivityDestroyed(Activity activity)
        {

        }

        public void OnActivityPaused(Activity activity)
        {

        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
            App.AppResumed();
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {

        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }

        private void SetupNotifications()
        {
            //Set the default notification channel for your app when running Android Oreo
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                //Change for your default notification channel id here
                FirebasePushNotificationManager.DefaultNotificationChannelId = "FirebasePushNotificationChannel";

                //Change for your default notification channel name here
                FirebasePushNotificationManager.DefaultNotificationChannelName = "General";
            }

            //If debug you should reset the token each time.
#if DEBUG
            FirebasePushNotificationManager.Initialize(this, true);
#else
            FirebasePushNotificationManager.Initialize(this,false);
#endif
        }

    }
}
