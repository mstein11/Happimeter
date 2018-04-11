using System;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace Happimeter.Services
{
    public class PredictionService : IPredictionService
    {
        public PredictionService()
        {
        }

        public async Task DownloadAndSavePrediction()
        {
            var predictions = await ServiceLocator.Instance.Get<IHappimeterApiService>().GetPredictions();
            if (!predictions.IsSuccess)
            {
                return;
            }
            var dbObjPleasance = new PredictionEntry();
            var dbObjActivation = new PredictionEntry();
            var currentTimestamp = DateTime.UtcNow;
            dbObjPleasance.Timestamp = currentTimestamp;
            dbObjPleasance.PredictedValue = predictions.Pleasance;
            dbObjPleasance.QuestionId = 2;

            dbObjActivation.Timestamp = currentTimestamp;
            dbObjActivation.PredictedValue = predictions.Activation;
            dbObjActivation.QuestionId = 1;

            ServiceLocator.Instance.Get<ISharedDatabaseContext>().Add(dbObjPleasance);
            ServiceLocator.Instance.Get<ISharedDatabaseContext>().Add(dbObjActivation);
        }

        public IList<PredictionEntry> GetLastPrediction()
        {
            var predictions = ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<PredictionEntry>().OrderBy(x => x.Timestamp);

            var lastPleasancePrediction = predictions.Where(x => x.QuestionId == 2).LastOrDefault();
            var lastActivationPrediction = predictions.Where(x => x.QuestionId == 1).LastOrDefault();
            var returnList = new List<PredictionEntry>() {
                lastPleasancePrediction,
                lastActivationPrediction
            };

            return returnList;
        }

    }
}
