 
## About Jiminy
Jiminy is a small library for building concurrent programs in an easy manner. The main abstraction is the *channel* which is used to communicate between threads/tasks, and also to synchronise them.

There are several ways of writing concurrent applications. One of the best is the actor model, which is famously implemented in the [akka framework for the JVM](http://akka.io) and the [.NET port akka.NET](http://getakka.net). Another strong favourite is CSP.

Communicating Sequential Processes (CSP) with channels is another method for writing concurrent programs, and **Jiminy** is a .NET implementation of this. CSP and the actor model has overlapping use cases, but differ in some key areas. Which method is the best depends entirely on the use case and the application being written, and akka/akka.NET are really great libraries; Many high performing and resilient systems are written using the actor model.

### Inspired by Go
Jiminy is inspired by my experience with CSP in the [Go programming language](https://golang.org), and I wanted to bring CSP to .NET when I wrote Jiminy. This also means that I have done my best to get rid of exceptions, and instead return instances of `Jiminy.Error` if and when an error occurs.

Now why not just use exceptions? Well, first of all I think try-catch blocks are disruptive to the flow of the program - it breaks up the natural progression of code.
You will also need to figure out in what situations an exception can be thrown, and catch the particular exception. You may not know what types of exception a particular library throws and under what conditions, and the best way to deal with that is to `catch(Exception)`, i.e. a catch-all. Not particular informative to the reader of the code, nor is it clear what has to happen next. This is a longer discussion that will not be continued here.

Jiminy uses return values instead, as illustrated in the following code

	public Error Foo() {
		var (message,error) = chan.Receive();
		if(error != null) {
			//for example when the channel is closed
			return $"Foo failed: {error}";  
		}
		...
	}

### In-depth Examples
Take a look at the wiki for more examples on how to use Jiminy, and take a look at the example code. These sources are more extensive than this document is meant to be, and there you will get more information on:

*	Buffered and unbuffered channels
*	Blocking and non-blocking send / receive
*	Channel select for reading from multiple channels at once
*	Fan-out for distributing work across multiple threads
*	Channel merge for fan-in, where multiple channels are merged into one 
*	Timers, tickers and time-outs
*	Publish/subscribe 

## Download
Get the latest source from this repository, or dive right in and [get the latest nuget package](https://www.nuget.org/packages/Jiminy) for your project.

## Contribute
Get in touch if you want to be part of this or have new ideas or feature requests. Or even better: create pull requests.

