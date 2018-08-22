using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Xamarin.Forms;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Happimeter.ViewModels.Forms
{
    public class MoodHistoryCardViewModel : BaseViewModel
    {
        public MoodHistoryCardViewModel()
        {
            QuestionId = 2;
            DateTo = DateTime.Now;
            DateFrom = DateTo - TimeSpan.FromDays(7);
            DateRangeChanged();

            DateRangeEarlierCommand = new Command(() =>
            {
                DateFrom = DateTo;
                DateTo = DateFrom.AddDays(7);
                DateRangeChanged();
            });
            DateRangeLaterCommand = new Command(() =>
            {
                DateTo = DateFrom;
                DateFrom = DateTo.AddDays(-7);
                DateRangeChanged();
            });
            PleasanceActivated = true;
            ActivationActivated = false;
            PleasanceToggledCommand = new Command(() =>
            {
                if (PleasanceActivated)
                {
                    return;
                }
                QuestionId = 2;
                PleasanceActivated = !PleasanceActivated;
                ActivationActivated = !ActivationActivated;
                DateRangeChanged();
            });
            ActivationToggledCommand = new Command(() =>
            {
                if (ActivationActivated)
                {
                    return;
                }
                QuestionId = 1;
                PleasanceActivated = !PleasanceActivated;
                ActivationActivated = !ActivationActivated;
                DateRangeChanged();
            });

        }


        private int _questionId;
        public int QuestionId
        {
            get => _questionId;
            set => SetProperty(ref _questionId, value);
        }
        #region DateRange
        private bool _dateRangeCanBeEarlier;
        public bool DateRangeCanBeEarlier
        {
            get => _dateRangeCanBeEarlier;
            set => SetProperty(ref _dateRangeCanBeEarlier, value);
        }
        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                SetProperty(ref _dateFrom, value);
                OnPropertyChanged(nameof(TimeRangeDisplay));
            }
        }
        private DateTime _dateTo;
        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                SetProperty(ref _dateTo, value);
                if (DateFrom != default(DateTime))
                {
                    //DateRangeChanged();
                }
                OnPropertyChanged(nameof(TimeRangeDisplay));
            }
        }
        public string TimeRangeDisplay { get => string.Format("{0:d} - {1:d}", DateTo, DateFrom); }
        private void DateRangeChanged()
        {
            var moods = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyData((DateTime?)DateFrom, (DateTime?)DateTo);

            foreach (var bar in MoodBars)
            {
                bar.MoodBarSelected -= On_Bar_Tapped;
            }
            MoodBars.Clear();
            var groupedMoods = moods.GroupBy(x => x.Timestamp.Date);
            for (var i = 0; i < (DateTo - DateFrom).Days + 1; i++)
            {
                var currentDate = DateTo.AddDays(i * -1).Date;
                var viewModel = new MoodBarViewModel();
                viewModel.MoodBarSelected += On_Bar_Tapped;
                viewModel.Date = currentDate;
                if (groupedMoods.Any(x => x.Key == currentDate))
                {
                    var currentMoodColors = groupedMoods
                        .FirstOrDefault(x => x.Key == currentDate)
                        .SelectMany(x => x.SurveyItemMeasurement)
                        .Where(x => x.QuestionId == QuestionId)
                        .Select(x =>
                        {
                            if (x.Answer < 33)
                            {
                                return Color.Red;
                            }
                            else if (x.Answer < 66)
                            {
                                return Color.Yellow;
                            }
                            return Color.Green;
                        })
                        .ToList();
                    viewModel.SetColors(currentMoodColors);
                }
                else
                {
                    viewModel.SetColors();
                }
                MoodBars.Add(viewModel);
                var tmp = MoodBars;
                MoodBars = new ObservableCollection<MoodBarViewModel>();
                MoodBars = tmp;
                if (DateTo >= DateTime.Now.Date)
                {
                    DateRangeCanBeEarlier = false;
                }
                else
                {
                    DateRangeCanBeEarlier = true;
                }
            }
        }
        private ICommand _dateRangeEarlierCommand;
        public ICommand DateRangeEarlierCommand
        {
            get => _dateRangeEarlierCommand;
            set => SetProperty(ref _dateRangeEarlierCommand, value);
        }
        private ICommand _dateRangeLaterCommand;
        public ICommand DateRangeLaterCommand
        {
            get => _dateRangeLaterCommand;
            set => SetProperty(ref _dateRangeLaterCommand, value);
        }
        #endregion
        private ObservableCollection<MoodBarViewModel> _moodBars = new ObservableCollection<MoodBarViewModel>();
        public ObservableCollection<MoodBarViewModel> MoodBars
        {
            get => _moodBars;
            set => SetProperty(ref _moodBars, value);
        }

        public void On_Bar_Tapped(object sender, EventArgs e)
        {
            if (!(sender is MoodBarViewModel model))
            {
                return;
            }
            foreach (var oneModel in MoodBars)
            {
                oneModel.IsSelected = false;
            }
            model.IsSelected = true;
            _onDateSelected.OnNext((model.Date, QuestionId));
        }

        private readonly Subject<(DateTime, int)> _onDateSelected = new Subject<(DateTime, int)>();
        public IObservable<(DateTime, int)> WhenDateSelected()
        {
            return _onDateSelected;
        }

        private Command _activationToggledCommand;
        public Command ActivationToggledCommand
        {
            get => _activationToggledCommand;
            set => SetProperty(ref _activationToggledCommand, value);
        }
        private bool _activationActivated;
        public bool ActivationActivated
        {
            get => _activationActivated;
            set => SetProperty(ref _activationActivated, value);
        }
        private Command _pleasanceToggledCommand;
        public Command PleasanceToggledCommand
        {
            get => _pleasanceToggledCommand;
            set => SetProperty(ref _pleasanceToggledCommand, value);
        }
        private bool _pleasanceActivated;
        public bool PleasanceActivated
        {
            get => _pleasanceActivated;
            set => SetProperty(ref _pleasanceActivated, value);
        }
    }
}
