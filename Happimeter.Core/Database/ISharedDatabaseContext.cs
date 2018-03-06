using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Happimeter.Core.Events;

namespace Happimeter.Core.Database
{
    public interface ISharedDatabaseContext
    {
        void CreateDatabase();

        void Add<T>(T entity) where T : new();
        void AddGraph<T>(T entity)where T : new();
        void Update<T>(T entity) where T : new();
        void Delete<T>(T entity) where T : new();
        void DeleteAll<T>() where T : new();

        void ResetDatabase();

        T GetWithChildren<T>(Expression<Func<T, bool>> whereClause) where T : new();
        T Get<T>(Expression<Func<T, bool>> whereClause) where T : new();
        List<T> GetAll<T>(Expression<Func<T, bool>> whereClause = null) where T : new();
        List<T> GetAllWithChildren<T>(Expression<Func<T, bool>> whereClause = null) where T : new();
        List<SensorMeasurement> GetSensorMeasurements(int skip = 0, int take = 150, bool orderDesc = false);

        IObservable<DatabaseChangedEventArgs> WhenEntryAdded();
        IObservable<DatabaseChangedEventArgs> WhenEntryAdded<T>();
        IObservable<DatabaseChangedEventArgs> WhenEntryUpdated();
        IObservable<DatabaseChangedEventArgs> WhenEntryUpdated<T>();
        IObservable<DatabaseChangedEventArgs> WhenEntryDeleted();
        IObservable<DatabaseChangedEventArgs> WhenEntryDeleted<T>();
        IObservable<DatabaseChangedEventArgs> WhenEntryChanged<T>();
        IObservable<DatabaseChangedEventArgs> WhenEntryChanged();
    }
}