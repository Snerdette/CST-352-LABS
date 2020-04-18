using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment_1
{
    public class SafeRing
    {
        private int capacity;
        private int[] buffer;
        private int head;
        private int tail;
        private int size;
        private ManualResetEvent hasItems;
        private ManualResetEvent hasCapacity;
        private Mutex mutex;


        public SafeRing(int capacity)
        {
            this.capacity = capacity;
            buffer = new int[capacity];
            head = 0;
            tail = 0;
            size = 0;
            hasItems = new ManualResetEvent(false);
            hasCapacity = new ManualResetEvent(true);
            mutex = new Mutex();
        }

        public int Remove()
        {
            Console.WriteLine("Info: Removing...");
            // Wait until it's safe to remove and there is at least 1 item in the queue.
            WaitHandle.WaitAll(new WaitHandle[] { mutex, hasItems });

            // Remove an item...
            int i = buffer[head];
            head = (head + 1) % capacity;
            size--;
                
            // Signal we now have capacity 
            hasCapacity.Set();

            // If we emptied the queue, signal that we do NOT have any items available.
            if (size == 0)
                hasItems.Reset();

            // Print out what we removed
            Console.WriteLine("Removed: " + i);

            // Release the Mutex!!!
            mutex.ReleaseMutex();
            return i;
        }

        public void Insert(int i)
        {
            Console.WriteLine("Info: Inserting...");
            WaitHandle.WaitAll(new WaitHandle[] { mutex, hasCapacity });

            // Remove an item...
            buffer[tail] = i;
            tail = (tail + 1) % capacity;
            size++;


            // Signal we now have capacity 
            hasItems.Set();

            // If we maxed out the queue, signal that we have NO capacity available.
            if (size == capacity)
                hasCapacity.Reset();

            // Print out what we removed
            Console.WriteLine("Inserted: " + i);

            // Release the Mutex!!!
            mutex.ReleaseMutex();
        }

        public int Count()
        {
            mutex.WaitOne();
            int count = size;
            mutex.ReleaseMutex();

            return count;           
        }
    }
}
