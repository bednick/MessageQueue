using System;
using System.Collections.Generic;


namespace MessageQueue
{
    public class MessageQueue<T>
    {
        Dictionary<Type, Handler<T>> handlers;
        Dictionary<Action<T>, Handler<T>> allHandler;

        public MessageQueue()
        {
            handlers = new Dictionary<Type, Handler<T>>();
            allHandler = new Dictionary<Action<T>, Handler<T>>();
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
                lock (handlers[typeMessage].queue)
                {
                    handlers[typeMessage].queue.Enqueue(mes);
                    if (handlers[typeMessage].Reentrant)
                    {
                        handlers[typeMessage].AsyncHandle(handlers[typeMessage].queue.Dequeue());
                    }
                    else
                    {
                        if (!handlers[typeMessage].Working)
                        {
                            handlers[typeMessage].AsyncHandle(handlers[typeMessage].queue.Dequeue());

                        }
                    }
                }

                return mes;
            }
        }

        public void RegisterHandler<M>(Action<T> handler, bool reentrant = false) where M : T
        {
            Type typeMessage = typeof(M);

            if (!allHandler.ContainsKey(handler))
            {
                Queue<Message<T>> newQueue = new Queue<Message<T>>();
                Handler<T> newHandler = new Handler<T>(handler, newQueue, reentrant);
                handlers.Add(typeMessage, newHandler);

                allHandler.Add(handler, newHandler);
            }
            else
            {
                handlers.Add(typeMessage, allHandler[handler]);
            }
        }

    }
}
