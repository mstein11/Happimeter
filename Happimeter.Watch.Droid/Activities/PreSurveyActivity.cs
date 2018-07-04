
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
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
	[Activity(Label = "PreSurveyActivity")]
	public class PreSurveyActivity : Activity
	{
		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			RequestWindowFeature(WindowFeatures.NoTitle);

			//Remove notification bar
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
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


			//if we don't get predictions          
			StartActivity(typeof(SurveyActivity));
			Toast.MakeText(ApplicationContext, "Prediction could not be loaded", ToastLength.Long);
			Finish();
		}
	}
}
