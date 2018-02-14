
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Happimeter.Watch.Droid.Activities
{
    [Activity(Label = "FinishSurveyActivity")]
    public class FinishSurveyActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            Task.Delay(TimeSpan.FromSeconds(1));
            //Finish();
            // Create your application here
        }
    }
}
