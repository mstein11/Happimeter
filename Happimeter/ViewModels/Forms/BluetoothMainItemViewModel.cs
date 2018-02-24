using System;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;

namespace Happimeter.ViewModels.Forms
{
    public class BluetoothMainItemViewModel : BaseViewModel
    {
        public BluetoothMainItemViewModel(SensorMeasurement measurement)
        {
            TimeStamp = measurement.Timestamp.ToLocalTime();
            AvgHeartrate = measurement?.SensorItemMeasures.FirstOrDefault(x => x.Type == MeasurementItemTypes.HeartRate)?.Average ?? 0;
            StepCount = measurement?.SensorItemMeasures.FirstOrDefault(x => x.Type == MeasurementItemTypes.Step)?.Magnitude ?? 0;
            AvgMicrophone = measurement?.SensorItemMeasures.FirstOrDefault(x => x.Type == MeasurementItemTypes.Microphone)?.Average ?? 0;
        }

        private DateTime _timestamp;
        public DateTime TimeStamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        private double _avgHeartrate;
        public double AvgHeartrate
        {
            get => _avgHeartrate;
            set => SetProperty(ref _avgHeartrate, value);
        }

        private double _stepCount;
        public double StepCount
        {
            get => _stepCount;
            set => SetProperty(ref _stepCount, value);
        }

        private double _avgMicrophone;
        public double AvgMicrophone
        {
            get => _avgMicrophone;
            set => SetProperty(ref _avgMicrophone, value);
        }
    }
}
