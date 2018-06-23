using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ViewModels;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
	public class MeasurementService : IMeasurementService
	{
		public MeasurementService()
		{
		}

		public void AddSurveyMeasurement(SurveyMeasurement measurement)
		{
			var dbContext = ServiceLocator.Instance.Get<IDatabaseContext>();
			dbContext.AddGraph(measurement);
		}

		public SurveyViewModel GetSurveyQuestions()
		{
			var questions = new SurveyViewModel();

			var generics = ServiceLocator.Instance.Get<IDatabaseContext>().GetAll<GenericQuestion>(x => !x.Deactivated);
			foreach (var generic in generics)
			{
				questions.Questions.Add(new SurveyFragmentViewModel(questions)
				{
					Question = generic.Question,
					QuestionShort = generic.QuestionShort,
					QuestionId = generic.QuestionId,
					Answer = 50
				});
			}
			if (!questions.Questions.Any())
			{
				//if we don't have any questions. Add the default ones.
				var question1 = new SurveyFragmentViewModel(questions)
				{
					Question = "How Pleasant do you feel?",
					QuestionShort = "Pleasance",
					IsHardcodedQuestion = true,
					QuestionId = 2,
					Answer = 50
				};
				var question2 = new SurveyFragmentViewModel(questions)
				{
					Question = "How Active do you feel?",
					QuestionShort = "Activation",
					IsHardcodedQuestion = true,
					QuestionId = 1,
					Answer = 50
				};
				questions.Questions.Add(question1);
				questions.Questions.Add(question2);
			}

			var sortedQuestions = new List<SurveyFragmentViewModel>();

			var first = questions.Questions.FirstOrDefault(x => x.QuestionId == 2);
			if (first != null)
			{
				sortedQuestions.Add(first);
			}
			var second = questions.Questions.FirstOrDefault(x => x.QuestionId == 1);
			if (second != null)
			{
				sortedQuestions.Add(second);
			}
			sortedQuestions.AddRange(questions.Questions.Where(x => x.QuestionId != 1 && x.QuestionId != 2));
			questions.Questions = sortedQuestions;

			return questions;
		}

		public void AddGenericQuestions(List<GenericQuestion> questions)
		{
			ServiceLocator.Instance.Get<IDatabaseContext>().DeleteAll<GenericQuestion>();
			foreach (var question in questions)
			{
				question.Id = 0;
				ServiceLocator.Instance.Get<IDatabaseContext>().Add(question);
			}
		}

		public DataExchangeMessage GetMeasurementsForDataTransfer()
		{
			//used to account for time differences between phone and watch
			var referenceTime = DateTime.UtcNow;
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			var moods = ServiceLocator.Instance.Get<IDatabaseContext>().GetAllWithChildren<SurveyMeasurement>().Take(100).ToList();
			stopWatch.Stop();
			Debug.WriteLine($"Read {moods.Count} moods from db. took {stopWatch.Elapsed.Seconds} seconds");
			stopWatch.Reset();
			stopWatch.Start();
			var sensors = ServiceLocator.Instance.Get<IDatabaseContext>().GetSensorMeasurements().ToList();
			Debug.WriteLine($"Read {sensors.Count} sensors from db. took {stopWatch.Elapsed.Seconds} seconds");
			//remove self referencing loop so that we can serialize it
			moods.ForEach(x => x.SurveyItemMeasurement.ForEach(y => y.SurveyMeasurement = null));
			sensors.ForEach(x => x.SensorItemMeasures.ForEach(y => y.SensorMeasurement = null));
			return new DataExchangeMessage { SurveyMeasurements = moods, SensorMeasurements = sensors, CurrentTimeUtc = referenceTime };
		}

		public void DeleteSurveyMeasurement(DataExchangeMessage message)
		{
			var context = ServiceLocator.Instance.Get<IDatabaseContext>();
			foreach (var measruement in message.SurveyMeasurements)
			{
				context.Delete(measruement);
			}
			foreach (var measruement in message.SensorMeasurements)
			{
				context.Delete(measruement);
			}
		}
	}
}
