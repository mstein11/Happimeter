using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Helpers;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Core.Services;
using Happimeter.Interfaces;
using Happimeter.Models.ServiceModels;
using Happimeter.ViewModels.Forms;
using System.Collections.ObjectModel;

namespace Happimeter.Services
{
	public class MeasurementService : IMeasurementService
	{
		public MeasurementService()
		{
		}

		private const double MeterPerSqaureSecondToMilliGForce = 101.93679918451;


		public IList<GenericQuestion> GetGenericQuestions()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();

			return context.GetAll<GenericQuestion>();
		}

		public IList<GenericQuestion> GetActiveGenericQuestions()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			return context.GetAll<GenericQuestion>().Where(x => !x.Deactivated).ToList();
		}

		public void ToggleGenericQuestionActivation(int questionId, bool isActivated)
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var question = context.Get<GenericQuestion>(x => x.Id == questionId);
			question.Activated = isActivated;
			context.Update(question);
		}

		/// <summary>
		///     Method returns Generic Questions for display in survey.
		/// </summary>
		/// <returns>The survey questions.</returns>
		public SurveyViewModel GetSurveyQuestions()
		{
			var questions = new SurveyViewModel();
			var dbQuestions = GetActiveGenericQuestions();
			var additionalQuestions = dbQuestions.Select(x => new SurveyItemViewModel
			{
				Question = x.Question,
				QuestionId = x.QuestionId,
				QuestionShort = x.QuestionShort,
				Answer = .5,
			});

			if (additionalQuestions.Count() == 0)
			{
				//if we don't have downloaded any questions, lets use the standart questions
				var question1 = new SurveyItemViewModel
				{
					Question = "How Pleasant do you feel?",
					QuestionShort = "Pleasance",
					QuestionId = 2, //QuestionId corresponds to the question id of the server
					Answer = .5
				};
				var question2 = new SurveyItemViewModel
				{
					Question = "How Active do you feel?",
					QuestionShort = "Activation",
					QuestionId = 1, //QuestionId corresponds to the question id of the server
					Answer = .5
				};
				questions.SurveyItems.Add(question1);
				questions.SurveyItems.Add(question2);
			}
			else
			{
				questions.SurveyItems.AddRange(additionalQuestions);
			}

			return questions;
		}

		public MyTabMenuViewModel GetQuestionsToDisplayInTabMenu()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var surveyitems = context.GetAll<SurveyItemMeasurement>().Where(x => x.QuestionShort != null).GroupBy(x => new { x.QuestionId, x.QuestionShort });
			List<TabMenuItemViewModel> viewModelItems;
			viewModelItems = surveyitems.Select(x => new TabMenuItemViewModel { Text = x.Key.QuestionShort, Id = x.Key.QuestionId }).ToList();

			var questions = GetSurveyQuestions();
			//merge those that don't have an answer yet
			foreach (var question in questions.SurveyItems.Where(x => x.QuestionShort != null))
			{
				if (viewModelItems.All(x => x.Id != question.QuestionId))
				{
					viewModelItems.Add(new TabMenuItemViewModel { Text = question.QuestionShort, Id = question.QuestionId });
				}
			}

			//if we don't have the two basic ones, lets add them
			if (!viewModelItems.Any(x => x.Id == 1))
			{
				viewModelItems.Add(new TabMenuItemViewModel { Text = "Activation", Id = 1 });
			}

			if (!viewModelItems.Any(x => x.Id == 2))
			{
				viewModelItems.Add(new TabMenuItemViewModel { Text = "Pleasance", Id = 2 });
			}

			var observableCollection = new ObservableCollection<TabMenuItemViewModel>(viewModelItems);

			var viewModel = new MyTabMenuViewModel();
			viewModel.Items = observableCollection;
			return viewModel;
		}

		public async Task AddMeasurements(DataExchangeMessage message)
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();

			var location = await ServiceLocator.Instance.Get<IGeoLocationService>().GetLocation();

			//save survey responses
			foreach (var measurement in message.SurveyMeasurements)
			{
				measurement.IdFromWatch = measurement.Id;
				measurement.Id = 0;

				if (location != null)
				{
					measurement.Altitude = location.Altitude;
					measurement.Longitude = location.Longitude;
					measurement.Latitude = location.Latitude;
				}
				context.AddGraph(measurement);
			}

			//save sensor data
			foreach (var measurement in message.SensorMeasurements)
			{
				measurement.IdFromWatch = measurement.Id;
				measurement.Id = 0;

				if (location != null && !measurement.SensorItemMeasures.Any(x => x.Type == MeasurementItemTypes.LocationLon) && !measurement.SensorItemMeasures.Any(x => x.Type == MeasurementItemTypes.LocationLat))
				{
					measurement.SensorItemMeasures.Add(new SensorItemMeasurement
					{
						Type = MeasurementItemTypes.LocationLon,
						Magnitude = location.Longitude
					});
					measurement.SensorItemMeasures.Add(new SensorItemMeasurement
					{
						Type = MeasurementItemTypes.LocationLat,
						Magnitude = location.Latitude
					});
					measurement.SensorItemMeasures.Add(new SensorItemMeasurement
					{
						Type = MeasurementItemTypes.LocationAlt,
						Magnitude = location.Altitude
					});
				}

				context.AddGraph(measurement);
			}
		}

		public async Task AddSurveyData(SurveyViewModel model)
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();


			var surveyMeasurement = new SurveyMeasurement
			{
				IdFromWatch = -1,
				Timestamp = DateTime.UtcNow,
				SurveyItemMeasurement = new List<SurveyItemMeasurement>(),
				//GenericQuestionGroupId = model.GenericQuestionGroupId
			};
			var location = await ServiceLocator.Instance.Get<IGeoLocationService>().GetLocation();
			if (location != null)
			{
				surveyMeasurement.Latitude = location.Latitude;
				surveyMeasurement.Longitude = location.Longitude;
				surveyMeasurement.Altitude = location.Altitude;
			}



			foreach (var item in model.SurveyItems)
			{
				surveyMeasurement.SurveyItemMeasurement.Add(new SurveyItemMeasurement
				{
					Answer = (int)(item.Answer * 100),
					Question = item.Question,
					QuestionId = item.QuestionId,//identifier for server
					AnswerDisplay = item.AnswerDisplay,
					QuestionShort = item.QuestionShort
				});
			}

			context.AddGraph(surveyMeasurement);
			var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
			await apiService.UploadMood();
		}

		public bool HasUnsynchronizedChanges()
		{
			return HasUnsynchronizedSensorData() || HasUnsynchronizedSurveyData();
		}

		public bool HasUnsynchronizedSensorData()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			return context.Get<SensorMeasurement>(x => !x.IsUploadedToServer) != null;
		}

		public int CountUnsynchronizedSensorData()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			return context.GetAll<SensorMeasurement>(x => !x.IsUploadedToServer).Count();
		}

		public bool HasUnsynchronizedSurveyData()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			return context.Get<SurveyMeasurement>(x => !x.IsUploadedToServer) != null;
		}

		public int CountUnsynchronizedSurveyData()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			return context.GetAll<SurveyMeasurement>(x => !x.IsUploadedToServer).Count();
		}

		public List<SurveyMeasurement> GetSurveyData()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			return context.GetAllWithChildren<SurveyMeasurement>().ToList();
		}


		/// <summary>
		///     returns the data in a format that we can send to the server
		/// </summary>
		/// <returns>The survey model for server.</returns>
		public List<PostMoodServiceModel> GetSurveyModelForServer()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var entries = context.GetAllWithChildren<SurveyMeasurement>(x => !x.IsUploadedToServer);
			var groupId = ServiceLocator
				.Instance
				.Get<IConfigService>()
				.GetConfigValueByKey(ConfigService.GenericQuestionGroupIdKey);
			var result = new List<PostMoodServiceModel>();

			foreach (var entry in entries)
			{
				var answers = entry.SurveyItemMeasurement.Where(x => x.QuestionId != 0).Select(x => new KeyValuePair<int, object>(x.QuestionId, new { answer = GetOldSurveyScaleValue(x.Answer), answer_new_scale = x.Answer })).ToDictionary(x => x.Key, x => x.Value);
				result.Add(new PostMoodServiceModel
				{
					Id = entry.Id,
					Timestamp = UtilHelper.GetUnixTimestamp(entry.Timestamp),
					LocalTimestamp = UtilHelper.GetUnixTimestamp(entry.Timestamp.ToLocalTime()),
					Activation = GetOldSurveyScaleValue(entry
														?.SurveyItemMeasurement
														?.FirstOrDefault(x => x.QuestionId == (int)SurveyHardcodedEnumeration.Activation)?.Answer ?? 0),
					Pleasance = GetOldSurveyScaleValue(entry
														?.SurveyItemMeasurement
													   ?.FirstOrDefault(x => x.QuestionId == (int)SurveyHardcodedEnumeration.Pleasance)?.Answer ?? 0),
					DeviceId = "",
					Position = entry.Latitude != default(double) && entry.Longitude != default(double)
									? new PostMoodServicePositionModel(entry.Latitude, entry.Longitude) : null,
					MoodAnswers = answers
				});
			}

			return result;
		}

		public void SetIsUploadedToServerForSurveys(PostMoodServiceModel survey)
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var toUpdate = context.Get<SurveyMeasurement>(x => x.Id == survey.Id);
			toUpdate.IsUploadedToServer = true;
			context.Update(toUpdate);
		}

		public void SetIsUploadedToServerForSensorData(PostSensorDataServiceModel sensor)
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var toUpdate = context.Get<SensorMeasurement>(x => x.Id == sensor.Id);
			toUpdate.IsUploadedToServer = true;
			context.Update(toUpdate);
		}

		/// <summary>
		///     We only return 150 entries to prevent errors. If we return more, than during the upload process the app might be killed because too long in background.
		///     The first return list contains the data in a format compatible with happimeter api v1. However, in this format we can not save all the data
		///     The second reutnr list contains the data in a format compatible with happimeter api v2. Here we can send all the data, but the api might not be available at this point.
		/// </summary>
		/// <returns>The survey model for server.</returns>
		public (List<PostSensorDataServiceModel>, List<SensorMeasurement>) GetSensorDataForServer()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var entries = context.GetAllWithChildren<SensorMeasurement>(x => !x.IsUploadedToServer).Take(10).ToList();

			var result = new List<PostSensorDataServiceModel>();
			foreach (var entry in entries)
			{
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
					Position = new PositionModel
					{
						Altitude = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.LocationAlt)?.Magnitude ?? null,
						Latitude = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.LocationLat)?.Magnitude ?? null,
						Longitude = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.LocationLon)?.Magnitude ?? null,
					},
					Activity = GetOldActivityScaleValue(entry.SensorItemMeasures),
					Vmc = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.Vmc)?.Magnitude ?? 0,
					AvgLightlevel = entry.SensorItemMeasures?.FirstOrDefault(x => x.Type == MeasurementItemTypes.Light)?.Magnitude ?? 0,
				});
			}

			//remove circular references so that we can jsonify
			entries.ForEach(e => e.SensorItemMeasures.ForEach(i => i.SensorMeasurement = null));

			return (result, entries);
		}

		private int GetOldSurveyScaleValue(int newScaleValue)
		{

			if (newScaleValue < 33)
			{
				return 0;
			}
			if (newScaleValue < 66)
			{
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
		private int GetOldAccelerometerScaleValue(double newScaleValue)
		{
			return (int)(newScaleValue * MeterPerSqaureSecondToMilliGForce);
		}

		private int GetOldActivityScaleValue(List<SensorItemMeasurement> items)
		{
			var activityMeasures = items.Where(x => MeasurementItemTypes.ActivityTypes.Contains(x.Type));
			var mostLikelyActivity = items.OrderByDescending(x => x.Average).FirstOrDefault();

			if (mostLikelyActivity == null)
			{
				//lookup unspecific actovity
				return 2;
			}

			if (mostLikelyActivity.Type == MeasurementItemTypes.ActivityOnFoot)
			{
				mostLikelyActivity = activityMeasures.Where(x => x.Type != MeasurementItemTypes.ActivityOnFoot).OrderByDescending(x => x.Magnitude).FirstOrDefault();
			}

			if (mostLikelyActivity.Type == MeasurementItemTypes.ActivityWalking)
			{
				return 3;
			}

			if (mostLikelyActivity.Type == MeasurementItemTypes.ActivityRunning)
			{
				return 4;
			}

			if (mostLikelyActivity.Type == MeasurementItemTypes.ActivityStill)
			{
				return 1;
			}

			return 2;
		}

		public List<SurveyMeasurement> GetSurveyMeasurements()
		{
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			return context.GetAllWithChildren<SurveyMeasurement>();
		}

		/// <summary>
		///     Returns null, when api return an error (e.g. no internt)
		/// </summary>
		/// <returns>The and save generic questions.</returns>
		public async Task<List<GenericQuestion>> DownloadAndSaveGenericQuestions()
		{
			List<GenericQuestionItemApiResult> genericQuestions = new List<GenericQuestionItemApiResult>();
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();

			var api = ServiceLocator.Instance.Get<IHappimeterApiService>();
			var questions = await api.GetGenericQuestions();
			if (!questions.IsSuccess)
			{
				return null;
			}
			genericQuestions.AddRange(questions.Questions);

			context.DeleteAll<GenericQuestion>();

			var dbQuestions = genericQuestions.Select(q => new GenericQuestion
			{
				//GenericQuestionGroupId = groupId,
				Question = q.Question,
				QuestionShort = q.QuestionShort,
				QuestionId = q.Id
			}).ToList();

			foreach (var dbQuestion in dbQuestions)
			{
				context.Add(dbQuestion);
			}
			return dbQuestions;
		}
	}
}
