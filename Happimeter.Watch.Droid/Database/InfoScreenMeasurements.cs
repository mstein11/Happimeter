using System;
using SQLite;
using Java.Interop;
using System.Net.NetworkInformation;
namespace Happimeter.Watch.Droid.Database
{
	public class InfoScreenMeasurements
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public DateTime Timestamp { get; set; }
		public double Heartrate { get; set; }
		public int Steps { get; set; }
		public int CloseTo { get; set; }


	}
}
