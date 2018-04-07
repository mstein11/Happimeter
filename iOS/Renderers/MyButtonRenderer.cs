using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Happimeter.Controls;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(MyButton), typeof(Happimeter.iOS.Renderers.MyButtonRenderer))]
namespace Happimeter.iOS.Renderers
{
    public class MyButtonRenderer : ButtonRenderer
    {
        public MyButtonRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            SetColors();

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);
            if (args.PropertyName == nameof(Button.IsEnabled)) SetColors();
        }

        private void SetColors()
        {
            var myElement = Element as MyButton;
            if (Control != null && myElement != null)
            {
                Control.SetTitleColor(myElement.DisabledColor.ToUIColor(), UIKit.UIControlState.Disabled);
            }
        }
    }
}
