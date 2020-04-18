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
        public Producer(SafeRing buffer, int numItemsToProduce)
        {
            // Set to not complete when initialized.
            completeEvent = new ManualResetEvent(false);
        }

        // Starts a new thread to do the producing work
        public void Start()
        {

        }

        // Signaled when the producer is done producing
        public ManualResetEvent Complete { get { return completeEvent; } }
    }
}
