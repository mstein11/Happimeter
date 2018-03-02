using System;
using SQLite;

namespace Happimeter.Core.Database
{
    public class GenericQuestion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Question { get; set; }
        public string GenericQuestionGroupId { get; set; }
    }
}
