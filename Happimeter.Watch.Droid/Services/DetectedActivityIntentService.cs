
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;

namespace Happimeter.Watch.Droid.Services
{
    [Service(Label = "DetectedActivityIntentService")]
    [IntentFilter(new String[] { "com.yourname.DetectedActivityIntentService" })]
    public class DetectedActivityIntentService : IntentService
    {
        IBinder binder;

        public static ConcurrentDictionary<int, ConcurrentBag<int>> Measures = new ConcurrentDictionary<int, ConcurrentBag<int>>();
        protected override void OnHandleIntent(Intent intent)
        {
            var result = ActivityRecognitionResult.ExtractResult(intent);

            IList<DetectedActivity> detectedActivities = result.ProbableActivities;

            for (var i = 0; i < 9; i++)
            {
                if (i == 5 || i == 6)
                {
                    //5 is tilting and 6 shouldn't occur, we skip for those activities
                    continue;
                }
                if (!Measures.ContainsKey(i))
                {
                    var res = Measures.TryAdd(i, new ConcurrentBag<int>());
                    if (!res)
                    {
                        continue;
                    }
                }
                if (!detectedActivities.Any(x => x.Type == i))
                {
                    Measures[i].Add(0);
                }
                else
                {
                    Measures[i].Add(detectedActivities.FirstOrDefault(x => x.Type == i).Confidence);
                }
            }

            Console.WriteLine("activities detected");
            foreach (DetectedActivity da in detectedActivities)
            {
                Console.WriteLine($"activity: {da.Type} with string: {da.ToString()}: with confidence: {da.Confidence}");
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new DetectedActivityIntentServiceBinder(this);
            return binder;
        }
    }

    public class DetectedActivityIntentServiceBinder : Binder
    {
        readonly DetectedActivityIntentService service;

        public DetectedActivityIntentServiceBinder(DetectedActivityIntentService service)
        {
            this.service = service;
        }

        public DetectedActivityIntentService GetDetectedActivityIntentService()
        {
            return service;
        }
    }
}
