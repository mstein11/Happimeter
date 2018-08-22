using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
namespace Happimeter.ViewModels.Forms
{
    public class MoodBarViewModel : BaseViewModel
    {
        public MoodBarViewModel()
        {
            SetColors();
            TabbedCommand = new Command(() =>
            {
                MoodBarSelected?.Invoke(this, null);
            });
        }

        private ObservableCollection<Color> _colors;
        public ObservableCollection<Color> Colors
        {
            get => _colors;
            private set => SetProperty(ref _colors, value);
        }

        public void SetColors(List<Color> colors = null)
        {
            if (colors == null)
            {
                colors = new List<Color>();
            }
            if (!colors.Any())
            {
                colors.Add(Color.White);
            }
            var numberOfBarsToFill = 48;
            var numberOfInputs = colors.Count;

            var rand = new Random();
            //if we have more inputs than pixel to show
            while (colors.Count > numberOfBarsToFill)
            {
                var indexToRemove = rand.Next(0, colors.Count - 1);
                colors.RemoveAt(indexToRemove);
            }
            var finalList = new ObservableCollection<Color>();
            if (colors.Count < numberOfBarsToFill)
            {
                var ratio = (int)numberOfBarsToFill / colors.Count;
                foreach (var color in colors)
                {
                    for (var i = 0; i < ratio; i++)
                    {
                        finalList.Add(color);
                    }
                }

                while (finalList.Count < numberOfBarsToFill)
                {
                    var indexToCopy = rand.Next(0, colors.Count - 1);
                    finalList.Insert(indexToCopy, finalList[indexToCopy]);
                }
            }

            if (finalList.Count != numberOfBarsToFill)
            {
                throw new Exception("Problem here!");
            }
            Colors = finalList;
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private ICommand _tabbedCommand;
        public ICommand TabbedCommand
        {
            get => _tabbedCommand;
            set => SetProperty(ref _tabbedCommand, value);
        }

        public event EventHandler MoodBarSelected;
    }
}
