using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Hardware;
using Android.Locations;
using Android.Runtime;
using Android.Widget;
using Happimeter.Core.Database;
using Happimeter.Core.ExtensionMethods;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.Services;

namespace Happimeter.Watch.Droid.Workers
{
    public class MeasurementWorker : AbstractWorker
    {
        public ConcurrentBag<(double, double, double)> AccelerometerMeasures = new ConcurrentBag<(double, double, double)>();
        public ConcurrentBag<double> HeartRateMeasures = new ConcurrentBag<double>();
        public ConcurrentBag<double> StepMeasures = new ConcurrentBag<double>();
        public ConcurrentBag<double> LightMeasures = new ConcurrentBag<double>();

        public int StepsFromLastMeasurement = 0;

        private SensorManager _sensorManager { get; set; }
        private SensorListener _accelerometerListener { get; set; }
        private SensorListener _heartRateListener { get; set; }
        private SensorListener _stepListener { get; set; }
        private SensorListener _lightListener { get; set; }

        private bool _playServicesReady = false;
        FusedLocationProviderClient fusedLocationProviderClient;



        private MeasurementWorker()
        {
            _playServicesReady = IsGooglePlayServicesInstalled();
            if (_playServicesReady)
            {
                fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(BackgroundService.ServiceContext);
            }
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

        public async Task StartOnce() {
            IsRunning = true;
            StartSensors();
            //await Task.Delay(TimeSpan.FromSeconds(450));
            await Task.Delay(TimeSpan.FromSeconds(120));
            CollectMeasurements();
            StopSensors();
            IsRunning = false;
        }

        public async override void Start()
        {
            IsRunning = true;
            StartSensors();
            while(IsRunning) {
                //StartSensors();
                await Task.Delay(TimeSpan.FromSeconds(45));
                //StopSensors();

                await CollectMeasurements();


                await Task.Delay(TimeSpan.FromSeconds(45));
                Console.WriteLine("Saved new Sensormeasurement");
            }
        }

        private async Task CollectMeasurements() {
            var sensorMeasurement = new SensorMeasurement
            {
                Timestamp = DateTime.UtcNow,
                SensorItemMeasures = new List<SensorItemMeasurement>()
            };

            Location location = null;
            if (_playServicesReady) {
                location = await fusedLocationProviderClient.GetLastLocationAsync();
                if (location != null) {
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = MeasurementItemTypes.LocationLat,
                        Magnitude = location.Latitude,
                    });
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = MeasurementItemTypes.LocationLon,
                        Magnitude = location.Longitude,
                    });
                    sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                    {
                        Type = MeasurementItemTypes.LocationAlt,
                        Magnitude = location.Altitude,
                    });
                }
            } else {
                Toast.MakeText(BackgroundService.ServiceContext, $"Google Play Service not working, we don't get Locations and activities", ToastLength.Long).Show();
            }
             

            var accMeasuresToSave = AccelerometerMeasures.ToList();
            AccelerometerMeasures.Clear();

            if (accMeasuresToSave.Any())
            {
                sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                {
                    Type = MeasurementItemTypes.AccelerometerX,
                    NumberOfMeasures = accMeasuresToSave.Count,
                    Average = accMeasuresToSave.Select(x => x.Item1).Average(),
                    StdDev = accMeasuresToSave.Select(x => x.Item1).StdDev(),
                    Magnitude = accMeasuresToSave.Select(x => x.Item1).Sum()
                });
                sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                {
                    Type = MeasurementItemTypes.AccelerometerY,
                    NumberOfMeasures = accMeasuresToSave.Count,
                    Average = accMeasuresToSave.Select(x => x.Item2).Average(),
                    StdDev = accMeasuresToSave.Select(x => x.Item2).StdDev(),
                    Magnitude = accMeasuresToSave.Select(x => x.Item2).Sum()
                });
                sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                {
                    Type = MeasurementItemTypes.AccelerometerZ,
                    NumberOfMeasures = accMeasuresToSave.Count,
                    Average = accMeasuresToSave.Select(x => x.Item3).Average(),
                    StdDev = accMeasuresToSave.Select(x => x.Item3).StdDev(),
                    Magnitude = accMeasuresToSave.Select(x => x.Item3).Sum()
                });
            }


            var hearRateMeasuresToSave = HeartRateMeasures.ToList();
            HeartRateMeasures.Clear();

            if (hearRateMeasuresToSave.Any())
            {
                sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                {
                    Type = MeasurementItemTypes.HeartRate,
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
                    Type = MeasurementItemTypes.Step,
                    NumberOfMeasures = stepMeasuresToSave.Count(),
                    Average = stepMeasuresToSave.Average(),
                    StdDev = stepMeasuresToSave.StdDev(),
                    Magnitude = stepMeasuresToSave.Sum()
                });
            }

            var lightMeasuresToSave = LightMeasures.ToList();
            LightMeasures.Clear();
            if (lightMeasuresToSave.Any())
            {
                sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                {
                    Type = MeasurementItemTypes.Light,
                    NumberOfMeasures = lightMeasuresToSave.Count(),
                    Average = lightMeasuresToSave.Average(),
                    StdDev = lightMeasuresToSave.StdDev(),
                    Magnitude = lightMeasuresToSave.Sum()
                });
            }

            var microphoneMeasures = MicrophoneWorker.GetInstance().MicrophoneMeasures.ToList();
            MicrophoneWorker.GetInstance().MicrophoneMeasures.Clear();

            if (microphoneMeasures.Any())
            {
                sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
                {
                    Type = MeasurementItemTypes.Microphone,
                    NumberOfMeasures = microphoneMeasures.Count(),
                    Average = microphoneMeasures.Average(),
                    StdDev = microphoneMeasures.StdDev(),
                    Magnitude = microphoneMeasures.Sum()
                });
            }

            sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
            {
                Type = MeasurementItemTypes.Vmc,
                Magnitude = -1,
            });

            sensorMeasurement.SensorItemMeasures.Add(new SensorItemMeasurement
            {
                Type = MeasurementItemTypes.ActivityUnspecific,
                Magnitude = 1,
            });


            ServiceLocator.Instance.Get<IDatabaseContext>().AddGraph(sensorMeasurement);
        }

        public override void Stop()
        {
            IsRunning = false;
            StopSensors();
        }

        private void StopSensors() {
            if (_accelerometerListener != null)
            {
                _sensorManager.UnregisterListener(_accelerometerListener);
            }
            if (_heartRateListener != null)
            {
                _sensorManager.UnregisterListener(_heartRateListener);
            }
            if (_stepListener != null)
            {
                _sensorManager.UnregisterListener(_stepListener);
            }
            if (_lightListener != null)
            {
                _sensorManager.UnregisterListener(_lightListener);
            }
        }

        private void StartSensors() {
            _sensorManager = (SensorManager)Application.Context.GetSystemService(Android.Content.Context.SensorService);

            var light = _sensorManager.GetDefaultSensor(SensorType.Light);
            if (light != null)
            {
                _lightListener = new SensorListener(this);
                _sensorManager.RegisterListener(_lightListener, light, SensorDelay.Ui);
            }

            var acc = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            if (acc != null)
            {
                _accelerometerListener = new SensorListener(this);
                _sensorManager.RegisterListener(_accelerometerListener, acc, SensorDelay.Ui);
            }

            var heartRate = _sensorManager.GetDefaultSensor(SensorType.HeartRate);
            if (heartRate != null)
            {
                _heartRateListener = new SensorListener(this);
                _sensorManager.RegisterListener(_heartRateListener, heartRate, SensorDelay.Ui);
            }

            var stepCounter = _sensorManager.GetDefaultSensor(SensorType.StepCounter);
            if (stepCounter != null)
            {
                _stepListener = new SensorListener(this);
                _sensorManager.RegisterListener(_stepListener, stepCounter, SensorDelay.Ui);
            }
        }

        bool IsGooglePlayServicesInstalled()
        {
            var queryResult = Android.Gms.Common.GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(BackgroundService.ServiceContext);
            if (queryResult == Android.Gms.Common.ConnectionResult.Success)
            {
                Console.WriteLine("Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                // Check if there is a way the user can resolve the issue
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Console.WriteLine($"There is a problem with Google Play Services on this device: {queryResult} - {errorString}");
                Toast.MakeText(BackgroundService.ServiceContext, $"There is a problem with Google Play Services on this device: {queryResult} - {errorString}", ToastLength.Long).Show();
                // Alternately, display the error to the user.
            }

            return false;
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
                    if (_worker.StepsFromLastMeasurement == 0) {
                        _worker.StepsFromLastMeasurement = (int) measure;
                    } else {
                        _worker.StepMeasures.Add(measure - _worker.StepsFromLastMeasurement);    
                        _worker.StepsFromLastMeasurement = (int)measure;
                    }
                }
            }
            else if (e.Sensor.Type == SensorType.Light)
            {
                foreach (var measure in e.Values)
                {
                    _worker.LightMeasures.Add(measure);
                }
            }
        }
    }
}
