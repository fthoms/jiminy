using System;
using System.Threading.Tasks;

namespace Jiminy.Examples {
    /*
package main

import (
	"fmt"
	"time"
)

func consumer(in chan int, done chan bool) {
	ok := true
	x := 0
	for ok {
		x,ok = <-in
		if(ok) {
			fmt.Println(x)
			time.Sleep(100*time.Millisecond)
		}
	}
	fmt.Println("done")
	done <- true
}

func main() {
	ch := make(chan int,3)
	done := make(chan bool)
	go consumer(ch,done)
	for i:=0;i<10;i++ {
		ch <- i
	}
	close(ch)
	//<- done
	time.Sleep(3000*time.Millisecond)
}
     */

    /*  Failure description: when closing a channel then the waiting messages are purged. Receive should
     *  continue to function even after close(), and only fail after that.
     *  Send() should still fail after close().
     */
    sealed class Tester : IExample {
        public void Run() {
            var outgoing = Channel.Make<int>(30);
            var done = Channel.Make<bool>();
            Task.Run(() => Consumer(outgoing, done));
            for (var i = 0; i < 10; i++) {
                outgoing.Send(i);
            }
            outgoing.Close();
            done.Receive();
            //Task.Delay(5000).Wait();
        }

        void Consumer(IChannel<int> incoming, IChannel<bool> done) {
            foreach(var x in incoming.Range()) {
                Console.WriteLine(x);
                Task.Delay(200).Wait();
            }
            Console.WriteLine("done");
            done.Send(true);
        }
    }
}
