﻿using System;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.DependencyInjection;
using Happimeter.Interfaces;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Happimeter
{
    public class App
    {
        public static bool UseMockDataStore = true;
        public static string BackendUrl = "http://localhost:5000";

        public static void Initialize()
        {
            if (UseMockDataStore)
                ServiceLocator.Instance.Register<IDataStore<Item>, MockDataStore>();
            else
                ServiceLocator.Instance.Register<IDataStore<Item>, CloudDataStore>();

            Container.RegisterElements();
            var sharedDb = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var pairing = sharedDb.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
            if (pairing != null) {
                ServiceLocator.Instance.Get<IBeaconWakeupService>().StartWakeupForBeacon();
            }
        }
    }
}
