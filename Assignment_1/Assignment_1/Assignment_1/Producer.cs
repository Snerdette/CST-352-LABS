using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment_1
{
    public class Producer
    {
        ManualResetEvent completeEvent;
        private int numItemsToProduce;
        private SafeRing buffer;
        private Thread workingThread;
        private Random rand;

        public Producer(SafeRing buffer, int numItemsToProduce, Random rand)
        {
            this.buffer = buffer;
            this.numItemsToProduce = numItemsToProduce;
            this.rand = rand;

            // Set to not complete when initialized.
            completeEvent = new ManualResetEvent(false);          
        }

        // Starts a new thread to do the producing work
        // Returns immediatly
        public void Start()
        {
            workingThread = new Thread(Produce);
            workingThread.Start(this);

        }

        // Method that does actual production.
        // This will run on the working thread.
        // When this method exits, the thread will stop.
        private void Produce()
        {
            // Now onto production...
            for (int i = 0; i < numItemsToProduce; i++)
            {
                // Randomly generate numbers between 1 and 1,000
                int num = rand.Next(1, 1001);

                try
                {
                    // Inset number into the queue
                    buffer.Insert(num);

                    // Sleep the thread for that many msec's
                    Console.WriteLine("Info: Producer Sleeping...");
                    Thread.Sleep(num);
                }
                //catch (ThreadInterruptedException tie)
                //{
                //    Console.WriteLine("Info: Producer Thread Inturrupted");
                //}
                catch (Exception e)
                {
                    Console.WriteLine("Error: Producer Exception: " + e.Message);
                }
            }

            // Signals Complete
            completeEvent.Set();
        }

        // Signaled when the producer is done producing
        public ManualResetEvent Complete { get { return completeEvent; } }
    }
}
