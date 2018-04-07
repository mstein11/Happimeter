using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Happimeter.Controls;
using Android.Content;

[assembly: ExportRenderer(typeof(MyButton), typeof(Happimeter.Droid.Renderers.MyButtonRenderer))]
namespace Happimeter.Droid.Renderers
{
    public class MyButtonRenderer : ButtonRenderer
    {
        public MyButtonRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                //Control.SetMaxLines(2);
                //Control.LayoutParameters.Height = LayoutParams.WrapContent;
                SetColors();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(Button.IsEnabled))
            {
                SetColors();
            }
        }

        private void SetColors()
        {
            var myElement = Element as MyButton;
            if (myElement != null)
            {
                Control.SetTextColor(Element.IsEnabled ? Element.TextColor.ToAndroid() : myElement.DisabledColor.ToAndroid());
            }
        }
    }
}
