using System;
namespace Happimeter.Core.Helper
{
    public static class UuidHelper
    {
        public const string BeaconUuidString = "6c2e3172-768b-4d3e-a47b-01470fe908a4";
        public static Guid BeaconUuid => Guid.Parse(BeaconUuidString);
    }
}
