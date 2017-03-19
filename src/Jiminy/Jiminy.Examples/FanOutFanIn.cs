using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    sealed class FanOutFanIn : IExample {
        public void Run() {
            var messageCount = 30;
            var numbers = Generate(messageCount);
            var r1 = Channel.Make<string>();
            var r2 = Channel.Make<string>();
            var r3 = Channel.Make<string>();

            Task.Run(() => Resend(numbers, r1, "first"));
            Task.Run(() => Resend(numbers, r2, "second"));
            Task.Run(() => Resend(numbers, r3, "third"));

            var messagesReceived = 0;
            foreach (var r in Channel.Merge(r1, r2, r3).Range()) {
                Console.WriteLine(r);
                messagesReceived++;
            }
            if (messagesReceived != messageCount) {
                throw new Exception($"Received {messagesReceived} messages but expected {messageCount}");
            }
            Console.WriteLine("done");
        }

        IReceive<int> Generate(int n) {
            var numbers = Channel.Make<int>(n);
            for (var i = 0; i < n; i++) {
                numbers.Send(i);
            }
            numbers.Close();
            return numbers;
        }

        void Resend(IReceive<int> numbers, ISend<string> results, string name) {
            foreach (var n in numbers.Range()) {
                Task.Delay(5).Wait();
                results.Send($"{name} : {n}");
            }
            results.Close();
            Console.WriteLine($" [+] {name} done");
        }
    }
}
