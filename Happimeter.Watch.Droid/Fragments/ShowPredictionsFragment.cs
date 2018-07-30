
using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Happimeter.Watch.Droid.Helper;
using Happimeter.Watch.Droid.Activities;

namespace Happimeter.Watch.Droid.Fragments
{
    public class ShowPredictionsFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override void OnStart()
        {
            base.OnStart();
            var pleasance = Arguments.GetInt("pleasance");
            var activation = Arguments.GetInt("activation");
            SetTextForPredictions(activation, pleasance);
            UiHelper.SetMoodImage(View.FindViewById<ImageView>(Resource.Id.moodImageView), pleasance, activation);

            var wrongButton = View.FindViewById<ImageButton>(Resource.Id.prediction_wrong);
            wrongButton.Click += (object sender, EventArgs e) =>
            {
                Activity.Finish();
                var intent = new Intent(Application.Context, typeof(SurveyActivity));
                intent.AddFlags(ActivityFlags.ReorderToFront);
                StartActivity(intent);
            };
            var rightButton = View.FindViewById<ImageButton>(Resource.Id.prediction_right);
            rightButton.Click += (object sender, EventArgs e) =>
            {
                var intent = new Intent(Application.Context, typeof(SurveyActivity));
                intent.AddFlags(ActivityFlags.ReorderToFront);
                intent.PutExtra("activation", activation);
                intent.PutExtra("pleasance", pleasance);
                Activity.Finish();
                StartActivity(intent);
            };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.ShowPredictions, container, false);
            return view;
        }

        private void SetTextForPredictions(int activation, int pleasance)
        {
            string text = "";
            if (pleasance == 0 && activation == 0)
            {
                text = "UNHAPPY (1) - INACTIVE (1)";
            }
            else if (pleasance == 0 && activation == 1)
            {
                text = "UNHAPPY (1) - NEUTRAL (2)";
            }
            else if (pleasance == 0 && activation == 2)
            {
                text = "UNHAPPY (1) - ACTIVE (3)";
            }
            else if (pleasance == 1 && activation == 0)
            {
                text = "NEUTRAL (2) - INACTIVE (1)";
            }
            else if (pleasance == 1 && activation == 1)
            {
                text = "NEUTRAL (2) - NEUTRAL (2)";
            }
            else if (pleasance == 1 && activation == 2)
            {
                text = "NEUTRAL (2) - ACTIVE (3)";
            }
            else if (pleasance == 2 && activation == 0)
            {
                text = "HAPPY (3) - INACTIVE (1)";
            }
            else if (pleasance == 2 && activation == 1)
            {
                text = "HAPPY (3) - NEUTRAL (2)";
            }
            else if (pleasance == 2 && activation == 2)
            {
                text = "HAPPY (3) - ACTIVE (3)";
            }
            View.FindViewById<TextView>(Resource.Id.predictions_text).Text = text;
        }
    }
}
