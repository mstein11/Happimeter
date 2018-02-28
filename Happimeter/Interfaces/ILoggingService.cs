using System.Collections.Generic;

namespace Happimeter.Interfaces
{
    public interface ILoggingService
    {
        void LogEvent(string name, Dictionary<string, string> data = null);
    }
}