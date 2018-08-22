using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using System.Linq;
using Xamarin.Forms.GoogleMaps.Extensions;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Views
{
    public partial class MoodMapPage : ContentPage
    {
        public MoodMapPage()
        {
            InitializeComponent();
            (MoodHistoryCard.BindingContext as MoodHistoryCardViewModel).WhenDateSelected().Subscribe(dateAndQuestion =>
            {
                var data = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyData(dateAndQuestion.Item1, dateAndQuestion.Item1.AddDays(1));
                var locations = data.Select(x =>
                {
                    return new
                    {
                        Position = new Position(x.Latitude, x.Longitude),
                        IconColor = _getColorFromInt(x.SurveyItemMeasurement.FirstOrDefault(y => y.QuestionId == dateAndQuestion.Item2).Answer)
                    };
                });
                MapView.Pins.Clear();
                var pins = locations.Select(x => new Pin
                {
                    Position = x.Position,
                    Type = PinType.SearchResult,
                    Icon = BitmapDescriptorFactory.DefaultMarker(x.IconColor),
                    Label = ""
                });
                foreach (var pin in pins)
                {
                    MapView.Pins.Add(pin);
                }
                MapView.MoveToRegion(MapSpan.FromPositions(locations.Select(x => x.Position)));
            });
        }

        private Color _getColorFromInt(int val)
        {
            return val < 33 ? Color.Red : val < 66 ? Color.Yellow : Color.Green;
        }
    }
}
