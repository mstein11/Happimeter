
using System;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.Services
{
    [Service(Label = "MeasurementJobService", Permission = "android.permission.BIND_JOB_SERVICE")]
    public class MeasurementJobService : JobService
    {
        IBinder binder;

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            // start your service logic here

            // Return the correct StartCommandResult for the type of service you are building
            return StartCommandResult.NotSticky;
        }


        public override bool OnStartJob(JobParameters @params)
        {
            System.Diagnostics.Debug.WriteLine($"Job started at: {DateTime.UtcNow}");
            var task1 = Task.Factory.StartNew(async () => {
                System.Diagnostics.Debug.WriteLine($"Measurement started at: {DateTime.UtcNow}");
                await MeasurementWorker.GetInstance().StartOnce();
                System.Diagnostics.Debug.WriteLine($"Measurement ended at: {DateTime.UtcNow}");
            });

            var task2 = Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Micro started at: {DateTime.UtcNow}");
                MicrophoneWorker.GetInstance().StartOnce();
                System.Diagnostics.Debug.WriteLine($"Micro ended at: {DateTime.UtcNow}");
            });

            Task.WhenAll(task1,task2).ContinueWith((Task) => {
                System.Diagnostics.Debug.WriteLine($"job finished at: {DateTime.UtcNow}");
                JobFinished(@params, false);
            });

            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            MeasurementWorker.GetInstance().Stop();
            return true;
        }
    }

    public class MeasurementJobServiceBinder : Binder
    {
        readonly MeasurementJobService service;

        public MeasurementJobServiceBinder(MeasurementJobService service)
        {
            this.service = service;
        }

        public MeasurementJobService GetMeasurementJobService()
        {
            return service;
        }
    }
}
