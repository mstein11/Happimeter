using System;
using System.Collections.Generic;

namespace Happimeter.Core.Events
{
    public class DatabaseChangedEventArgs : EventArgs
    {
        public DatabaseChangedEventArgs()
        {
            Entites = new List<object>();
        }
        public DatabaseChangedEventArgs(object entity, Type typeOfEntry, DatabaseChangedEventTypes eventTypes) : this(new List<object>{entity}, typeOfEntry, eventTypes)
        {
        }
        public DatabaseChangedEventArgs(List<object> entities, Type typeOfEntry, DatabaseChangedEventTypes eventTypes)
        {
            Entites = entities;
            TypeOfEnties = typeOfEntry;
            TypeOfEvent = eventTypes;
        }

        public List<object> Entites { get; set; }
        public Type TypeOfEnties { get; set; }
        public DatabaseChangedEventTypes TypeOfEvent { get; set; }
    }

    public enum DatabaseChangedEventTypes {
        Added,
        Updated,
        Deleted,
        DeleteAll
    }
}
