using System;
using System.Collections.Generic;
using System.Threading;

namespace MessageQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageQueue<string> messageQueue = new MessageQueue<string>();
            messageQueue.RegisterHandler("dhisakdjask1", Print, false);
            MessageQueue<string>.Message<string> status1 = messageQueue.Engueue("dhisakdjask1");
            MessageQueue<string>.Message<string> status2 = messageQueue.Engueue("dhisakdjask2");
            MessageQueue<string>.Message<string> status3 = messageQueue.Engueue("dhisakdjask3");
            status1.Wait();
            status2.Wait();
            status3.Wait();
        }
        
        static void Print(string str)
        {
            Thread.Sleep(3000);
            Console.WriteLine(str);
            
        }
    }

    class MessageQueue<T>
    {
        Dictionary<Type, Handler> handlers;
        Dictionary<Handler, Queue<Message<T>>> queues;
        Dictionary<Action<T>, Handler> allHandler;

        public MessageQueue()
        {
            handlers = new Dictionary<Type, Handler>();
            queues = new Dictionary<Handler, Queue<Message<T>>>();
            allHandler = new Dictionary<Action<T>, Handler>();
        }

        public Message<T> Engueue(T message)
        {
            Type typeMessage = message.GetType();

            if (!handlers.ContainsKey(typeMessage))
            {
                throw new NotRegisterMessage();
            }
            else
            {
                Message<T> mes = new Message<T>(message);
                lock (queues[handlers[typeMessage]])
                {
                    queues[handlers[typeMessage]].Enqueue(mes);

                    if (handlers[typeMessage].Reentrant)
                    {
                        handlers[typeMessage].AsyncHandle(queues[handlers[typeMessage]].Dequeue());
                    }
                    else
                    {
                        if (!handlers[typeMessage].Working)
                        {
                            handlers[typeMessage].AsyncHandle(queues[handlers[typeMessage]].Dequeue());

                        }
                    }
                }
                return mes;
            }
        }

        public void RegisterHandler (List<T> messages, Action<T> handler, bool reentrant = false)
        {
            messages.ForEach(delegate (T message) {
                RegisterHandler(message, handler);
            });
        }

        public void RegisterHandler (T message, Action<T> handler, bool reentrant = false)
        {
            Type typeMessage = message.GetType();
            
            if ( ! allHandler.ContainsKey(handler) )
            {
                Queue<Message<T>> newQueue = new Queue<Message<T>>();
                Handler newHandler = new Handler(handler, newQueue, reentrant);
                handlers.Add(typeMessage, newHandler);
                queues.Add(newHandler, newQueue);
                allHandler.Add(handler, newHandler);
            } 
            else
            {
                handlers.Add(typeMessage, allHandler[handler]);
            }
        }

        public class Message<M>
        {
            public M message { get; }
            private IAsyncResult resultObj;
            private Exception exception;

            internal Message(M message)
            {
                this.message = message;
                resultObj = null;
                exception = null; 
            }

            internal void setException(Exception exception)
            {
                this.exception = exception;
            }

            internal void setResultObj(IAsyncResult resultObj)
            {
                this.resultObj = resultObj;
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

        internal class Handler
        {
            private Action<T> handler;
            private Queue<Message<T>> queue;
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
                        handler.Invoke(mes.message);
                    } catch (Exception e)
                    {
                        mes.setException(e);
                    }
                };
                message.setResultObj(asyncHandler.BeginInvoke(message, new AsyncCallback(AsyncCompleted), null));
                lock (message.message)
                {
                    Monitor.PulseAll(message.message);
                }
            }

            private void AsyncCompleted(IAsyncResult resObj)
            {
                lock (queue)
                {
                    if(queue.Count != 0)
                    {
                        AsyncHandle(queue.Dequeue());
                    }
                    else
                    {
                        working = false;
                    }
                }
            }

            internal bool Equals(Handler obj)
            {
                return handler == obj.handler;
            }
        }
    }

    public class NotRegisterMessage: Exception
    {
        public NotRegisterMessage()
        {

        }
    }
}
