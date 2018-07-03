using System;
namespace Happimeter.Core.Models.Bluetooth
{
	public class PreSurveyFirstMessage : BaseBluetoothMessage
	{
		public const string MessageNameConstant = "PSFMess";

		public PreSurveyFirstMessage() : base(MessageNameConstant)
		{
		}
	}
}
