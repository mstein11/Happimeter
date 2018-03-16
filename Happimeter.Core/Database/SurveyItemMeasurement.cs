using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Happimeter.Core.Database
{
    public class SurveyItemMeasurement
    {
        public SurveyItemMeasurement()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public string Question { get; set; }
        public int Answer { get; set; }
        public int AnswerDisplay { get; set; }

        [ForeignKey(typeof(SurveyMeasurement))]
        public int SurveyMeasurementId { get; set; }

        [ManyToOne]
        public SurveyMeasurement SurveyMeasurement { get; set; }
    }
}
