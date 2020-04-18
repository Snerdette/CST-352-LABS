using System;
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

        static void Main(string[] args)
        {
            // Instantiates a SafeRing queue with capacity of qCapacity.
            SafeRing buffer = new SafeRing(qCapacity);

            // Instantiates and starts nProducers producers, each to produce nItems items
            List<Producer> producers = new List<Producer>();
            WaitHandle[] completeEvents = new WaitHandle[nProducers];
            for (int i = 0; i < nProducers; i++)
            {
                Producer p = new Producer(buffer, nItems);
                completeEvents[i] = p.Complete;
                p.Start();
                producers.Add(p);
            }

            // Instantiates and starts nConsumers consumers
            List<Consumer> consumers = new List<Consumer>();
            for (int i = 0; i < nConsumers; i++)
            {
                Consumer c = new Consumer(buffer);
                c.Start();
                consumers.Add(c);
            }


            // Wait for all producers to complete then signal consumers to stop
            WaitHandle.WaitAll(completeEvents);
            // TODO: buffer.WaitUntilEmpty();           
            consumers.ForEach(c => { c.Stop(); });
            
            
        }
    }
}
