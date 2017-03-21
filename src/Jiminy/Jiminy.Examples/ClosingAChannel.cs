using System;

namespace Jiminy.Examples
{
    sealed class ClosingAChannel : IExample {
        public void Run() {
            //create a buffered channel
            var chan = Channel.Make<int>(2);
            chan.Send(1);
            chan.Send(2);
            chan.Close();

            //the channel is now closed so sending additional messages is not possible
            var err = chan.Send(3);
            if (err != null) {
                Console.WriteLine(err);
            } else {
                Console.WriteLine("this should not be printed");
            }

            //we can still receive the messages sent prior to closing the channel
            var (msg, error) = chan.Receive();
            if (error != null) {
                Console.WriteLine(error);
            }
            Console.WriteLine(msg);

            (msg, error) = chan.Receive();
            if (error != null) {
                Console.WriteLine(error);
            }
            Console.WriteLine(msg);

            //continuing to receive will result in an error
            (msg, error) = chan.Receive();
            if (error != null) {
                Console.WriteLine(error);
            } else {
                Console.WriteLine(msg);
            }
        }
    }
}
