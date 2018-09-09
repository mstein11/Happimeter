using System;
using System.Collections.Generic;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Core.Services;
using Happimeter.Interfaces;
namespace Happimeter.Services
{
    public class GenericQuestionService : IGenericQuestionService
    {
        public IList<GenericQuestion> GetGenericQuestions()
        {
            var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAll<GenericQuestion>().Where(x => x.UserId == userId).ToList();
        }

        public IList<GenericQuestion> GetActiveGenericQuestions()
        {
            var userId = ServiceLocator.Instance.Get<IAccountStoreService>()
                                       .GetAccountUserId();
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAll<GenericQuestion>()
                          .Where(x => !x.Deactivated
                                 && x.UserId == userId)
                          .ToList();
        }

        public void ToggleGenericQuestionActivation(int questionId, bool isActivated)
        {
            var userId = ServiceLocator.Instance.Get<IAccountStoreService>()
                                       .GetAccountUserId();
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var question = context
                .Get<GenericQuestion>(x => x.Id == questionId
                                      && x.UserId == userId);
            question.Activated = isActivated;
            context.Update(question);
        }

        /// <summary>
        ///     Returns null, when api return an error (e.g. no internt)
        /// </summary>
        /// <returns>The and save generic questions.</returns>
        public async Task<List<GenericQuestion>> DownloadAndSaveGenericQuestions()
        {
            try
            {
                var currentUserId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
                System.Diagnostics.Debug.WriteLine("DownloadAndSaveGenericQuestions.Start");
                var api = ServiceLocator.Instance.Get<IHappimeterApiService>();
                var questions = await api.GetGenericQuestions();
                if (!questions.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine("Questions.NotSuccessful");
                    return null;
                }

                var newQuestions = questions.Questions.Select(q => new GenericQuestion
                {
                    Question = q.Question,
                    QuestionShort = q.QuestionShort,
                    QuestionId = q.Id,
                    UserId = currentUserId
                }).ToList();
                System.Diagnostics.Debug.WriteLine(newQuestions.Count());
                var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
                var oldQuestions = context.GetAll<GenericQuestion>();
                foreach (var question in newQuestions)
                {
                    System.Diagnostics.Debug.WriteLine("DownloadAndSaveGenericQuestions.inForEach");
                    var matchedQuestion = oldQuestions
                        .FirstOrDefault(x => x.QuestionId == question.QuestionId
                                        && x.UserId == question.UserId);

                    if (matchedQuestion != null)
                    {
                        question.Id = matchedQuestion.Id;
                        question.Deactivated = matchedQuestion.Deactivated;
                        question.Activated = matchedQuestion.Activated;
                        context.Update(question);
                    }
                    else
                    {
                        context.Add(question);
                    }
                }
                System.Diagnostics.Debug.WriteLine("DownloadAndSaveGenericQuestions.Return");
                return newQuestions;
            }
            catch (Exception e)
            {
                ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
                System.Diagnostics.Debug.WriteLine("DownloadAndSaveGenericQuestions.Catch");
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}
