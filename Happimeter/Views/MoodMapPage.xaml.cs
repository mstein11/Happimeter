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
            (MoodHistoryCard.BindingContext as MoodHistoryCardViewModel).WhenDateSelected().Subscribe(date =>
            {
                var data = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyData(date, date.AddDays(1));
                var locations = data.Select(x =>
                {
                    return new Position(x.Latitude, x.Longitude);
                });
                var pins = locations.Select(x => new Pin
                {
                    Position = x,
                    Type = PinType.SearchResult,
                    Icon = BitmapDescriptorFactory.DefaultMarker(Color.Green),
                    Label = ""
                });
                foreach (var pin in pins)
                {
                    MapView.Pins.Add(pin);
                }
                MapView.MoveToRegion(MapSpan.FromPositions(locations));
            });
        }
    }
}
