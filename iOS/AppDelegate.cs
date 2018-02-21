using System;
using CoreLocation;
using Foundation;
using Happimeter.DependencyInjection;
using Happimeter.Interfaces;
using Plugin.BluetoothLE;
using UIKit;
using Xamarin.Forms;

namespace Happimeter.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            //we need this or our chartview are not working. Probably an issue with the linker
            var ignore_chartView = new Microcharts.Forms.ChartView();

            Happimeter.iOS.DependencyInjection.Container.RegisterElements();
            Forms.Init();
            App.Initialize();
            CrossBleAdapter.Init(BleAdapterConfiguration.DefaultBackgroudingConfig);

            var store = ServiceLocator.Instance.Get<IAccountStoreService>();
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            if (store.IsAuthenticated()) {
                
                UIStoryboard board = UIStoryboard.FromName("Main", null);
                UIViewController ctrl = (UIViewController)board.InstantiateViewController("tabViewController");
                ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
                Window.RootViewController = ctrl;
                Window.MakeKeyAndVisible();
            } else {
                UIStoryboard board = UIStoryboard.FromName("Login", null);
                UIViewController ctrl = (UIViewController)board.InstantiateViewController("LoginViewController");
                ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
                Window.RootViewController = ctrl;
                Window.MakeKeyAndVisible();
                //application.KeyWindow.RootViewController = ctrl;
    
            }



            return true;
        }


        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
            Console.WriteLine("ON RESIGN ACTIVATION");

        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
            Console.WriteLine("DID ENTER BACKGROUND");
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
            Console.WriteLine("WILL ENTER FOREGROUND");
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.

            Console.WriteLine("ON ACTIVATED");
        }

        public override void WillTerminate(UIApplication application)
        {
            Console.WriteLine("WILL TERMINATE");
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}
