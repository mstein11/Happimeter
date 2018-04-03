using System;
using System.Collections.Generic;
using Happimeter.Core.Database;

namespace Happimeter.Core.Models.Bluetooth
{
    public class SwitchMeasurementModeMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "MModeMess";

        public SwitchMeasurementModeMessage(int? value) : base(MessageNameConstant)
        {
            //if value is null, it means continous measurement mode
            if (value != null)
            {
                //if we have a value, it means battery safer measurement mode
                //int value indicates the seconds of one interval
                MessageValue = value.Value.ToString();
            }
        }
    }
}
