using System;
namespace Happimeter.Core.Helper
{
    public static class UuidHelper
    {
        public const string BeaconUuidString = "6c2e3172-768b-4d3e-a47b-01470fe908a4";
        public static Guid BeaconUuid => Guid.Parse(BeaconUuidString);

        public const byte BeaconManufacturerId = 0x004C;
        public const int TxPowerLevel = -56;
        //Layout for iBeacon
        public const string BeaconLayout = "m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24";
    }
}
