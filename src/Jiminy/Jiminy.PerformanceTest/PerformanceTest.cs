using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jiminy.PerformanceTest {
    /// <summary>
    /// This example tests the speed of an unbuffered channel and a number of buffered channels
    /// with different buffer limits.
    /// </summary>
    sealed class PerformanceTest {
        const int NumberOfMessages = 2000000;

        public void Run() {
            TestWith(Channel.Make<Guid>(), "unbuffered channel");
            TestWith(Channel.Make<Guid>(10), "buffered channel (10)");
            TestWith(Channel.Make<Guid>(100), "buffered channel (100)");
            TestWith(Channel.Make<Guid>(1000), "buffered channel (1,000)");
            TestWith(Channel.Make<Guid>(10000), "buffered channel (10,000)");
            TestWith(Channel.Make<Guid>(100000), "buffered channel (100,000)");
            TestWith(Channel.Make<Guid>(1000000), "buffered channel (1,000,000)");
        }

        void TestWith(IChannel<Guid> chan, string name) {
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($" Running test with {name}");
            Console.WriteLine("----------------------------------------------------");

            var msgs = new Queue<Guid>();
            var done = Channel.Make<Queue<Guid>>();
            var ready = Channel.Make<bool>();
            Task.Run(() => Receiver(chan, ready, done));
            ready.Receive();
            ready.Close();
            var start = DateTime.Now;
            for (var i = 0; i < NumberOfMessages; i++) {
                var msg = Guid.NewGuid();
                msgs.Enqueue(msg);
                chan.Send(msg);
            }
            chan.Close();
            var (receivedMsgs, error) = done.Receive();
            var total = DateTime.Now - start;
            done.Close();
            if (error != null) {
                throw new Exception(error.description);
            }

            Console.WriteLine($"Sending {NumberOfMessages} took {total.TotalMilliseconds} ms");
            Console.WriteLine($"Received {receivedMsgs.Count} messages");
            Console.WriteLine($"Throughput: {Math.Round(NumberOfMessages / total.TotalSeconds)} messages/s");
            Console.Write("Verifying received messages... ");
            var ok = receivedMsgs.Count == msgs.Count && receivedMsgs.Count == NumberOfMessages;
            while (ok && receivedMsgs.Count > 0) {
                ok &= receivedMsgs.Dequeue() == msgs.Dequeue();
            }
            var oldColor = Console.ForegroundColor;
            if (ok) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("ok");
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("failure");
            }
            Console.ForegroundColor = oldColor;
            Console.WriteLine();
        }

        void Receiver(IChannel<Guid> incoming, IChannel<bool> ready, IChannel<Queue<Guid>> done) {
            Queue<Guid> receivedMessages = new Queue<Guid>();
            ready.Send(true);
            foreach (var msg in incoming.Range()) {
                receivedMessages.Enqueue(msg);
            }
            done.Send(receivedMessages);
        }
    }
}
