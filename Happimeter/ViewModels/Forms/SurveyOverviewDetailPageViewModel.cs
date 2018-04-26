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
            LoadTurnTakingText = "Load Turntaking";
            var entries = ServiceLocator.Instance.Get<IProximityService>().GetProximityEntries(forDay);
            CloseToPersons = entries.GroupBy(x => new { x.CloseToUserId, x.CloseToUserIdentifier })
                                    .Select(x =>
                                            new SurveyOverviewDetailCloseToPersonViewModel(forDay, x.Key.CloseToUserIdentifier, x.ToList()))
                                    .ToList();
            if (!CloseToPersons.Any())
            {
                NoProximityData = true;
            }
            else
            {
                NoProximityData = false;
            }

            LoadTurnTakingDataIsEnabled = true;
            LoadTurnTakingCommand = new Command(async () =>
            {
                LoadTurnTakingText = "Loading...";
                LoadTurnTakingDataIsEnabled = false;
                var signals = await ServiceLocator.Instance.Get<IHappimeterApiService>().GetSignals(forDay);
                var timestamps = signals.DataTurnTaking.Select(x => x.Timestamp).OrderBy(x => x);
                var chartEntries = signals.DataTurnTaking.GroupBy(x => x.LoudestUserId).Select(x => new Microcharts.Entry(x.Count()) { ValueLabel = x.Key.ToString(), Color = GetRandomColor() }).ToList();

                if (!chartEntries.Any())
                {
                    ShowNoTurnTakingData = true;
                    HasTurnTakingChart = false;
                    TurnTakingChart = null;
                    return;
                }
                HasTurnTakingChart = true;
                ShowNoTurnTakingData = false;
                TurnTakingChart = new DonutChart
                {
                    Entries = chartEntries,
                };
                LoadTurnTakingDataIsEnabled = true;
                LoadTurnTakingText = "Load Turntaking";
            });
        }

        public List<SurveyOverviewDetailCloseToPersonViewModel> CloseToPersons { get; set; }
        public Command LoadTurnTakingCommand { get; set; }

        private bool _loadTurnTakingIsEnabled;
        public bool LoadTurnTakingDataIsEnabled
        {
            get => _loadTurnTakingIsEnabled;
            set => SetProperty(ref _loadTurnTakingIsEnabled, value);
        }

        private string _loadTurnTakingText;
        public string LoadTurnTakingText
        {
            get => _loadTurnTakingText;
            set => SetProperty(ref _loadTurnTakingText, value);
        }

        private Chart _turnTakingChart;
        public Chart TurnTakingChart
        {
            get => _turnTakingChart;
            set => SetProperty(ref _turnTakingChart, value);
        }

        private bool _hasTurnTakingChart;
        public bool HasTurnTakingChart
        {
            get => _hasTurnTakingChart;
            set => SetProperty(ref _hasTurnTakingChart, value);
        }

        private bool _showNoTurnTakingData;
        public bool ShowNoTurnTakingData
        {
            get => _showNoTurnTakingData;
            set => SetProperty(ref _showNoTurnTakingData, value);
        }

        private bool _noProximityData;
        public bool NoProximityData
        {
            get => _noProximityData;
            set => SetProperty(ref _noProximityData, value);
        }

        private SKColor GetRandomColor()
        {
            var color = Color.FromRgb(Random.Next(255), Random.Next(255), Random.Next(255));
            return color.ToSKColor();
        }
    }
}
