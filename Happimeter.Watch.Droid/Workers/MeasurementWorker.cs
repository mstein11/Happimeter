using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Hardware;
using Android.Runtime;
using Happimeter.Core.Database;
using Happimeter.Core.ExtensionMethods;
using Happimeter.Watch.Droid.Database;

namespace Happimeter.Watch.Droid.Workers
{
    public class MeasurementWorker : AbstractWorker
    {
        public ConcurrentBag<(double, double, double)> AccelerometerMeasures = new ConcurrentBag<(double, double, double)>();
        public ConcurrentBag<double> HeartRateMeasures = new ConcurrentBag<double>();
        public ConcurrentBag<double> StepMeasures = new ConcurrentBag<double>();

        private SensorManager _sensorManager { get; set; }
        private SensorListener _accelerometerListener { get; set; }
        private SensorListener _heartRateListener { get; set; }
        private SensorListener _stepListener { get; set; }


        private MeasurementWorker()
        {
        }

        private static MeasurementWorker Instance { get; set; }

        public static MeasurementWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new MeasurementWorker();
            }
            return Instance;
        }

        public async override void Start()
        {
            _sensorManager = (SensorManager) Application.Context.GetSystemService(Android.Content.Context.SensorService);
            var sensorsList = _sensorManager.GetSensorList(SensorType.All);
            var acc = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            var heartRate = _sensorManager.GetDefaultSensor(SensorType.HeartRate);
            var stepCounter = _sensorManager.GetDefaultSensor(SensorType.StepCounter);

            _accelerometerListener = new SensorListener(this);
            _heartRateListener = new SensorListener(this);
            _stepListener = new SensorListener(this);

            _sensorManager.RegisterListener(_accelerometerListener, acc, SensorDelay.Ui);
            _sensorManager.RegisterListener(_heartRateListener, heartRate, SensorDelay.Ui);
            _sensorManager.RegisterListener(_stepListener, stepCounter, SensorDelay.Ui);
            IsRunning = true;

            while(IsRunning) {
                await Task.Delay(TimeSpan.FromSeconds(60));
                var sensorMeasurement = new SensorMeasurement
                {
                    Timestamp = DateTime.UtcNow,
                    SensorItemMeasures = new List<SensorItemMeasurement>()
                };

                var accMeasuresToSave = AccelerometerMeasures.ToList();
                AccelerometerMeasures.Clear();

                if (accMeasuresToSave.Any()) {
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = "AccelerometerX",
                        NumberOfMeasures = accMeasuresToSave.Count,
                        Average = accMeasuresToSave.Select(x => x.Item1).Average(),
                        StdDev = accMeasuresToSave.Select(x => x.Item1).StdDev(),
                        Magnitude = accMeasuresToSave.Select(x => x.Item1).Sum()
                    });    
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = "AccelerometerY",
                        NumberOfMeasures = accMeasuresToSave.Count,
                        Average = accMeasuresToSave.Select(x => x.Item2).Average(),
                        StdDev = accMeasuresToSave.Select(x => x.Item2).StdDev(),
                        Magnitude = accMeasuresToSave.Select(x => x.Item2).Sum()
                    });    
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = "AccelerometerZ",
                        NumberOfMeasures = accMeasuresToSave.Count,
                        Average = accMeasuresToSave.Select(x => x.Item3).Average(),
                        StdDev = accMeasuresToSave.Select(x => x.Item3).StdDev(),
                        Magnitude = accMeasuresToSave.Select(x => x.Item3).Sum()
                    });    
                }


                var hearRateMeasuresToSave = HeartRateMeasures.ToList();
                hearRateMeasuresToSave.Clear();

                if (hearRateMeasuresToSave.Any()) {
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = "Heartrate",
                        NumberOfMeasures = hearRateMeasuresToSave.Count(),
                        Average = hearRateMeasuresToSave.Average(),
                        StdDev = hearRateMeasuresToSave.StdDev(),
                        Magnitude = hearRateMeasuresToSave.Sum()
                    });
                }

                var stepMeasuresToSave = StepMeasures.ToList();
                StepMeasures.Clear();

                if (stepMeasuresToSave.Any())
                {
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = "Step",
                        NumberOfMeasures = stepMeasuresToSave.Count(),
                        Average = stepMeasuresToSave.Average(),
                        StdDev = stepMeasuresToSave.StdDev(),
                        Magnitude = stepMeasuresToSave.Sum()
                    });
                }

                ServiceLocator.Instance.Get<IDatabaseContext>().AddGraph(sensorMeasurement);
                Console.WriteLine("Saved new Sensormeasurement");
            }
        }

        public override void Stop()
        {
            _sensorManager.UnregisterListener(_accelerometerListener);
            _sensorManager.UnregisterListener(_heartRateListener);
            _sensorManager.UnregisterListener(_stepListener);
        }
    }

    public class SensorListener : Java.Lang.Object, ISensorEventListener
    {
        public SensorListener(MeasurementWorker worker)
        {
            _worker = worker;
        }

        private MeasurementWorker _worker { set; get; }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            
        }

        public void OnSensorChanged(SensorEvent e)
        {
            //Console.WriteLine($"Got Sensor event from sensor: {e.Sensor.Type} with values: {string.Concat(e.Values.Select(x => x.ToString()))}");
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                _worker.AccelerometerMeasures.Add((e.Values[0], e.Values[1], e.Values[2]));
            }
            else if (e.Sensor.Type == SensorType.HeartRate)
            {
                foreach (var measure in e.Values)
                {
                    _worker.HeartRateMeasures.Add(measure);
                }
            }
            else if (e.Sensor.Type == SensorType.StepCounter) {
                foreach (var measure in e.Values)
                {
                    _worker.StepMeasures.Add(measure);
                }
            }
        }
    }
}
