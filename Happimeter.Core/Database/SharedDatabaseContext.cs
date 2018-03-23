using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Happimeter.Core.Database.ModelsNotMappedToDb;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Happimeter.Core.Events;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace Happimeter.Core.Database
{
    public class SharedDatabaseContext : ISharedDatabaseContext
    {
        protected string DatabaseName = "db_sqlnet.db";
        protected object SyncLock = new object();
        private bool DatabaseCreated = false;

        protected Subject<DatabaseChangedEventArgs> DatabaseEntriesChangedSubject = new Subject<DatabaseChangedEventArgs>();

        public SharedDatabaseContext()
        {
        }

        protected virtual string GetDatabasePath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DatabaseName);
        }

        protected virtual SQLiteConnection GetConnection() {
            return new SQLiteConnection(GetDatabasePath());
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

        private List<Type> GetDatabaseTables() {
            var databaseTables = new List<Type>();
            databaseTables.Add(typeof(MicrophoneMeasurement));
            databaseTables.Add(typeof(SharedBluetoothDevicePairing));
            databaseTables.Add(typeof(SurveyMeasurement));
            databaseTables.Add(typeof(SurveyItemMeasurement));
            databaseTables.Add(typeof(SensorMeasurement));
            databaseTables.Add(typeof(SensorItemMeasurement));
            databaseTables.Add(typeof(ConfigEntry));
            databaseTables.Add(typeof(GenericQuestion));

            //here we give the possibility to alter the list of tables created by subprojects (e.g. different devices)
            databaseTables = BeforeCreateDatabase(databaseTables);

            return databaseTables;
        }

        public virtual void CreateDatabase() {
            var databasePath = GetDatabasePath();
            try
            {
                //here we add all the tables that all projects share
                var databaseTables = GetDatabaseTables();

                lock (SyncLock)
                {
                    using (var connection = GetConnection())
                    {
                        foreach (var table in databaseTables)
                        {
                            connection.CreateTable(table);
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public virtual void ResetDatabase()
        {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {                    
                    DeleteAll<ConfigEntry>();
                    DeleteAll<MicrophoneMeasurement>();
                    DeleteAll<SensorItemMeasurement>();
                    DeleteAll<SensorMeasurement>();
                    DeleteAll<SharedBluetoothDevicePairing>();
                    DeleteAll<SurveyItemMeasurement>();
                    DeleteAll<SurveyMeasurement>();
                }
            }
        }

        public virtual List<T> GetAll<T>(Expression<Func<T, bool>> whereClause = null) where T: new() {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    if (whereClause == null)
                    {
                        return connection.Table<T>().ToList();
                    }

                    return connection.Table<T>().Where(whereClause).ToList();
                }
            }
        }

        public virtual List<T> GetAllWithChildren<T>(Expression<Func<T, bool>> whereClause = null) where T : new()
        {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    return connection.GetAllWithChildren<T>(whereClause, true).ToList();
                }
            }
        }

        /// <summary>
        ///     The methods below is a little bit faster in quering the data from db. Thats why we use it. 
        ///     The best way to query the data would be to not rely on reflection for the object mapping.
        ///     We might want to implement that at some point.
        /// </summary>
        /// <returns>The sensor measurements.</returns>
        /// <param name="skip">Skip.</param>
        /// <param name="take">Take.</param>
        /// <param name="orderDesc">If true, we will order descending before applying skip and take, if false we will order ascending</param>
        public virtual List<SensorMeasurement> GetSensorMeasurements(int skip = 0, int take = 150, bool orderDesc = false) {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var orderString = orderDesc ? "DESC" : ""; 
                    var items =  connection.Query<SensorMeasurementsAndItems>($"SELECT * FROM (SELECT * FROM SensorMeasurement as s1 ORDER BY Timestamp {orderString} LIMIT {take} OFFSET {skip}) as s1, SensorItemMeasurement as s2 WHERE s1.Id == s2.SensorMeasurementId").ToList();
                    sw.Stop();
                    Debug.WriteLine($"Took {sw.ElapsedMilliseconds} milliseconds to read sensordata");
                    return items.GroupBy(group => new {group.SensorMeasurementId, group.Timestamp, group.IsUploadedToServer})
                         .Select(x => new SensorMeasurement{
                        Id = x.Key.SensorMeasurementId,
                        Timestamp = x.Key.Timestamp,
                        IsUploadedToServer = x.Key.IsUploadedToServer,
                        SensorItemMeasures = x.Select(item => {
                            return new SensorItemMeasurement
                            {
                                Average = item.Average,
                                Max = item.Max,
                                Min = item.Min,
                                Magnitude = item.Magnitude,
                                NumberOfMeasures = item.NumberOfMeasures,
                                Quantile1 = item.Quantile1,
                                Quantile2 = item.Quantile2,
                                Quantile3 = item.Quantile3,
                                StdDev = item.StdDev,
                                Type = item.Type,
                                SensorMeasurementId = item.SensorMeasurementId
                            };
                        }).ToList()
                    }).ToList();
                }
            }
        }

        public virtual T Get<T>(Expression<Func<T,bool>> whereClause) where T : new()
        {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    return connection.Table<T>().Where(whereClause).FirstOrDefault();
                }
            }
        }

        public virtual T GetWithChildren<T>(Expression<Func<T, bool>> whereClause) where T : new()
        {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    return connection.GetWithChildren<T>(whereClause, true);
                }
            }
        }

        public virtual void Add<T>(T entity) where T : new() {
            var btPairing = entity as SharedBluetoothDevicePairing;
            if (btPairing != null) {
                AddBluetoothPairing(btPairing);
                return;
            }
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    connection.Insert(entity);
                }
            }
            DatabaseEntriesChangedSubject.OnNext(new DatabaseChangedEventArgs(entity, typeof(T), DatabaseChangedEventTypes.Added));
        }

        public virtual void AddGraph<T>(T entity) where T : new()
        {
            var btPairing = entity as SharedBluetoothDevicePairing;
            if (btPairing != null)
            {
                AddBluetoothPairing(btPairing);
                return;
            }
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    connection.InsertWithChildren(entity, true);
                }
            }
            DatabaseEntriesChangedSubject.OnNext(new DatabaseChangedEventArgs(entity, typeof(T), DatabaseChangedEventTypes.Added));
        }

        public virtual void Update<T>(T entity) where T : new()
        {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    connection.Update(entity);
                }
            }
            DatabaseEntriesChangedSubject.OnNext(new DatabaseChangedEventArgs(entity, typeof(T), DatabaseChangedEventTypes.Updated));
        }

        public virtual void DeleteAll<T>() where T : new() {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    connection.DeleteAll<T>();
                }
            }
        }

        public virtual void Delete<T>(T entity) where T : new() {
            EnsureDatabaseCreated();
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    connection.Delete(entity);
                }
            }
            DatabaseEntriesChangedSubject.OnNext(new DatabaseChangedEventArgs(entity, entity.GetType(), DatabaseChangedEventTypes.Deleted));
        }

        protected void AddBluetoothPairing(SharedBluetoothDevicePairing pairing) {
            EnsureDatabaseCreated();
            var oldPairings = GetAll<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
            foreach (var oldPairing in oldPairings) {
                oldPairing.IsPairingActive = false;
                Update(oldPairing);
            }
            if (oldPairings.Any()) {
                DatabaseEntriesChangedSubject.OnNext(new DatabaseChangedEventArgs {
                    Entites = oldPairings.ToList<object>(),
                    TypeOfEnties = oldPairings.FirstOrDefault().GetType(),
                    TypeOfEvent = DatabaseChangedEventTypes.Updated
                });
            }
            lock (SyncLock)
            {
                using (var connection = GetConnection())
                {
                    connection.Insert(pairing);
                }
            }
            DatabaseEntriesChangedSubject.OnNext(new DatabaseChangedEventArgs(pairing, pairing.GetType(), DatabaseChangedEventTypes.Added));
        }


        public IObservable<DatabaseChangedEventArgs> WhenEntryAdded()
        {
            return DatabaseEntriesChangedSubject.Where(x => x.TypeOfEvent == DatabaseChangedEventTypes.Added);
        }
        public IObservable<DatabaseChangedEventArgs> WhenEntryAdded<T>()
        {
            return DatabaseEntriesChangedSubject.Where(x => x.TypeOfEvent == DatabaseChangedEventTypes.Added && x.TypeOfEnties == typeof(T));
        }
        public IObservable<DatabaseChangedEventArgs> WhenEntryUpdated()
        {
            return DatabaseEntriesChangedSubject.Where(x => x.TypeOfEvent == DatabaseChangedEventTypes.Updated);
        }
        public IObservable<DatabaseChangedEventArgs> WhenEntryUpdated<T>()
        {
            return DatabaseEntriesChangedSubject.Where(x => x.TypeOfEvent == DatabaseChangedEventTypes.Updated && x.TypeOfEnties == typeof(T));
        }
        public IObservable<DatabaseChangedEventArgs> WhenEntryDeleted()
        {
            return DatabaseEntriesChangedSubject.Where(x => x.TypeOfEvent == DatabaseChangedEventTypes.Deleted);
        }
        public IObservable<DatabaseChangedEventArgs> WhenEntryDeleted<T>()
        {
            return DatabaseEntriesChangedSubject.Where(x => (x.TypeOfEvent == DatabaseChangedEventTypes.Deleted || x.TypeOfEvent == DatabaseChangedEventTypes.DeleteAll) && x.TypeOfEnties == typeof(T));
        }
        public IObservable<DatabaseChangedEventArgs> WhenEntryChanged<T>()
        {
            return DatabaseEntriesChangedSubject.Where(x => x.TypeOfEnties == typeof(T));
        }
        public IObservable<DatabaseChangedEventArgs> WhenEntryChanged()
        {
            return DatabaseEntriesChangedSubject;
        }
    }
}
