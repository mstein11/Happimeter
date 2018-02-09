using System;
using Happimeter.Core.Database;
using SQLite;

namespace Happimeter.Watch.Droid.Database
{
    public class BluetoothPairing : SharedBluetoothDevicePairing
    {
        public string PairedWithUserName { get; set; }
        public int PairedWithUserId { get; set; }

        public override string ToString()
        {
            return string.Format("[BluetoothPairing: ID={0}, PairedAt={1}, PairedWithUserName={2}, PairedDeviceName={3}, IsIosPaired={4}, IsPairingActive={5}, LastDataSync={6}]", Id, PairedAt, PairedDeviceName, PairedWithUserName, PhoneOs, IsPairingActive, LastDataSync);
        }
    }
}
