using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    sealed class ChannelSynchronisation : IExample {
        public void Run() {
            var done = Channel.Make<bool>();
            Task.Run(() => Worker(done));
            done.Receive(); //wait for worker to complete
            Console.WriteLine("done");
        }

        void Worker(IChannel<bool> done) {
            Task.Delay(1000).Wait();
            done.Send(true); //signal completion
        }
    }
}
