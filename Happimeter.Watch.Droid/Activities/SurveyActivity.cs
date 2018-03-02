
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Fragments;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.ViewModels;

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
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);



            ViewModel = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyQuestions();

            // Create your application here
            SetContentView(Resource.Layout.Survey);
            FindViewById<Button>(Resource.Id.surveyConfirmBurron).Click += (sender, e) => {
                var currentQuestion = ViewModel.GetCurrentQuestion();

                currentQuestion.IsAnswered = true;
                if (ViewModel.GetCurrentQuestion() == null) {

                    var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
                    //save answers to database
                    measurementService.AddSurveyMeasurement(ViewModel.GetDataModel());

                    //var intent = new Intent(this, typeof(MainActivity));
                    //StartActivity(intent);
                    Finish();
                    return;
                } 
                NavigateToNextQuestion();
            };

            // However, if we're being restored from a previous state,
            // then we don't need to do anything and should return or else
            // we could end up with overlapping fragments.
            if (savedInstanceState == null) {
                NavigateToNextQuestion();
            }

        }

        private void NavigateToNextQuestion() {
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
