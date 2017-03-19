using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Xunit.Assert;

namespace Jiminy.UnitTests {
    public class BufferedChannelSpecs {
        [Fact(DisplayName = "Construction fails when limit is less than 0")]
        void ctor_fails_on_invalid_limit() {
            Throws<ArgumentException>(() => Channel.Make<int>(-1));
        }

        [Fact(DisplayName = "Blocking send succeeds immediately if buffer limit has not been exceeded")]
        void blocking_send_succeeds_if_below_buffer_limit() {
            var chan = Channel.Make<int>(1);
            var error = chan.Send(1);
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking send blocks if buffer limit has been exceeded until a slot is released")]
        void blocking_send_blocks_on_buffer_limit() {
            var chan = Channel.Make<int>(1);
            var secondSendComplete = false;
            chan.Send(1);
            var sender = Task.Run(() =>
            {
                chan.Send(2);
                secondSendComplete = true;
            });
            Task.Delay(100).Wait();
            secondSendComplete.ShouldBeFalse();
            (sender.IsCanceled || sender.IsCompleted || sender.IsFaulted).ShouldBeFalse();
            //release the second send by receiving a value
            chan.Receive();
            sender.Wait();
            secondSendComplete.ShouldBeTrue();
        }

        [Fact(DisplayName = "Blocking send succeeds when there is a waiting send and the channel is closed after the fact")]
        void blocking_send_fails_for_waiting_sends_when_channel_closes() {
            var chan = Channel.Make<int>(1);
            chan.Send(1);
            var closer = Task.Run(() =>
            {
                Task.Delay(100).Wait();
                chan.Close();
            });
            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                chan.Receive();
            });
            var error = chan.Send(2);
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Nonblocking send succeeds if there is an available slot and a waiting receiver")]
        void nonblocking_send_succeeds_if_within_limit_and_receiver() {
            var chan = Channel.Make<Guid>(1);
            var expectedMsg = Guid.NewGuid();
            var msg = Guid.Empty;
            var error = Error.Nil;
            var defaultInvoked = false;
            var receiver = Task.Run(() => (msg, error) = chan.Receive());
            Task.Delay(100).Wait();
            chan.Send(expectedMsg, () => defaultInvoked = true);
            receiver.Wait();
            msg.ShouldBe(expectedMsg);
            error.ShouldBeNull();
            defaultInvoked.ShouldBeFalse();
        }

        [Fact(DisplayName = "Nonblocking send succeeds if there is an available slot without a waiting receiver")]
        void nonblocking_send_succeeds_if_within_limit_no_receiver() {
            var chan = Channel.Make<int>(1);
            var defaultInvoked = false;
            var error = chan.Send(1, () => defaultInvoked = true);
            error.ShouldBeNull();
            defaultInvoked.ShouldBeFalse();
        }


        [Fact(DisplayName = "Nonblocking send invokes default handler if there is no available slots")]
        void nonblocking_send_succeeds_if_outside_limit() {
            var chan = Channel.Make<int>(1);
            chan.Send(1);
            var defaultInvoked = false;
            var error = chan.Send(2, () => defaultInvoked = true);
            error.ShouldBeNull();
            defaultInvoked.ShouldBeTrue();
        }

        [Fact(DisplayName = "Nonblocking send fails if the channel is closed")]
        void nonblocking_send_fails_on_closed_channel() {
            var chan = Channel.Make<int>();
            chan.Close();
            var defaultInvoked = false;
            var error = chan.Send(1, () => defaultInvoked = true);
            error.ShouldNotBeNull();
            defaultInvoked.ShouldBeFalse();
        }

        [Fact(DisplayName = "Closing the channel will preserve the buffered messages for later reception even though the channel is closed")]
        void closing_channel_preserves_buffered_messages() {
            var chan = Channel.Make<int>(2);
            chan.Send(1);
            chan.Send(2);
            chan.Close();
            var (msg, error) = chan.Receive(); //we should still be able to receive since two messages are buffered and should not get lost
            msg.ShouldBe(1);
            error.ShouldBeNull();
            (msg, error) = chan.Receive();
            msg.ShouldBe(2);
            error.ShouldBeNull();
        }

        [Fact(DisplayName ="Multiple readers on the same channel will receive all messages sent prior to the channel being closed")]
        void multiple_readers_receive_all_messages_sent_prior_to_channel_close() {
            var n = 500;
            //make a buffer channel
            var source = Channel.Make<int>(n);
            for(var i = 0; i < n; i++) {
                source.Send(i);
            }
            source.Close();
            //start multiple tasks with a result channel for each. Each task reads from the source channel (fan-out)
            var results = new List<IReceive<int>>();
            for (var i = 0; i < 10; i++) {
                var result = Channel.Make<int>();
                results.Add(result);
                Task.Run(() =>
                {
                    foreach(var m in source.Range()) {
                        result.Send(m);
                    }
                    result.Close();
                });
            }
            //use fan-in to merge the result channels into a single channel and read from it until all result channels are closed (which closes the merged channel)
            var count = 0;
            foreach(var m in Channel.Merge(results.ToArray()).Range()) {
                count++;
            }
            count.ShouldBe(n);
        }

        [Fact(DisplayName = "Blocking select used to fan-in will properly send the correct amount of messages and close output when finished")]
        void fan_in_using_select() {
            var n = 2000;
            var remainingMessages = new HashSet<Guid>();
            var ch1 = Channel.Make<Guid>(100);
            var ch2 = Channel.Make<Guid>(100);
            var ch3 = Channel.Make<Guid>(100);
            Action<ISend<Guid>> startProducer = ch =>
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < n; i++) {
                        var msg = Guid.NewGuid();
                        lock (remainingMessages) {
                            remainingMessages.Add(msg);
                        }
                        ch.Send(msg);
                    }
                    ch.Close();
                });
            };
            startProducer(ch1);
            startProducer(ch2);
            startProducer(ch3);
            var output = Channel.Make<Guid>(2000);
            //start fan-in task
            Task.Run(() =>
            {
                var err = Error.Nil;
                while (err == null) {
                    var selector = Channel.Select()
                        .Case(ch1, m => output.Send(m))
                        .Case(ch2, m => output.Send(m))
                        .Case(ch3, m => output.Send(m));
                    err = selector
                    .Receive();
                }
                var k = Channel.Select();
                output.Close();
            });

            //read output and compare number of messages
            foreach (var m in output.Range()) {
                lock (remainingMessages) {
                    remainingMessages.Remove(m);
                }
            }
            remainingMessages.Count.ShouldBe(0);
        }

        [Fact(DisplayName = "Channel merge used to fan-in will properly send the correct amount of messages and close output when finished")]
        void fan_in_using_channel_merge() {
            var n = 2000;
            var remainingMessages = new HashSet<Guid>();
            var ch1 = Channel.Make<Guid>(100);
            var ch2 = Channel.Make<Guid>(100);
            var ch3 = Channel.Make<Guid>(100);
            Action<ISend<Guid>> startProducer = ch =>
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < n; i++) {
                        var msg = Guid.NewGuid();
                        lock (remainingMessages) {
                            remainingMessages.Add(msg);
                        }
                        ch.Send(msg);
                    }
                    ch.Close();
                });
            };
            startProducer(ch1);
            startProducer(ch2);
            startProducer(ch3);
            var output = Channel.Merge(ch1, ch2, ch3);
            
            //read output and compare number of messages
            foreach (var m in output.Range()) {
                lock (remainingMessages) {
                    remainingMessages.Remove(m);
                }
            }
            remainingMessages.Count.ShouldBe(0);
        }
    }
}
