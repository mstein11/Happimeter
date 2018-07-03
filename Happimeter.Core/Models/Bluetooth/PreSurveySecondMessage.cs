using System;
using System.Collections.Generic;
using Happimeter.Core.Database;
namespace Happimeter.Core.Models.Bluetooth
{
	public class PreSurveySecondMessage : BaseBluetoothMessage
	{
		public const string MessageNameConstant = "PSFMess";

		public PreSurveySecondMessage() : base(MessageNameConstant)
		{
		}

		public int PredictedPleasance { get; set; }
		public int PredictedActivation { get; set; }

		public DateTime PredictionFrom { get; set; }
		public List<GenericQuestion> Questions { get; set; }
	}
}
