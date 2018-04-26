using System;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using System.Linq;
using System.Collections.Generic;
using Microcharts;
using SkiaSharp;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyOverviewDetailPageViewModel : BaseViewModel
    {
        private Random Random = new Random();
        public SurveyOverviewDetailPageViewModel(DateTime forDay)
        {
            var entries = ServiceLocator.Instance.Get<IProximityService>().GetProximityEntries(forDay);
            CloseToPersons = entries.GroupBy(x => new { x.CloseToUserId, x.CloseToUserIdentifier })
                                    .Select(x =>
                                            new SurveyOverviewDetailCloseToPersonViewModel(forDay, x.Key.CloseToUserIdentifier, x.ToList()))
                                    .ToList();

            LoadTurnTakingCommand = new Command(async () =>
            {
                var signals = await ServiceLocator.Instance.Get<IHappimeterApiService>().GetSignals(forDay);
                var timestamps = signals.DataTurnTaking.Select(x => x.Timestamp).OrderBy(x => x);
                var chartEntries = signals.DataTurnTaking.GroupBy(x => x.LoudestUserId).Select(x => new Microcharts.Entry(x.Count()) { ValueLabel = x.Key.ToString(), Color = GetRandomColor() }).ToList();
                TurnTakingChart = new DonutChart
                {
                    Entries = chartEntries,
                };
            });
        }

        public List<SurveyOverviewDetailCloseToPersonViewModel> CloseToPersons { get; set; }
        public Command LoadTurnTakingCommand { get; set; }
        private Chart _turnTakingChart;
        public Chart TurnTakingChart
        {
            get => _turnTakingChart;
            set => SetProperty(ref _turnTakingChart, value);
        }

        private SKColor GetRandomColor()
        {


            var color = Color.FromRgb(Random.Next(255), Random.Next(255), Random.Next(255));
            return color.ToSKColor();
        }
    }
}
