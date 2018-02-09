using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite;

namespace Happimeter.Watch.Droid.Database
{
    public class DatabaseContext : IDatabaseContext
    {
        private const string DatabaseName = "db_sqlnet.db";
        private readonly string DatabasePath;

        private bool DatabaseIsCreated = false;

        public DatabaseContext()
        {
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            DatabasePath  = System.IO.Path.Combine(docsFolder, DatabaseName);
        }

        public void CreateDatabase() {
            try
            {
                var connection = new SQLiteConnection(DatabasePath);
                connection.CreateTable<BluetoothPairing>();
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public IList<MicrophoneMeasurement> GetMicrophoneMeasurements() {
            if (!DatabaseIsCreated) {
                CreateDatabase();
            }
            using (var connection = new SQLiteConnection(DatabasePath)) {
                return connection.Table<MicrophoneMeasurement>().ToList(); 
            }
        }

        public bool AddMicrophoneMeasurement(MicrophoneMeasurement measurement) {
            try {
                if (!DatabaseIsCreated)
                {
                    CreateDatabase();
                }
                using (var connection = new SQLiteConnection(DatabasePath))
                {
                    connection.Insert(measurement);
                    return true;
                }
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public void DeleteAllMicrophoneMeasurements() {
            try
            {
                if (!DatabaseIsCreated)
                {
                    CreateDatabase();
                }
                using (var connection = new SQLiteConnection(DatabasePath))
                {
                    connection.DeleteAll<MicrophoneMeasurement>();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void AddNewPairing(BluetoothPairing newPairing) {
            try
            {
                if (!DatabaseIsCreated)
                {
                    CreateDatabase();
                }
                using (var connection = new SQLiteConnection(DatabasePath))
                {
                    var bluetoothPairings = connection.Table<BluetoothPairing>().Where(prop => prop.IsPairingActive).ToList();
                    for (int i = 0; i < bluetoothPairings.Count; i++)
                    {
                        bluetoothPairings[i].IsPairingActive = false;
                    }
                    connection.UpdateAll(bluetoothPairings);
                    connection.Insert(newPairing);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void DeleteAllBluetoothPairings() {
            try
            {
                if (!DatabaseIsCreated)
                {
                    CreateDatabase();
                }
                using (var connection = new SQLiteConnection(DatabasePath))
                {
                    connection.DeleteAll<BluetoothPairing>();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public BluetoothPairing GetCurrentBluetoothPairing() {
            try
            {
                if (!DatabaseIsCreated)
                {
                    CreateDatabase();
                }
                using (var connection = new SQLiteConnection(DatabasePath))
                {
                    return connection.Table<BluetoothPairing>().Where(prop => prop.IsPairingActive).FirstOrDefault();

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}
