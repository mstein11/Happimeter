using System;
using SQLite;

namespace Happimeter.Core.Database
{
    public class AudioFeatures
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public float Vad { get; set; }

        public override string ToString()
        {
            return string.Format("[MicrophoneMeasurement: ID={0}, TimeStamp={1}, VAD={2}]", Id, Timestamp, Vad);
        }
    }
}
