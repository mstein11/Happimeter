using System;
using SQLite;
namespace Happimeter.Core.Database
{
    public class TeamEntry
    {

        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }

        public double? Activation { get; set; }
        public double? Pleasance { get; set; }
    }
}
