using System;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewDetailPageViewModel : BaseViewModel
    {
        public SurveyOverviewDetailPageViewModel(DateTime forDay)
        {
            var entries = ServiceLocator.Instance.Get<IProximityService>().GetProximityEntries(forDay);
            CloseToPersons = entries.GroupBy(x => new { x.CloseToUserId, x.CloseToUserIdentifier })
                                    .Select(x =>
                                            new SurveyOverviewDetailCloseToPersonViewModel(forDay, x.Key.CloseToUserIdentifier, x.ToList()))
                                    .ToList();
        }

        public List<SurveyOverviewDetailCloseToPersonViewModel> CloseToPersons { get; set; }
    }
}
