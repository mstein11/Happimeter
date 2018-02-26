using System;
namespace Happimeter.Events
{
    public enum SyncronizeDataStates
    {
        UploadingMood,
        UploadingSensor,
        UploadingSuccessful,
        UploadingError
    }


    public class SynchronizeDataEventArgs : EventArgs
    {
        public SynchronizeDataEventArgs()
        {
        }
        public SyncronizeDataStates EventType { get; set; }

        public int EntriesSent { get; set; }

        public int TotalEntries { get; set; }

        public string ErrorMessage { get; set; }
    }

}
