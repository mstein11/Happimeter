using System;
using SQLite;

namespace Happimeter.Watch.Droid.Database
{
    public class MicrophoneMeasurement
    {
        public MicrophoneMeasurement()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public double Volumne { get; set; }

        public override string ToString()
        {
            return string.Format("[MicrophoneMeasurement: ID={0}, TimeStamp={1}, Volumne={2}]", Id, TimeStamp, Volumne);
        }
    }
}
