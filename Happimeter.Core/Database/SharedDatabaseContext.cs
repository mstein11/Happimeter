using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite;

namespace Happimeter.Core.Database
{
    public class SharedDatabaseContext : ISharedDatabaseContext
    {
        protected string DatabaseName = "db_sqlnet.db";
        private bool DatabaseCreated = false;

        public SharedDatabaseContext()
        {
        }

        protected virtual string GetDatabasePath() {
            return DatabaseName;
        }

        protected virtual SQLiteConnection GetConnection() {
            return new SQLiteConnection(DatabaseName);
        }

        /// <summary>
        ///     Override in child classes to change the tables that are getting created.
        /// </summary>
        /// <returns>The create database.</returns>
        /// <param name="tables">Tables.</param>
        protected virtual List<Type> BeforeCreateDatabase(List<Type> tables) {
            return tables;
        }

        protected virtual void EnsureDatabaseCreated() {
            if (!DatabaseCreated) {
                CreateDatabase();
            }
        }

        public virtual void CreateDatabase() {
            var databasePath = GetDatabasePath();
            try
            {
                //here we add all the tables that all projects share
                var databaseTables = new List<Type>();
                databaseTables.Add(typeof(MicrophoneMeasurement));
                databaseTables.Add(typeof(SharedBluetoothDevicePairing));

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

        public virtual List<T> GetAll<T>(Expression<Func<T, bool>> whereClause = null) where T: new() {
            EnsureDatabaseCreated();
            using (var connection = GetConnection()) {
                if (whereClause == null) {
                    return connection.Table<T>().ToList(); 
                }

                return connection.Table<T>().Where(whereClause).ToList();
            }
        }

        public virtual T Get<T>(Expression<Func<T,bool>> whereClause) where T : new()
        {
            EnsureDatabaseCreated();
            using (var connection = GetConnection())
            {
                return connection.Table<T>().Where(whereClause).FirstOrDefault();
            }
        }

        public virtual void Add<T>(T entity) where T : new() {
            EnsureDatabaseCreated();
            using(var connection = GetConnection()) {
                connection.Insert(entity);
            }
        }

        public virtual void DeleteAll<T>() where T : new() {
            EnsureDatabaseCreated();
            using (var connection = GetConnection())
            {
                connection.DeleteAll<T>();
            }
        }
    }
}
