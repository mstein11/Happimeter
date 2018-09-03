using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Happimeter.Core.Database
{
    public class GenericQuestion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string QuestionShort { get; set; }

        public bool Deactivated { get; set; }

        public int? UserId { get; set; }

        //We don't have the option to give a default value in the database. So we use Deactivated as datastore (which will default to false)
        [Ignore]
        public bool Activated
        {
            get
            {
                return !Deactivated;
            }
            set
            {
                Deactivated = !value;
            }

        }
    }
}
