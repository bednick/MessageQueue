using System;
using System.Threading;

namespace MessageQueue
{
    public class Message<T>
    {
        public T message { get; }
        internal IAsyncResult resultObj { set; get; }
        internal Exception exception { set; get; }

        internal Message(T message)
        {
            this.message = message;
            resultObj = null;
            exception = null;
        }

        public bool Start()
        {
            return resultObj != null;
        }

        public void Wait()
        {
            if (resultObj == null)
            {
                lock (this)
                {
                    while (resultObj == null)
                    {
                        Monitor.Wait(this);

                    }
                }
            }
            resultObj.AsyncWaitHandle.WaitOne();
        }

        public Exception Exception()
        {
            return exception;
        }
    }
}
