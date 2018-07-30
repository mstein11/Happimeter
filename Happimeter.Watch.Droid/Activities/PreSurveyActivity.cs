
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Workers;
using System.Threading.Tasks;
using Happimeter.Watch.Droid.Fragments;
using Happimeter.Watch.Droid.ServicesBusinessLogic;

namespace Happimeter.Watch.Droid.Activities
{
    [Activity(Label = "PreSurveyActivity", LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]

    public class PreSurveyActivity : Activity
    {
        private bool IsAlarmTriggered = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Remove notification bar
            Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn | WindowManagerFlags.KeepScreenOn);

            if (Intent.HasExtra("IsAlarmTriggered"))
            {
                IsAlarmTriggered = Intent.GetBooleanExtra("IsAlarmTriggered", false);
            }
            SetupView();
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (Intent.HasExtra("IsAlarmTriggered"))
            {
                IsAlarmTriggered = Intent.GetBooleanExtra("IsAlarmTriggered", false);
            }
            else
            {
                IsAlarmTriggered = false;
            }

            SetupView();
            base.OnNewIntent(intent);
        }

        private void SetupView()
        {
            SetContentView(Resource.Layout.PreSurveyFragmentContainer);
            var predictionsFragment = new LoadingPredictionsFragment();
            var transaction = FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragment_container, predictionsFragment);
            transaction.Commit();
            GetPredictions();
        }

        private async void GetPredictions()
        {
            var task = BluetoothWorker.GetInstance().SendNotificationAwaitResponse(UuidHelper.PreSurveyDataCharacteristicUuid, new PreSurveyFirstMessage());
            var timeout = Task.Delay(10000);

            if (await Task.WhenAny(task, timeout) == task)
            {
                var res = await task as PreSurveySecondMessage;

                if (res != null)
                {
                    if (res.Questions != null)
                    {
                        ServiceLocator.Instance.Get<IMeasurementService>().AddGenericQuestions(res.Questions);
                    }
                    if (IsAlarmTriggered)
                    {
                        var vibrator = (Vibrator)ApplicationContext.GetSystemService(Context.VibratorService);
                        //vibrate for 500 milis
                        vibrator.Vibrate(500);
                    }

                    var bundle = new Bundle();
                    bundle.PutInt("activation", res.PredictedActivation);
                    bundle.PutInt("pleasance", res.PredictedPleasance);

                    var predictionsFragment = new ShowPredictionsFragment();
                    predictionsFragment.Arguments = bundle;

                    var transaction = FragmentManager.BeginTransaction();
                    transaction.Replace(Resource.Id.fragment_container, predictionsFragment);
                    transaction.CommitAllowingStateLoss();

                    return;
                }
            }

            if (IsAlarmTriggered)
            {
                var vibrator = (Vibrator)ApplicationContext.GetSystemService(Context.VibratorService);
                //vibrate for 500 milis
                vibrator.Vibrate(500);
            }

            //if we don't get predictions     
            var intent = new Intent(Application.Context, typeof(SurveyActivity));
            intent.AddFlags(ActivityFlags.ReorderToFront);
            StartActivity(intent);
            Toast.MakeText(ApplicationContext, "Prediction could not be loaded", ToastLength.Long);
            Finish();
        }
    }
}
