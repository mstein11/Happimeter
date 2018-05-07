using System;
using Newtonsoft.Json;

namespace Happimeter.Models.ServiceModels
{
    public class PostSensorDataServiceModel
    {
        public PostSensorDataServiceModel()
        {
        }

        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty("timestamp")]
        public decimal Timestamp { get; set; }
        [JsonProperty("local_timestamp")]
        public decimal LocalTimestamp { get; set; }
        [JsonProperty("avg_bpm")]
        public double AvgHeartrate { get; set; }
        [JsonProperty("avg_light_level")]
        public double AvgLightlevel { get; set; }
        [JsonProperty("activity")]
        public double Activity { get; set; }
        [JsonProperty("position")]
        public PositionModel Position { get; set; }
        [JsonProperty("accelerometer")]
        public AccelerometerModel Accelerometer { get; set; }
        [JsonProperty("vmc")]
        public double Vmc { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
    }

    public class PositionModel
    {
        [JsonProperty("lat")]
        public double? Latitude { get; set; }
        [JsonProperty("lon")]
        public double? Longitude { get; set; }
        [JsonProperty("alt")]
        public double? Altitude { get; set; }
    }
    public class AccelerometerModel
    {
        [JsonProperty("avg_x")]
        public double AvgX { get; set; }
        [JsonProperty("avg_y")]
        public double AvgY { get; set; }
        [JsonProperty("avg_z")]
        public double AvgZ { get; set; }
        [JsonProperty("var_x")]
        public double VarX { get; set; }
        [JsonProperty("var_y")]
        public double VarY { get; set; }
        [JsonProperty("var_z")]
        public double VarZ { get; set; }
    }
}