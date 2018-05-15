using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Happimeter.Models.ServiceModels
{
	public class PostMoodServiceModel
	{
		[JsonIgnore]
		public int Id { get; set; }

		[JsonProperty("timestamp")]
		public decimal Timestamp { get; set; }
		[JsonProperty("local_timestamp")]
		public decimal LocalTimestamp { get; set; }
		[JsonProperty("pleasance")]
		public int Pleasance { get; set; }
		[JsonProperty("activation")]
		public int Activation { get; set; }
		[JsonProperty("position")]
		public PostMoodServicePositionModel Position { get; set; }
		[JsonProperty("mood_answers")]
		public Dictionary<int, object> MoodAnswers { get; set; }

		[JsonProperty("generic_question_group")]
		public string GenericQuestionGroup { get; set; }
		[JsonProperty("generic_question_count")]
		public int GenericQuestionCount { get; set; }
		[JsonProperty("generic_values")]
		public int[] GenericQuestionValues { get; set; }
		[JsonProperty("has_new_format")]
		public bool HasNewFormat { get; set; } = true;

		[JsonProperty("device_id")]
		public string DeviceId { get; set; }
	}

	public class PostMoodServicePositionModel
	{
		public PostMoodServicePositionModel()
		{
		}

		public PostMoodServicePositionModel(double lat, double lon)
		{
			Lat = lat;
			Lon = lon;
		}
		[JsonProperty("lat")]
		public double Lat { get; set; }
		[JsonProperty("lon")]
		public double Lon { get; set; }
	}
}
