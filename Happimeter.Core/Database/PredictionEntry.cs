using System;
using SQLite;

namespace Happimeter.Core.Database
{
    public class PredictionEntry
    {
        public PredictionEntry()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public DateTime Timestamp { get; set; }

        public int PredictedValue { get; set; }
    }
}
