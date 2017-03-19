using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    sealed class PublishSubscribe : IExample {
        public void Run() {
            var wgReady = new WaitGroup(2);
            var wgDone = new WaitGroup(2);
            using (var publisher = new PubSub<int>()) {
                Subscribe(publisher, wgReady, wgDone, "subscriber 1");
                Subscribe(publisher, wgReady, wgDone, "subscriber 2");
                wgReady.Wait();
                for (var i = 0; i < 5; i++) {
                    publisher.Publish(i);
                }
            }
            wgDone.Wait();
        }

        void Subscribe(PubSub<int> publisher, WaitGroup wgReady, WaitGroup wgDone, string name) {
            Task.Run(() =>
            {
                var chan = publisher.Subscribe();
                wgReady.Done();
                foreach (var m in chan.Range()) {
                    Console.WriteLine($"{name} received {m}");
                }
                Console.WriteLine($"{name} is done");
                wgDone.Done();
            });
        }
    }
}
