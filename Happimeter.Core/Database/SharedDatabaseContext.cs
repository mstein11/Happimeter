using System;
using System.Collections.Generic;
using SQLite;

namespace Happimeter.Core.Database
{
    public class SharedDatabaseContext
    {
        private string DatabasePath = "db_sqlnet.db";


        public SharedDatabaseContext()
        {
        }

        protected virtual string GetDatabasePath() {
            return DatabasePath;
        }

        protected virtual SQLiteConnection GetConnection() {
            return new SQLiteConnection(DatabasePath);
        }

        /// <summary>
        ///     Override in child classes to change the tables that are getting created.
        /// </summary>
        /// <returns>The create database.</returns>
        /// <param name="tables">Tables.</param>
        protected virtual List<Type> BeforeCreateDatabase(List<Type> tables) {
            return tables;
        }

        public virtual void CreateDatabase() {
            var databasePath = GetDatabasePath();
            try
            {
                //here we add all the tables that all projects share
                var databaseTables = new List<Type>();
                databaseTables.Add(typeof(MicrophoneMeasurement));

                //here we give the possibility to alter the list of tables created by subprojects (e.g. different devices)
                databaseTables = BeforeCreateDatabase(databaseTables);

                using (var connection = GetConnection()) {
                    foreach (var table in databaseTables) {
                        connection.CreateTable(table);
                    }
                }
                //connection.CreateTable<MicrophoneMeasurement>();
                //connection.CreateTable<BluetoothPairing>();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
