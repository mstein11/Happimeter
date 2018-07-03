
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Fragments;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.ViewModels;
using Happimeter.Watch.Droid.Workers;
using Happimeter.Core.Models.Bluetooth;
using System.Linq;

namespace Happimeter.Watch.Droid.Activities
{
	[Activity(Label = "SurveyActivity")]
	public class SurveyActivity : Activity
	{
		public SurveyViewModel ViewModel { get; set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			//Remove title bar
			RequestWindowFeature(WindowFeatures.NoTitle);

			//Remove notification bar
			Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn | WindowManagerFlags.KeepScreenOn);

			int? activation = null;
			int? pleasance = null;
			if (Intent.HasExtra("pleasance"))
			{
				pleasance = Intent.GetIntExtra("pleasance", 0);
			}
			if (Intent.HasExtra("activation"))
			{
				activation = Intent.GetIntExtra("activation", 0);
			}


			ViewModel = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyQuestions(pleasance, activation);

			// Create your application here
			SetContentView(Resource.Layout.Survey);
			FindViewById<Button>(Resource.Id.surveyConfirmBurron).Click += (sender, e) =>
			{
				var currentQuestion = ViewModel.GetCurrentQuestion();
				if (currentQuestion != null)
				{
					currentQuestion.IsAnswered = true;
				}

				if (currentQuestion == null || ViewModel.GetCurrentQuestion() == null)
				{
					FinishSurvey();
					return;
				}
				NavigateToNextQuestion();
			};

			// However, if we're being restored from a previous state,
			// then we don't need to do anything and should return or else
			// we could end up with overlapping fragments.
			if (savedInstanceState == null)
			{
				if (ViewModel.Questions.All(x => x.IsAnswered))
				{
					FinishSurvey();
					return;
				}
				NavigateToNextQuestion();
			}

		}

		private void FinishSurvey()
		{
			var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
			//save answers to database
			measurementService.AddSurveyMeasurement(ViewModel.GetDataModel());

			//var intent = new Intent(this, typeof(MainActivity));
			//StartActivity(intent);
			Finish();
		}

		private void NavigateToNextQuestion()
		{
			if (FindViewById(Resource.Id.fragment_container) != null)
			{
				// Create a new Fragment to be placed in the activity layout
				var fragment = SurveyItemFragment.NewInstance(ViewModel.GetCurrentQuestionPosition());

				// In case this activity was started with special instructions from an
				// Intent, pass the Intent's extras to the fragment as arguments
				//firstFragment.setArguments(getIntent().getExtras());

				var transaction = FragmentManager.BeginTransaction();
				transaction.Replace(Resource.Id.fragment_container, fragment);
				transaction.Commit();
			}
		}
	}
}
