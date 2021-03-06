﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment_1
{
    class Program
    {
        const int qCapacity = 5;
        const int nProducers = 2;
        const int nConsumers = 2;
        const int nItems = 10;
        const int timeout = 10;
        const int numRetries = 5;
        const int backoffTime = 1000;

        static void Main(string[] args)
        {
            // Instantiates a SafeRing queue with capacity of qCapacity.
            SafeRing buffer = new SafeRing(qCapacity);

            // Instantiates and starts nProducers producers, each to produce nItems items
            List<Producer> producers = new List<Producer>();
            WaitHandle[] completeEvents = new WaitHandle[nProducers];
            Random rand = new Random();
            for (int i = 0; i < nProducers; i++)
            {
                Producer p = new Producer(buffer, nItems, rand, numRetries, backoffTime, timeout);
                completeEvents[i] = p.Complete;
                p.Start();
                producers.Add(p);
            }

            // Instantiates and starts nConsumers consumers
            List<Consumer> consumers = new List<Consumer>();
            for (int i = 0; i < nConsumers; i++)
            {
                Consumer c = new Consumer(buffer, rand, timeout);
                c.Start();
                consumers.Add(c);
            }


            // Wait for all producers to complete then signal consumers to stop
            WaitHandle.WaitAll(completeEvents);
            buffer.WaitUntilEmpty();           
            consumers.ForEach(c => { c.Stop(); });

            Console.WriteLine("Program is Done!");
            Console.ReadLine();
            
            
        }
    }
}
