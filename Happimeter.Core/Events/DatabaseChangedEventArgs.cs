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
        public List<object> Entites { get; set; }
        public Type TypeOfEnties { get; set; }
    }
}
