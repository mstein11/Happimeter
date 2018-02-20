using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Happimeter.iOS
{
    public partial class SurveyViewController : UINavigationController
    {
        public SurveyViewController (IntPtr handle) : base (handle)
        {
            var formsPage = new InitializeSurveyView();
            var startSurveyVc = formsPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = formsPage.Title;

            formsPage.StartSurveyClickedEvent += (sender, e) => {
                var surveyPage = new SurveyPage();
                var surveyPageVc = surveyPage.CreateViewController();
                surveyPageVc.Title = surveyPage.Title;
                PushViewController(surveyPageVc, true);

                surveyPage.FinishedSurvey += async (sender1, e1) => {
                    var finalizeSurveyPage = new FinalizeSurveyPage();
                    var finalizeSurveyPageVc = new FinalizeSurveyPage().CreateViewController();

                    finalizeSurveyPageVc.Title = finalizeSurveyPage.Title;
                    PushViewController(finalizeSurveyPageVc, true);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    PopToRootViewController(true);
                };



            };
            /*AddChildViewController(startSurveyVc);
            View.Add(startSurveyVc.View);
            startSurveyVc.DidMoveToParentViewController(this);
            EdgesForExtendedLayout = UIRectEdge.None;*/

        }
    }
}