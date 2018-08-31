using System;
using Android.App;
using AltBeaconOrg.Bluetooth;
using Android.Bluetooth;
namespace Happimeter.Watch.Droid
{
    [Application]
    public class MainApplication : Application
    {
        public MainApplication(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (!mBluetoothAdapter.IsEnabled)
            {
                mBluetoothAdapter.Enable();
            }

            BluetoothMedic.Instance.EnablePeriodicTests(this, BluetoothMedic.ScanTest | BluetoothMedic.TransmitTest);
            BluetoothMedic.Instance.EnablePowerCycleOnFailures(this);             BluetoothMedic.Instance.SetNotificationsEnabled(true, Resource.Drawable.notification_icon_background);
        }
    }
}
