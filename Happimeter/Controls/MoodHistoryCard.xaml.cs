using Xamarin.Forms;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Controls
{
    public partial class MoodHistoryCard : ContentView
    {
        public MoodHistoryCard()
        {
            InitializeComponent();
            var viewModel = new MoodHistoryCardViewModel();
            BindingContext = viewModel;
        }
    }
}
