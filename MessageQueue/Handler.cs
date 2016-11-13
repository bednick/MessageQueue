using System;
using System.Collections.Generic;
using System.Threading;

namespace MessageQueue
{
    internal class Handler<T>
    {
        private Action<T> handler;
        internal Queue<Message<T>> queue { get; }
        private bool reentrant;
        private bool working;

        internal bool Working { get { return working; } }

        internal bool Reentrant { get { return reentrant; } }

        internal Handler(Action<T> handler, Queue<Message<T>> queue, bool reentrant)
        {
            this.handler = handler;
            this.queue = queue;
            this.reentrant = reentrant;
            this.working = false;
        }

        internal void AsyncHandle(Message<T> message)
        {
            working = true;
            Action<Message<T>> asyncHandler = delegate (Message<T> mes) {
                try
                {
                    handler(mes.message);
                }
                catch (Exception e)
                {
                    mes.exception = e;
                }
            };
            message.resultObj = (asyncHandler.BeginInvoke(message, AsyncCompleted, null));
            lock (message)
            {
                Monitor.PulseAll(message);
            }
        }

        private void AsyncCompleted(IAsyncResult resObj)
        {
            lock (queue)
            {
                if (queue.Count != 0)
                {
                    AsyncHandle(queue.Dequeue());
                }
                else
                {
                    working = false;
                }
            }
        }

        internal bool Equals(Handler<T> obj)
        {
            return handler == obj.handler;
        }
    }
}
