using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Happimeter.Core.Database
{
    public interface ISharedDatabaseContext
    {
        void Add<T>(T entity) where T : new();
        void Update<T>(T entity) where T : new();
        void CreateDatabase();
        void DeleteAll<T>() where T : new();
        T Get<T>(Expression<Func<T, bool>> whereClause) where T : new();
        List<T> GetAll<T>(Expression<Func<T, bool>> whereClause = null) where T : new();
        event EventHandler ModelChanged;
    }
}