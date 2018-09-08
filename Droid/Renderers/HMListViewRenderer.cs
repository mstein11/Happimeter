using Xamarin.Forms.Platform.Android;
using Android.Content;
using Xamarin.Forms;
using Android.Graphics.Drawables;
using Happimeter.Controls;
using Happimeter.Droid.Renderers;
[assembly: ExportRenderer(typeof(HMListView), typeof(HMListViewRenderer))]
namespace Happimeter.Droid.Renderers
{
    public class HMListViewRenderer : ListViewRenderer
    {
        public HMListViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (Element is HMListView formsListView && !formsListView.RowsSelectable)
            {
                Control.Selector = new StateListDrawable();
            }
        }
    }
}
