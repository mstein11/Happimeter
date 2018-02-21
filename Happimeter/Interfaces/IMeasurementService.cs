﻿using System.Collections.Generic;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Interfaces
{
    public interface IMeasurementService
    {
        SurveyViewModel GetSurveyQuestions();
        void AddMeasurements(DataExchangeMessage message);
        void AddSurveyData(SurveyViewModel model);
        List<SurveyMeasurement> GetSurveyData();
        List<SurveyMeasurement> GetSurveyMeasurements();
    }
}