using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Happimeter.Core.Database;
using SQLite;

namespace Happimeter.Watch.Droid.Database
{
    public class DatabaseContext : SharedDatabaseContext, IDatabaseContext
    {
        protected override string GetDatabasePath()
        {
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            return System.IO.Path.Combine(docsFolder, DatabaseName);
        }
        protected override List<Type> BeforeCreateDatabase(List<Type> tables)
        {
            //here we have our own version of that model
            tables.Remove(typeof(SharedBluetoothDevicePairing));
            tables.Add(typeof(BluetoothPairing));

            return tables;
        }

        /// <summary>
        ///     Here we can adjust the add logic for some of the types
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="entity">Entity.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override void Add<T>(T entity) {
            //we have some special logic for bluetooth pairings
            if (typeof(T) == typeof(BluetoothPairing)) {
                AddNewPairing(entity as BluetoothPairing);
            } else {
                base.Add(entity);
            }
        }

        private void AddNewPairing(BluetoothPairing newPairing) {
            try
            {
                EnsureDatabaseCreated();
                using (var connection = new SQLiteConnection(GetDatabasePath()))
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


        /// <summary>
        ///     Just for conviniece.
        /// </summary>
        /// <returns>The current bluetooth pairing.</returns>
        public BluetoothPairing GetCurrentBluetoothPairing() {
            try
            {
                return base.Get<BluetoothPairing>(x => x.IsPairingActive == true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}
