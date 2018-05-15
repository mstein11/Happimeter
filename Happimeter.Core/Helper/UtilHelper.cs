using System;

namespace Happimeter.Core.Helper
{
	public static class UtilHelper
	{
		/// <summary>
		/// Gets the major minor from user identifier.
		/// if userId higher than 65535, than we increment major by one.
		/// </summary>
		/// <returns>The major minor from user identifier.</returns>
		/// <param name="userId">User identifier.</param>
		public static Tuple<int, int> GetMajorMinorFromUserId(int userId)
		{
			int major = userId / 65535;
			int minor = userId % 65535;

			return new Tuple<int, int>(major, minor);
		}

		public static int GetUserIdFromMajorMinor(int major, int minor)
		{
			return 65535 * major + minor;
		}

		public static string GetNewScaleFromOldAsString(int old, int questionType)
		{
			if (old == 0)
			{
				var prefix = "LOW";
				if (questionType == 2)
				{
					prefix = "UNHAPPY";
				}
				else if (questionType == 1)
				{
					prefix = "INACTIVE";
				}
				return $"{prefix} (1 - 3)";
			}
			else if (old == 1)
			{
				var prefix = "NEUTRAL";
				return $"{prefix} (4 - 6)";
			}
			else
			{
				var prefix = "HIGH";
				if (questionType == 2)
				{
					prefix = "HAPPY";
				}
				else if (questionType == 1)
				{
					prefix = "ACTIVE";
				}
				return $"{prefix} (7 - 9)";
			}
		}

		public static string TimeAgo(DateTime dateTime)
		{
			string result = string.Empty;
			var timeSpan = DateTime.UtcNow.Subtract(dateTime);

			if (timeSpan <= TimeSpan.FromSeconds(60))
			{
				result = string.Format("{0} seconds ago", timeSpan.Seconds);
			}
			else if (timeSpan <= TimeSpan.FromMinutes(60))
			{
				result = timeSpan.Minutes > 1 ?
					String.Format("about {0} minutes ago", timeSpan.Minutes) :
					"about a minute ago";
			}
			else if (timeSpan <= TimeSpan.FromHours(24))
			{
				result = timeSpan.Hours > 1 ?
					String.Format("about {0} hours ago", timeSpan.Hours) :
					"about an hour ago";
			}
			else if (timeSpan <= TimeSpan.FromDays(30))
			{
				result = timeSpan.Days > 1 ?
					String.Format("about {0} days ago", timeSpan.Days) :
					"yesterday";
			}
			else if (timeSpan <= TimeSpan.FromDays(365))
			{
				result = timeSpan.Days > 30 ?
					String.Format("about {0} months ago", timeSpan.Days / 30) :
					"about a month ago";
			}
			else
			{
				result = timeSpan.Days > 365 ?
					String.Format("about {0} years ago", timeSpan.Days / 365) :
					"about a year ago";
			}

			return result;
		}

		/// <summary>
		///     Calculates the next time we poll the user.
		///     Two hours later than right now +- 60 minutes based on a random number.
		///     If between 10pm and 10 am, we set next time to 10am
		/// </summary>
		/// <returns>The next survey prompt time.</returns>
		public static DateTime GetNextSurveyPromptTime()
		{
			var random = new Random();
			var randNumber = random.Next(0, 120) - 60;
			var nextTime = DateTime.UtcNow.ToLocalTime().AddMinutes(120 + randNumber);


			if (nextTime.Hour < 10)
			{
				nextTime = nextTime.ToLocalTime().Date.AddHours(10);
			}
			else if (nextTime.Hour >= 22)
			{
				nextTime = nextTime.ToLocalTime().Date.AddDays(1).AddHours(10);
			}

			return nextTime;
		}

		public static int GetUnixTimestamp(DateTime datetime)
		{
			return (Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		}
	}
}
