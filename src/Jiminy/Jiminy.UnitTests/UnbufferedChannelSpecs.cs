using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jiminy.UnitTests {
    public class UnbufferedChannelSpecs {
        [Fact(DisplayName = "Blocking receive blocks until there is a message")]
        void blocking_receice_blocks_until_message() {
            var chan = Channel.Make<Guid>();
            var messageSent = false;
            var sender = Task.Run(() =>
            {
                Task.Delay(100).Wait();
                chan.Send(Guid.NewGuid());
                messageSent = true;
            });
            messageSent.ShouldBeFalse();
            var (msg, error) = chan.Receive();
            sender.Wait();
            messageSent.ShouldBeTrue();
        }

        [Fact(DisplayName = "Blocking receive returns correct result")]
        void blocking_receive_returns_correct_result() {
            var chan = Channel.Make<Guid>();
            var expectedMsg = Guid.NewGuid();
            Task.Run(() => chan.Send(expectedMsg));
            var (msg, error) = chan.Receive();
            msg.ShouldBe(expectedMsg);
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking receive fails if the channel is already closed")]
        void blocking_receive_fails_on_closed_channel() {
            var chan = Channel.Make<Guid>();
            chan.Close();
            var (msg, error) = chan.Receive();
            error.ShouldNotBeNull();
        }

        [Fact(DisplayName = "Blocking receive succeeds if the channel is closed after the fact")]
        void blocking_receive_fails_when_channel_closes() {
            var chan = Channel.Make<Guid>();
            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                chan.Send(Guid.NewGuid());
                chan.Close();
            });
            var (msg, error) = chan.Receive();
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking send succeeds if there is a blocking receiver")]
        void blocking_send_succeeds_on_blocking_receiver() {
            var chan = Channel.Make<int>();
            Task.Run(() => chan.Receive());
            chan.Send(1).ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking send blocks until there is a blocking receiver")]
        void blocking_send_blocks_until_blocking_receiver() {
            var chan = Channel.Make<int>();
            var messageReceived = false;
            var complete = new ManualResetEventSlim();
            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                chan.Receive();
                messageReceived = true;
                complete.Set();
            });
            messageReceived.ShouldBeFalse();
            chan.Send(1);
            complete.Wait();
            messageReceived.ShouldBeTrue();
        }

        [Fact(DisplayName = "Blocking send fails if the channel is already closed")]
        void blocking_send_fails_on_closed_channel() {
            var chan = Channel.Make<int>();
            chan.Close();
            chan.Send(1).ShouldNotBeNull();
        }

        [Fact(DisplayName = "Blocking send succeeds if the channel is closed after the fact")]
        void blocking_send_fails_when_channel_closes() {
            var chan = Channel.Make<int>();
            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                chan.Receive();
                chan.Close();
            });
            chan.Send(1).ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking select succeeds when there is a message")]
        void blocking_select_succeeds_on_message() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            Task.Run(() => chan2.Send(2));
            var error = Channel.Select()
                .Case(chan1, m => { })
                .Case(chan2, m => { })
                .Receive();
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking select succeeds if at least one channel is open and it receives a message")]
        void blocking_select_succeeds_when_at_least_one_channel_open() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            Task.Run(() => chan2.Send(2));
            chan1.Close();
            var error = Channel.Select()
                .Case(chan1, m => { })
                .Case(chan2, m => { })
                .Receive();
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking select fails if all channels are already closed")]
        void blocking_select_fails_on_all_channels_closed() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            int? chan1RcvMsg = null;
            int? chan2RcvMsg = null;
            chan1.Close();
            chan2.Close();
            var error = Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Receive();
            error.ShouldNotBeNull();
            chan1RcvMsg.ShouldBeNull();
            chan2RcvMsg.ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking select fails if all channels are closed after the fact")]
        void blocking_select_fails_when_all_channels_closes() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            int? chan1RcvMsg = null;
            int? chan2RcvMsg = null;
            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                chan1.Close();
                chan2.Close();
            });
            var error = Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Receive();
            error.ShouldNotBeNull();
            chan1RcvMsg.ShouldBeNull();
            chan2RcvMsg.ShouldBeNull();
        }

        [Fact(DisplayName = "Blocking select invokes the correct handler")]
        void blocking_select_invokes_correct_handler() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            int? chan1RcvMsg = null;
            int? chan2RcvMsg = null;
            Task.Run(() => chan2.Send(2));
            Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Receive();
            chan1RcvMsg.ShouldBeNull();
            chan2RcvMsg.Value.ShouldBe(2);
            //reset and try the other channel
            chan2RcvMsg = null;
            Task.Run(() => chan1.Send(1));
            Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Receive();
            chan2RcvMsg.ShouldBeNull();
            chan1RcvMsg.Value.ShouldBe(1);
        }

        [Fact(DisplayName = "Nonblocking select succeeds when there is a message")]
        void nonblocking_select_succeeds_on_message() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            Task.Run(() => chan2.Send(2));
            var error = Channel.Select()
                .Case(chan1, m => { })
                .Case(chan2, m => { })
                .Otherwise(() => { });
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Nonblocking select succeeds if at least one channel is open and it receives a message")]
        void nonblocking_select_succeeds_when_at_least_one_channel_open() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            Task.Run(() => chan2.Send(2));
            chan1.Close();
            var error = Channel.Select()
                .Case(chan1, m => { })
                .Case(chan2, m => { })
                .Otherwise(() => { });
            error.ShouldBeNull();
        }

        [Fact(DisplayName = "Nonblocking select succeeds even if all channels are closed")]
        void nonblocking_select_fails_on_all_channels_closed() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            int? chan1RcvMsg = null;
            int? chan2RcvMsg = null;
            var defaultInvoked = false;
            chan1.Close();
            chan2.Close();
            var error = Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Otherwise(() => defaultInvoked = true);
            error.ShouldBeNull();
            chan1RcvMsg.ShouldBeNull();
            chan2RcvMsg.ShouldBeNull();
            defaultInvoked.ShouldBeTrue();
        }

        [Fact(DisplayName = "Nonblocking select invokes the correct handler")]
        void nonblocking_select_invokes_correct_handler() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            int? chan1RcvMsg = null;
            int? chan2RcvMsg = null;
            var defaultInvoked = false;
            Task.Run(() => chan2.Send(2));
            Task.Delay(100).Wait();
            Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Otherwise(() => defaultInvoked = true);
            chan1RcvMsg.ShouldBeNull();
            chan2RcvMsg.Value.ShouldBe(2);
            defaultInvoked.ShouldBeFalse();
            //reset and try the other channel
            chan2RcvMsg = null;
            Task.Run(() => chan1.Send(1));
            Task.Delay(100).Wait();
            Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Otherwise(() => defaultInvoked = true);
            chan2RcvMsg.ShouldBeNull();
            chan1RcvMsg.Value.ShouldBe(1);
            defaultInvoked.ShouldBeFalse();
            //reset and don't send a message this time
            chan1RcvMsg = null;
            Task.Delay(100).Wait();
            Channel.Select()
                .Case(chan1, m => chan1RcvMsg = m)
                .Case(chan2, m => chan2RcvMsg = m)
                .Otherwise(() => defaultInvoked = true);
            chan2RcvMsg.ShouldBeNull();
            chan1RcvMsg.ShouldBeNull();
            defaultInvoked.ShouldBeTrue();
        }

        [Fact(DisplayName = "Range receiving will end when the channel is closed")]
        void range_receiving_ends_when_channel_closes() {
            var chan = Channel.Make<int>();
            var n = 2;
            var expected = 0;
            var sender = Task.Run(() =>
            {
                for (var i = 0; i < n; i++)
                    chan.Send(i);
                chan.Close();
            });
            foreach (var msg in chan.Range()) {
                msg.ShouldBe(expected);
                expected++;
            }
        }

        [Fact(DisplayName = "Blocking receive returns an error if a handler fails with an exception")]
        void blocking_receive_returns_error_on_handler_exception() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            Task.Run(() => chan2.Send(2));
            Task.Delay(100).Wait();
            var error = Channel.Select()
                .Case(chan1, m => { })
                .Case(chan2, m => { throw new DivideByZeroException(); })
                .Receive();
            error.ShouldNotBeNull();
            error.description.Contains(nameof(DivideByZeroException)).ShouldBeTrue();
        }

        [Fact(DisplayName = "Nonblocking select returns an error if a handler fails with an exception")]
        void nonblocking_receive_returns_error_on_handler_exception() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            Task.Run(() => chan2.Send(2));
            Task.Delay(100).Wait();
            var error = Channel.Select()
                .Case(chan1, m => { })
                .Case(chan2, m => { throw new DivideByZeroException(); })
                .Otherwise(() => { });
            error.ShouldNotBeNull();
            error.description.Contains(nameof(DivideByZeroException)).ShouldBeTrue();
        }

        [Fact(DisplayName = "Nonblocking select returns an error if there are no messages and the default handler fails with an exception")]
        void nonblocking_receive_returns_error_on_default_handler_exception() {
            var chan1 = Channel.Make<int>();
            var chan2 = Channel.Make<int>();
            var error = Channel.Select()
                .Case(chan1, m => { })
                .Case(chan2, m => { })
                .Otherwise(() => { throw new DivideByZeroException(); });
            error.ShouldNotBeNull();
            error.description.Contains(nameof(DivideByZeroException)).ShouldBeTrue();
        }

        [Fact(DisplayName = "Nonblocking send succeeds if there is a receiver")]
        void nonblocking_send_succeeds_if_receiver() {
            var chan = Channel.Make<Guid>();
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

        [Fact(DisplayName = "Nonblocking send invokes default handler if there is no receiver")]
        void nonblocking_send_invokes_default_if_no_receiver() {
            var chan = Channel.Make<int>();
            var defaultInvoked = false;
            var error = chan.Send(1, () => defaultInvoked = true);
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

        [Fact(DisplayName = "Blocking select used to fan-in will properly send the correct amount of messages and close output when finished")]
        void fan_in_using_select() {
            var n = 2000;
            var remainingMessages = new HashSet<Guid>();
            var ch1 = Channel.Make<Guid>();
            var ch2 = Channel.Make<Guid>();
            var ch3 = Channel.Make<Guid>();
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
        void fan_in_using_merge() {
            var n = 2000;
            var remainingMessages = new HashSet<Guid>();
            var ch1 = Channel.Make<Guid>();
            var ch2 = Channel.Make<Guid>();
            var ch3 = Channel.Make<Guid>();
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
