using System;
namespace Happimeter.Watch.Droid.Workers
{
    public abstract class AbstractWorker
    {
        public AbstractWorker()
        {
        }

        public abstract void Start();
        public abstract void Stop();
        public bool IsRunning { get; protected set; }


    }
}
