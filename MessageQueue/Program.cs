using System;

using System.Threading;

namespace MessageQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageQueue<IPrint> messageQueue = new MessageQueue<IPrint>();
            messageQueue.RegisterHandler<Print1>(Print, false);
            //messageQueue.RegisterHandler<Print2>(Print, true); // true не выставляется, тк ранее Print был выставлен как false
            messageQueue.RegisterHandler<Print2>(Print2, true); 
            Print1 print1 = new Print1("message 1");
            Print2 print2_0 = new Print2("message 2_0");
            Print2 print2_1 = new Print2("message 2_1");


            Message<IPrint> status1 = messageQueue.Engueue(print1);
            Message<IPrint> status2 = messageQueue.Engueue(print1);
            Message<IPrint> status3 = messageQueue.Engueue(print1);
            Message<IPrint> status4 = messageQueue.Engueue(print2_0);
            Message<IPrint> status5 = messageQueue.Engueue(print2_0);
            Message<IPrint> status6 = messageQueue.Engueue(print2_1);

            status1.Wait();
            Console.WriteLine("step");
            status2.Wait();
            Console.WriteLine("step");
            status3.Wait();
            Console.WriteLine("step");
            status4.Wait();
            Console.WriteLine("step");
            status5.Wait();
            Console.WriteLine("step");
            status6.Wait();
            Console.WriteLine("step");
        }
        
        static void Print(IPrint pr)
        {
            pr.Print(pr.GetMessage());
            
        }
        static void Print2(IPrint pr)
        {
            pr.Print(pr.GetMessage());

        }

    }

   interface IPrint
    {
        void Print(string mes);
        string GetMessage();
    }

    class Print1 : IPrint
    {
        string str;
        public Print1(string str)
        {
            this.str = str;
        }
        public void Print(string mes)
        {
            Thread.Sleep(1000);
            Console.WriteLine(" Print1 {0}", mes);
            
        }
        public string GetMessage()
        {
            return str;
        }
    }

    class Print2 : IPrint
    {
        string str;
        public Print2(string str)
        {
            this.str = str;
        }
        public void Print(string mes)
        {
            Console.WriteLine(" Print2 {0} start", mes);
            Thread.Sleep(5000);
            Console.WriteLine(" Print2 {0} finish", mes);
        }
        public string GetMessage()
        {
            return str;
        }
    }
}
