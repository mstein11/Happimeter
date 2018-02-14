using System;
using System.Collections.Generic;
using Happimeter.Core.Database;

namespace Happimeter.Core.Models.Bluetooth
{
    public class DataExchangeMessage : BaseBluetoothMessage
    {
        public DataExchangeMessage() : base(nameof(DataExchangeMessage))
        {
        }

        public List<SurveyMeasurement> SurveyMeasurements { get; set; }
    }
}
