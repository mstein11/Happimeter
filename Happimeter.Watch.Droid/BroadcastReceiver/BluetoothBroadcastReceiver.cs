using System;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Happimeter.Watch.Droid.Workers;
using Java.Security;
using System.Threading;

namespace Happimeter.Watch.Droid
{
	[BroadcastReceiver(Enabled = true)]
	[IntentFilter(new[] { Android.Bluetooth.BluetoothAdapter.ActionStateChanged }, Priority = (int)IntentFilterPriority.HighPriority)]
	public class BluetoothBroadcastReceiver : Android.Content.BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			var state = intent.GetIntExtra(Android.Bluetooth.BluetoothAdapter.ExtraState, -1);

			switch (state)
			{
				case (int)State.TurningOff:
					//disconnect greacefully
					if (BeaconWorker.GetInstance().IsRunning)
					{
						//BeaconWorker.GetInstance().Stop();
					}
					if (BluetoothWorker.GetInstance().IsRunning)
					{
						//BluetoothWorker.GetInstance().Stop();
					}
					AfterBluetoothShutDown();
					break;
				case (int)State.On:
					if (!BeaconWorker.GetInstance().IsRunning)
					{
						BeaconWorker.GetInstance().Start();
					}
					if (!BluetoothWorker.GetInstance().IsRunning)
					{
						BluetoothWorker.GetInstance().Start();
					}
					break;
					//restart worker


			}
		}

		private void AfterBluetoothShutDown()
		{
			Timer timer = null;
			timer = new Timer((obj) =>
			{
				BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
				if (!mBluetoothAdapter.IsEnabled)
				{
					mBluetoothAdapter.Enable();
				}
				timer.Dispose();
			}, null, 10000, System.Threading.Timeout.Infinite);


		}
	}
}
