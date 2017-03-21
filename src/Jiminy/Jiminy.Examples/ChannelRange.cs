using System;
using System.Threading.Tasks;

namespace Jiminy.Examples
{
    sealed class ChannelRange : IExample {
        public void Run() {
            var products = Channel.Make<string>();
            Task.Run(() => Producer(products));
            //iterate over all messages in the channel until it closes
            foreach(var msg in products.Range()) {
                Console.WriteLine(msg);
            }
            Console.WriteLine("done");
        }

        void Producer(ISend<string> products) {
            for(var i = 0; i < 10; i++) {
                products.Send($"message {i}");
            }
            products.Close(); //this signals completion to the consumer
        }
    }
}
