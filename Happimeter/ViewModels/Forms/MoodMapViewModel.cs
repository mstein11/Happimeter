using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.GoogleMaps;
using System.Security.Policy;
namespace Happimeter.ViewModels.Forms
{
    public class MoodMapViewModel : BaseViewModel
    {
        public MoodMapViewModel()
        {
            var tmp = new Pin()
            {
                Type = PinType.Place,
                Label = "Tokyo SKYTREE",
                Address = "Sumida-ku, Tokyo, Japan",
                Position = new Position(35.71d, 139.81d),
                Rotation = 33.3f,
                Tag = "id_tokyo"
            };
            Pins.Add(tmp);
        }

        private ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
        public ObservableCollection<Pin> Pins
        {
            get => _pins;
            set => SetProperty(ref _pins, value);
        }
    }
}
