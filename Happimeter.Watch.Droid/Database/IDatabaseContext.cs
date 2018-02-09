using System.Collections.Generic;
using Happimeter.Core.Database;

namespace Happimeter.Watch.Droid.Database
{
    public interface IDatabaseContext : ISharedDatabaseContext
    {
        BluetoothPairing GetCurrentBluetoothPairing();
    }
}