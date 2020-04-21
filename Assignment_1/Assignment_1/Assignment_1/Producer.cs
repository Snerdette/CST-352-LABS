using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private int timeout;
        private int numRetries;
        private int backoffTime;

        public Producer(SafeRing buffer, int numItemsToProduce, Random rand, int numRetries = 5, int backoffTime = 1000, int timeout = -1)
        {
            this.buffer = buffer;
            this.numItemsToProduce = numItemsToProduce;
            this.rand = rand;
            this.timeout = timeout;
            this.numRetries = numRetries;
            this.backoffTime = backoffTime;

            // Set to not complete when initialized.
            completeEvent = new ManualResetEvent(false);          
        }

        // Starts a new thread to do the producing work
        // Returns immediatly
        public void Start()
        {
            workingThread = new Thread(Produce);
            workingThread.Start();
        }

        // Method that does actual production.
        // This will run on the working thread.
        // When this method exits, the thread will stop.
        private void Produce()
        {
            try {
                // Now onto production...
                for (int i = 0; i < numItemsToProduce; i++)
                {
                    // Randomly generate numbers between 1 and 1,000
                    int num = rand.Next(1, 1001);
                    bool success = false;
                    while (!success)
                    {
                        // try to insert product
                        for (int tries = 0; !success && tries < numRetries; tries++)
                        {
                            try
                            {
                                // Inset number into the queue
                                buffer.Insert(num, timeout);

                                // Sleep the thread for that many msec's
                                Console.WriteLine("Info: Producer Sleeping...");
                                Thread.Sleep(num);
                                success = true;
                            }
                            catch (TimeoutException te)
                            {
                                Console.WriteLine("Info: Producer Thread Inturrupted: " + te);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: Producer Exception: " + e.Message);
                            }
                        }

                        // Back off for a bit and try again
                        if (!success)
                        {
                            Console.WriteLine("Warning: producer failing repeatedly, sleeping then retrying");
                            Thread.Sleep(backoffTime);
                        }
                    }
                }               
            }
            catch (Exception e)
            {
                Console.WriteLine("Producer: "+ e.Message);
            }

            // Signals Complete
            completeEvent.Set();
        }

        // Signaled when the producer is done producing
        public ManualResetEvent Complete { get { return completeEvent; } }
    }
}
