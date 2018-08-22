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

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
