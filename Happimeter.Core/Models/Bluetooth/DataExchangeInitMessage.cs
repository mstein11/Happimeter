using System;
namespace Happimeter.Core.Models.Bluetooth
{
	public class DataExchangeInitMessage : BaseBluetoothMessage
	{
		public const string MessageNameConstant = "DEInit";
		public DataExchangeInitMessage() : base(MessageNameConstant)
		{
		}
	}
}
