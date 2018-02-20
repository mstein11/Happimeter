using System;
using System.Linq;
using Happimeter.Views;
using UIKit;
using Xamarin.Forms;

namespace Happimeter.iOS
{
    public partial class TabBarController : UITabBarController
    {
        public TabBarController(IntPtr handle) : base(handle)
        {
            var viewControllers = ViewControllers.ToList();
            viewControllers = viewControllers.Append(new SurveyPage().CreateViewController()).ToList();
            ViewControllers = viewControllers.ToArray();


            var tabBarItem = new UITabBarItem();
            tabBarItem.Title = "asdasd";
            TabBar.Items.Append(tabBarItem);

            TabBar.Items[0].Title = "Browse";
            TabBar.Items[1].Title = "About";
            TabBar.Items[2].Title = "Bluetooth";
            //TabBar.Items[3].Title = "asd";

        }
    }
}
