using System;
using System.Collections.Generic;
using SuaveControls.Views;
using Xamarin.Forms;

namespace Happimeter.Controls
{
    public partial class MyFloatingActionButton : FloatingActionButton
    {
        public MyFloatingActionButton()
        {
            InitializeComponent();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    WidthRequest = 50;
                    HeightRequest = 50;
                    break;
                default:
                    WidthRequest = 80;
                    HeightRequest = 90;
                    break;
            }
        }
    }
}
