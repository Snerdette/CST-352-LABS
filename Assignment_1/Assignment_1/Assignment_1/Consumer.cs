﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment_1
{
    public class Consumer
    {
        private Thread workingThread;
        private SafeRing buffer;
        Random rand;
        bool done = false;

        public Consumer(SafeRing buffer, Random rand)
        {
            this.buffer = buffer;
            this.rand = rand;

        }

        // Starts a new thread to do the consuming work
        public void Start()
        {
            workingThread = new Thread(Consume);
            workingThread.Start(this);

        }

        // Method running on the working thread
        // When this method ecxits, the thread will stop
        private static void Consume(Object param)
        {
            Consumer c = param as Consumer;

            // Do the comsuming thing, until we are told to stop
            c.done = false;
            while (!c.done)
            { 
                try
                {
                        // Get the number from the queue
                        int num1 = c.buffer.Remove();

                        // Generate a second Number 1 to 1000
                        int num2 = c.rand.Next(1, 1001);

                        // Sleep for that amount of time
                        Console.WriteLine("Info: Consumer Sleeping...");
                        Thread.Sleep(num2);
                }
                catch (ThreadInterruptedException tie)
                {
                    Console.WriteLine("Info: Consumer Thread Inturrupted: " + tie.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Consumer Exception Thrown, retrying: " + e.Message);
                }
                    
            }
 
        }

        // Stop the Consuming thread.
        public void Stop ()
        {
            // Tells the thread it's time to stop comsuming!
            done = true;

            // Interupt the thread, in case it's currently blocked
            workingThread.Interrupt();
        }
    }
}
