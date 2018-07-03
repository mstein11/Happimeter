using System.Collections.Generic;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.ViewModels;
using System;
using Happimeter.Watch.Droid.Database;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
	public interface IMeasurementService
	{
		void AddSensorMeasurement(SensorMeasurement measurement);
		void AddSurveyMeasurement(SurveyMeasurement measurement);
		SurveyViewModel GetSurveyQuestions(int? pleasance = null, int? activation = null);
		DataExchangeMessage GetMeasurementsForDataTransfer();
		void DeleteSurveyMeasurement(DataExchangeMessage message);
		void AddGenericQuestions(List<GenericQuestion> questions);
		IObservable<InfoScreenMeasurements> WhenInfoScreenMeasurementUpdated();
		InfoScreenMeasurements GetInfoScreenMeasurements();
	}
}