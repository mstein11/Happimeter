﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Helpers;
using Happimeter.Interfaces;
using Happimeter.Models.ServiceModels;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Services
{
    public class MeasurementService : IMeasurementService
    {
        public MeasurementService()
        {
        }

        private const double MeterPerSqaureSecondToMilliGForce = 101.93679918451;



        public SurveyViewModel GetSurveyQuestions()
        {

            var groupId = ServiceLocator
                .Instance
                .Get<IConfigService>()
                .GetConfigValueByKey(ConfigService.GenericQuestionGroupIdKey);

            var questions = new SurveyViewModel
            {
                GenericQuestionGroupId = groupId
            };

            var question1 = new SurveyItemViewModel
            {
                Question = "How Pleasant do you feel?",
                HardcodedId = 1,
                Answer = .5
            };
            var question2 = new SurveyItemViewModel
            {
                Question = "How Active do you feel?",
                HardcodedId = 2,
                Answer = .5
            };

            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();




            var dbQuestions = context.GetAll<GenericQuestion>(x => x.GenericQuestionGroupId == groupId);
            var additionalQuestions = dbQuestions.Select(x => new SurveyItemViewModel
            {
                Question = x.Question,
                Answer = .5,
            });

            questions.SurveyItems.Add(question1);
            questions.SurveyItems.Add(question2);
            questions.SurveyItems.AddRange(additionalQuestions);

            return questions;
        }

        public void AddMeasurements(DataExchangeMessage message) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();

            foreach (var measurement in message.SurveyMeasurements)
            {
                measurement.IdFromWatch = measurement.Id;
                measurement.Id = 0;
                context.AddGraph(measurement);
            }

            if (message.SurveyMeasurements.Any()) {
                apiService.UploadMood();
            }

            foreach (var measurement in message.SensorMeasurements)
            {
                measurement.IdFromWatch = measurement.Id;
                measurement.Id = 0;
                context.AddGraph(measurement);
            }
            if (message.SensorMeasurements.Any()) {
                apiService.UploadSensor();
            }
        }

        public void AddSurveyData(SurveyViewModel model) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var surveyMeasurement = new SurveyMeasurement
            {
                IdFromWatch = -1,
                Timestamp = DateTime.UtcNow,
                SurveyItemMeasurement = new List<SurveyItemMeasurement>(),
                GenericQuestionGroupId = model.GenericQuestionGroupId
            };

            foreach (var item in model.SurveyItems) {
                surveyMeasurement.SurveyItemMeasurement.Add(new SurveyItemMeasurement
                {
                    Answer = (int)(item.Answer * 100),
                    HardcodedQuestionId = item.HardcodedId,
                    Question = item.Question,
                    AnswerDisplay = item.AnswerDisplay
                });  
            }

            context.AddGraph(surveyMeasurement);
            var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
            apiService.UploadMood();
        }

        public bool HasUnsynchronizedChanges() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var needsMoodUpload = context.Get<SurveyMeasurement>(x => !x.IsUploadedToServer) != null;
            var needsSensorUpload = context.Get<SensorMeasurement>(x => !x.IsUploadedToServer) != null;

            return needsMoodUpload || needsSensorUpload;
        }

        public List<SurveyMeasurement> GetSurveyData() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAllWithChildren<SurveyMeasurement>().ToList();
        }


        /// <summary>
        ///     The first return list contains the data in a format compatible with happimeter api v1. However, in this format we can not save all the data
        ///     The second reutnr list contains the data in a format compatible with happimeter api v2. Here we can send all the data, but the api might not be available at this point.
        /// </summary>
        /// <returns>The survey model for server.</returns>
        public (List<PostMoodServiceModel>, List<SurveyMeasurement>) GetSurveyModelForServer() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var entries = context.GetAllWithChildren<SurveyMeasurement>(x => !x.IsUploadedToServer);
            var groupId = ServiceLocator
                .Instance
                .Get<IConfigService>()
                .GetConfigValueByKey(ConfigService.GenericQuestionGroupIdKey);
            var result = new List<PostMoodServiceModel>();

            foreach (var entry in entries) {
                result.Add(new PostMoodServiceModel
                {
                    Id = entry.Id,
                    Timestamp = new DateTimeOffset(entry.Timestamp).ToUnixTimeSeconds(),
                    LocalTimestamp = new DateTimeOffset(entry.Timestamp.ToLocalTime()).ToUnixTimeSeconds(),
                    Activation = GetOldSurveyScaleValue(entry
                                                        ?.SurveyItemMeasurement
                                                        ?.FirstOrDefault(x => x.HardcodedQuestionId == (int)SurveyHardcodedEnumeration.Activation)?.Answer ?? 0),
                    Pleasance = GetOldSurveyScaleValue(entry
                                                        ?.SurveyItemMeasurement
                                                        ?.FirstOrDefault(x => x.HardcodedQuestionId == (int)SurveyHardcodedEnumeration.Pleasance)?.Answer ?? 0),
                    DeviceId = "",
                    GenericQuestionCount = entry.GenericQuestionGroupId != null ? entry.SurveyItemMeasurement.Count(x => x.HardcodedQuestionId == 0 || x.HardcodedQuestionId == null) : 0,
                    GenericQuestionGroup = entry.GenericQuestionGroupId,
                    GenericQuestionValues = entry.SurveyItemMeasurement.Where(x => x.HardcodedQuestionId == 0 || x.HardcodedQuestionId == null).Select(x => GetOldSurveyScaleValue(x.Answer)).ToArray()
                });
            }

            //remove circular references so that we can jsonify
            entries.ForEach(e => e.SurveyItemMeasurement.ForEach(i => i.SurveyMeasurement = null));

            return (result, entries);
        }

        public void SetIsUploadedToServerForSurveys(PostMoodServiceModel survey) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var toUpdate = context.Get<SurveyMeasurement>(x => x.Id == survey.Id);
            toUpdate.IsUploadedToServer = true;
            context.Update(toUpdate);
        }

        public void SetIsUploadedToServerForSensorData(PostSensorDataServiceModel sensor) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var toUpdate = context.Get<SensorMeasurement>(x => x.Id == sensor.Id);
            toUpdate.IsUploadedToServer = true;
            context.Update(toUpdate);
        }

        /// <summary>
        ///     The first return list contains the data in a format compatible with happimeter api v1. However, in this format we can not save all the data
        ///     The second reutnr list contains the data in a format compatible with happimeter api v2. Here we can send all the data, but the api might not be available at this point.
        /// </summary>
        /// <returns>The survey model for server.</returns>
        public (List<PostSensorDataServiceModel>, List<SensorMeasurement>) GetSensorDataForServer() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var entries = context.GetAllWithChildren<SensorMeasurement>(x => !x.IsUploadedToServer);

            var result = new List<PostSensorDataServiceModel>();
            foreach(var entry in entries) {
                result.Add(new PostSensorDataServiceModel
                {
                    Id = entry.Id,
                    AvgHeartrate = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.HeartRate)?.Average ?? 0,
                    LocalTimestamp = new DateTimeOffset(entry.Timestamp.ToLocalTime()).ToUnixTimeSeconds(),
                    Timestamp = new DateTimeOffset(entry.Timestamp).ToUnixTimeSeconds(),
                    Accelerometer = new AccelerometerModel
                    {
                        AvgX = GetOldAccelerometerScaleValue(entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.AccelerometerX)?.Average ?? 0),
                        AvgY = GetOldAccelerometerScaleValue(entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.AccelerometerY)?.Average ?? 0),
                        AvgZ = GetOldAccelerometerScaleValue(entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.AccelerometerZ)?.Average ?? 0),
                        VarX = Math.Pow(GetOldAccelerometerScaleValue(entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.AccelerometerX)?.StdDev ?? 0), 2),
                        VarY = Math.Pow(GetOldAccelerometerScaleValue(entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.AccelerometerY)?.StdDev ?? 0), 2),
                        VarZ = Math.Pow(GetOldAccelerometerScaleValue(entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.AccelerometerZ)?.StdDev ?? 0), 2)
                    },
                    Position = new PositionModel {
                        Altitude = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.LocationAlt)?.Magnitude ?? 0,
                        Latitude = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.LocationLat)?.Magnitude ?? 0,
                        Longitude = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.LocationLon)?.Magnitude ?? 0,
                    }
                });
            }

            //remove circular references so that we can jsonify
            entries.ForEach(e => e.SensorItemMeasures.ForEach(i => i.SensorMeasurement = null));

            return (result, entries);
        }

        private int GetOldSurveyScaleValue(int newScaleValue) {

            if (newScaleValue < 33) {
                return 0;
            } 
            if (newScaleValue < 66) {
                return 1;
            }
            return 2;
        }

        /// <summary>
        ///     Pebble measure acceleration in micro G. Android watches do the same in meters per square second.
        ///     This method takes a value in meters per square second and converts it to microG.
        /// </summary>
        /// <returns>The old accelerometer scale value.</returns>
        /// <param name="newScaleValue">New scale value.</param>
        private int GetOldAccelerometerScaleValue(double newScaleValue) {
            return (int) (newScaleValue * MeterPerSqaureSecondToMilliGForce);
        }

        public List<SurveyMeasurement> GetSurveyMeasurements() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAllWithChildren<SurveyMeasurement>();
        }

        /// <summary>
        ///     Returns null, when api return an error (e.g. no internt)
        /// </summary>
        /// <returns>The and save generic questions.</returns>
        /// <param name="groupId">Group identifier.</param>
        public async Task<List<GenericQuestion>> DownloadAndSaveGenericQuestions(string groupId) {
            List<string> genericQuestions = new List<string>();
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            if (!string.IsNullOrEmpty(groupId)) {
                var api = ServiceLocator.Instance.Get<IHappimeterApiService>();
                var questions = await api.GetGenericQuestions(groupId);
                if (!questions.IsSuccess)
                {
                    return null;
                }
                genericQuestions.AddRange(questions.Questions);
            }


            context.DeleteAll<GenericQuestion>();

            ServiceLocator
                .Instance
                .Get<IConfigService>()
                .AddOrUpdateConfigEntry(ConfigService.GenericQuestionGroupIdKey, groupId);

            var dbQuestions = genericQuestions.Select(q => new GenericQuestion
            {
                GenericQuestionGroupId = groupId,
                Question = q
            }).ToList();

            foreach (var dbQuestion in dbQuestions) {
                context.Add(dbQuestion);    
            }
            return dbQuestions;
        }
    }
}
