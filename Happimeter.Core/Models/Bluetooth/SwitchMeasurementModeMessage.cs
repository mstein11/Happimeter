using System;
using System.Collections.Generic;
using Happimeter.Core.Database;

namespace Happimeter.Core.Models.Bluetooth
{
    public class SwitchMeasurementModeMessage : BaseBluetoothMessage
    {
        public const string MessageNameConstant = "MModeMess2";

        public SwitchMeasurementModeMessage(int value) : base(MessageNameConstant)
        {
            //int value is id of mode

            //if we have a value, it means battery safer measurement mode
            //int value indicates the seconds of one interval
            MessageValue = value.ToString();

        }
    }
}
