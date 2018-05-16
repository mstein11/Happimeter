using System;
using System.Collections.Generic;
using Happimeter.Core.Database;

namespace Happimeter.Core.Models.Bluetooth
{
	public class DataExchangeMessage : BaseBluetoothMessage
	{
		public const string MessageNameConstant = "DEMain";
		public DataExchangeMessage() : base(MessageNameConstant)
		{
		}

		public List<SurveyMeasurement> SurveyMeasurements { get; set; }
		public List<SensorMeasurement> SensorMeasurements { get; set; }

		public DateTime CurrentTimeUtc { get; set; }
	}
}
