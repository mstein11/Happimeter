
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.Activities
{
	[Activity(Label = "PairingRequestActivity")]
	public class PairingRequestActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);

			//Remove notification bar
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
			// Create your application here
			SetContentView(Resource.Layout.PairingRequest);

			var vibrator = (Vibrator)GetSystemService(Context.VibratorService);
			//vibrate for 500 milis
			vibrator.Vibrate(500);
			var deviceName = Intent.GetStringExtra("DeviceName");

			var textView = FindViewById<TextView>(Resource.Id.PairingRequestDeviceName);
			textView.Text = deviceName;

			var acceptButton = FindViewById<Button>(Resource.Id.PairingRequestAccept);
			var declineButton = FindViewById<Button>(Resource.Id.PairingRequestDecline);
			var loading = FindViewById<ProgressBar>(Resource.Id.PairingRequestLoading);

			acceptButton.Click += (sender, e) =>
			{
				acceptButton.Visibility = ViewStates.Gone;
				declineButton.Visibility = ViewStates.Gone;
				loading.Visibility = ViewStates.Visible;
				BluetoothWorker.GetInstance().SendNotification(UuidHelper.AuthCharacteristicUuid, new AuthNotificationMessage(true));
				Timer timer = null;
				timer = new Timer((obj) =>
				{
					if (!IsFinishing)
					{
						//if after 15 seconds this activity is still not finished or in the process of being finished there was an error with the auth process
						Finish();
					}

					if (timer != null)
					{
						timer.Dispose();
					}
				}, null, 15000, System.Threading.Timeout.Infinite);

			};
			declineButton.Click += (sender, e) =>
			{
				acceptButton.Visibility = ViewStates.Gone;
				declineButton.Visibility = ViewStates.Gone;
				loading.Visibility = ViewStates.Visible;
				BluetoothWorker.GetInstance().SendNotification(UuidHelper.AuthCharacteristicUuid, new AuthNotificationMessage(false));
				Finish();
			};
			var db = ServiceLocator.Instance.Get<IDatabaseContext>();
			db.WhenEntryChanged<BluetoothPairing>().Take(1).Subscribe(eventInfo =>
			{
				if (eventInfo.Entites.Cast<BluetoothPairing>().Any(x => x.IsPairingActive))
				{
					//finish up the activity when pairing is created
					Finish();
				}
			});
		}

		public override void OnBackPressed()
		{
			//we do nothing here because we don't want the backbutton to have an action, do not delete method.
			//base.OnBackPressed();
		}
	}
}
