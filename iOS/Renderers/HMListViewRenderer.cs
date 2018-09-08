using System;
using Happimeter.Controls;
using Happimeter.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
[assembly: ExportRenderer(typeof(HMListView), typeof(HMListViewRenderer))]
namespace Happimeter.iOS.Renderers
{
    public class HMListViewRenderer : ListViewRenderer
    {
        public HMListViewRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (Element is HMListView formsListView && !formsListView.RowsSelectable)
            {
                Control.AllowsSelection = false;
            }

        }
    }
}
