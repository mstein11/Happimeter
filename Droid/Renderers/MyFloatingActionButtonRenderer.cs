using System;
using SuaveControls.FloatingActionButton.Droid.Renderers;
using Android.Support.Design.Widget;
using Android.Content;
using Java.Lang;
using Acr.Caching;
using SuaveControls.Views;
using Xamarin.Forms.Platform.Android;
using FAB = Android.Support.Design.Widget.FloatingActionButton;
using Android.Content.Res;
using System.ComponentModel;
using Xamarin.Forms;
using Happimeter.Droid.Renderers;
using Happimeter.Controls;

[assembly: ExportRenderer(typeof(MyFloatingActionButton), typeof(MyFloatingActionButtonRenderer))]
namespace Happimeter.Droid.Renderers
{
    public class MyFloatingActionButtonRenderer : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer<SuaveControls.Views.FloatingActionButton, FAB>
    {
        public MyFloatingActionButtonRenderer(Context context) : base(context)
        {
        }

        public static void Initialize() { /* used only for helping ensure the renderer is not linked out */ }

        protected override void OnElementChanged(ElementChangedEventArgs<SuaveControls.Views.FloatingActionButton> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
                return;

            var fab = new FAB(Context);
            // set the bg
            Android.Support.V4.View.ViewCompat.SetBackgroundTintList(fab, ColorStateList.ValueOf(Element.ButtonColor.ToAndroid()));
            fab.UseCompatPadding = true;

            // set the icon
            var elementImage = Element.Image;
            var imageFile = elementImage?.File;

            if (imageFile != null)
            {
                fab.SetImageDrawable(Context.GetDrawable(imageFile));
            }
            fab.Click += Fab_Click;
            SetNativeControl(fab);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            Control.BringToFront();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var fab = (FAB)Control;
            if (e.PropertyName == nameof(Element.ButtonColor))
            {
                Android.Support.V4.View.ViewCompat.SetBackgroundTintList(fab, ColorStateList.ValueOf(Element.ButtonColor.ToAndroid()));
            }
            if (e.PropertyName == nameof(Element.Image))
            {
                var elementImage = Element.Image;
                var imageFile = elementImage?.File;

                if (imageFile != null)
                {
                    fab.SetImageDrawable(Context.GetDrawable(imageFile));
                }
            }
            base.OnElementPropertyChanged(sender, e);

        }

        private void Fab_Click(object sender, EventArgs e)
        {
            // proxy the click to the element
            ((IButtonController)Element).SendClicked();
        }
    }
}
