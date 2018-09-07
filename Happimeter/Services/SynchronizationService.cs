using System;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using System.Threading.Tasks;
namespace Happimeter.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        private readonly IPredictionService _predictionService;
        private readonly IProximityService _proximityService;
        private readonly ITeamService _teamService;
        public SynchronizationService()
        {
            _predictionService = ServiceLocator.Instance.Get<IPredictionService>();
            _proximityService = ServiceLocator.Instance.Get<IProximityService>();
            _teamService = ServiceLocator.Instance.Get<ITeamService>();
        }

        public async Task Sync()
        {
            await _predictionService.DownloadAndSavePrediction();
            await _proximityService.DownloadAndSaveProximity();
            await _teamService.DownloadAndSave();
        }
    }
}
