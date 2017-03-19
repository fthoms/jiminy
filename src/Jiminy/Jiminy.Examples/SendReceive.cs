using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    /// <summary>
    /// This example sends n messages to a receiver, which discards them. When the channel is closed,
    /// the main method is notified via a sentinel channel (done).
    /// </summary>
    sealed class SendReceive : IExample {
        const int NumberOfMessages = 1000;

        public void Run() {
            var chan = Channel.Make<int>(1000);
            var done = Channel.Make<bool>();
            Task.Run(() => Send(chan));
            Task.Run(() => Receive(chan, done));
            done.Receive(); //wait for completion
            Console.WriteLine("done");
        }

        void Send(IChannel<int> outgoing) {
            for (var i = 0; i < NumberOfMessages; i++) {
                outgoing.Send(i);
            }
            outgoing.Close(); //signal completion by closing the channel
        }

        void Receive(IChannel<int> incoming, IChannel<bool> done) {
            var expectedMsg = 0;
            foreach(var msg in incoming.Range()) {
                expectedMsg++;
            }
            using (done)
                done.Send(true); //disposing a channel is another way to close it
        }
    }
}
