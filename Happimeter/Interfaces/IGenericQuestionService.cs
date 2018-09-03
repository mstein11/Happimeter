using System;
using System.Collections;
using Happimeter.Core.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Happimeter.Interfaces
{
    public interface IGenericQuestionService
    {
        IList<GenericQuestion> GetGenericQuestions();
        IList<GenericQuestion> GetActiveGenericQuestions();
        void ToggleGenericQuestionActivation(int questionId, bool isActivated);
        Task<List<GenericQuestion>> DownloadAndSaveGenericQuestions();
    }
}
