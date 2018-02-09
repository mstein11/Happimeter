using System;
using SQLite;

namespace Happimeter.Core.Database
{
    public class SharedBluetoothDevicePairing
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime? PairedAt { get; set; }
        public string PairedDeviceName { get; set; }
        public string PhoneOs { get; set; }
        //The passphrase that will be used during the communication between watch and phone
        public string Password { get; set; }
        public bool IsPairingActive { get; set; }
        public DateTime? LastDataSync { get; set; }
    }
}
