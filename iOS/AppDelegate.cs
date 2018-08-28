using System;
using CoreLocation;
using Foundation;
using Happimeter.Core.Helper;
using Happimeter.DependencyInjection;
using Happimeter.Interfaces;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.BluetoothLE;
using UIKit;
using Xamarin.Forms;
using Xfx;
using Happimeter.Core.Services;
using UserNotifications;
using Happimeter.Views;

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
            Xamarin.FormsGoogleMaps.Init("AIzaSyC9DzMXj6s1rwu3UK70iNLJ4VC32xEtREs");
            AppCenter.Start("3119c95f-ca17-4e2d-9ae0-46c5382633f8",
                   typeof(Analytics), typeof(Crashes));


            Happimeter.iOS.DependencyInjection.Container.RegisterElements();
            XfxControls.Init();
            Forms.Init();
            App.Initialize();
            CrossBleAdapter.Init(BleAdapterConfiguration.DefaultBackgroudingConfig);
            ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DebugSnapshot);
            var store = ServiceLocator.Instance.Get<IAccountStoreService>();
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            if (store.IsAuthenticated())
            {
                UIStoryboard board = UIStoryboard.FromName("Main", null);
                UIViewController ctrl = (UIViewController)board.InstantiateViewController("tabViewController");
                ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
                Window.RootViewController = ctrl;
                Window.MakeKeyAndVisible();
            }
            else
            {
                //UIStoryboard board = UIStoryboard.FromName("Main", null);
                //UIViewController ctrl = (UIViewController)board.InstantiateViewController("SignInViewController");
                //ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
                var formsPage = new SignInPage();
                var formsPageVc = formsPage.CreateViewController();
                Window.RootViewController = formsPageVc;
                Window.MakeKeyAndVisible();
                //application.KeyWindow.RootViewController = ctrl;

            }


            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                        UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                        (approved, error) => { });

                UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                var settings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
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
            App.AppResumed();
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

    public class UserNotificationCenterDelegate : UNUserNotificationCenterDelegate
    {
        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            // Tell system to display the notification anyway or use
            // `None` to say we have handled the display locally.
            completionHandler(UNNotificationPresentationOptions.Alert);
        }
    }
}
