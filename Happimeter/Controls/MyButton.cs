using System;
using Xamarin.Forms;

namespace Happimeter.Controls
{
    public class MyButton : Button
    {

        public static readonly BindableProperty DisabledColorProperty =
            BindableProperty.Create("DisabledColor", typeof(Color), typeof(MyButton), default(Color));

        public Color DisabledColor
        {
            get
            {
                return (Color)GetValue(DisabledColorProperty);
            }
            set
            {
                SetValue(DisabledColorProperty, value);
            }
        }

        public MyButton()
        {
        }
    }
}
