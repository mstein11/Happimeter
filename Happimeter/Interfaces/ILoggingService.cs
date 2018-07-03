using System.Collections.Generic;
using System;

namespace Happimeter.Interfaces
{
	public interface ILoggingService
	{
		void LogEvent(string name, Dictionary<string, string> data = null);
		void LogException(Exception exception, Dictionary<string, string> data = null);
	}
}