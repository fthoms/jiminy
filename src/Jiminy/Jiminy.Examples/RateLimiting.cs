using System;

namespace Jiminy.Examples {
    sealed class RateLimiting : IExample {
        public void Run() {
            var requests = Channel.Make<int>(5);
            for (var i = 0; i < 5; i++) {
                requests.Send(i);
            }
            requests.Close();
            var limiter = new Ticker(TimeSpan.FromMilliseconds(1000));
            foreach (var req in requests.Range()) {
                limiter.Channel.Receive();
                Console.WriteLine($"Request {req} : {DateTime.Now.TimeOfDay}");
            }
        }
    }
}
