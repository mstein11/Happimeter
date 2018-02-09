using System;
using SQLite;

namespace Happimeter.Watch.Droid.Database
{
    public class BluetoothPairing
    {
        public BluetoothPairing()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime? PairedAt { get; set; }
        public string PairedWithUserName { get; set; }
        public int PairedWithUserId { get; set; }
        public string PairedDeviceName { get; set; }
        public string PhoneOs { get; set; }

        //The passphrase that will be used during the communication between watch and phone
        public string Password { get; set; }

        public bool IsPairingActive { get; set; }
        public DateTime? LastDataSync { get; set; }

        public override string ToString()
        {
            return string.Format("[BluetoothPairing: ID={0}, PairedAt={1}, PairedWithUserName={2}, PairedDeviceName={3}, IsIosPaired={4}, IsPairingActive={5}, LastDataSync={6}]", Id, PairedAt, PairedDeviceName, PairedWithUserName, IsIosPaired, IsPairingActive, LastDataSync);
        }
    }
}
