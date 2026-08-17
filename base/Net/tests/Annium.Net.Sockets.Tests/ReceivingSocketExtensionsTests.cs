using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Tests for <see cref="ReceivingSocketExtensions.Observe"/>.
/// </summary>
public class ReceivingSocketExtensionsTests
{
    /// <summary>
    /// Three OnReceived raises produce exactly three items through the observable in order.
    /// </summary>
    [Fact]
    public void Observe_ThreeEvents_EmitsThreeItemsInOrder()
    {
        var fake = new FakeReceivingSocket();
        var items = new List<byte[]>();

        using var sub = fake.Observe().Subscribe(mem => items.Add(mem.ToArray()));

        var a = new byte[] { 1 };
        var b = new byte[] { 2, 3 };
        var c = new byte[] { 4, 5, 6 };

        fake.RaiseReceived(a);
        fake.RaiseReceived(b);
        fake.RaiseReceived(c);

        items.Has(3);
        items.At(0).IsEqual(a);
        items.At(1).IsEqual(b);
        items.At(2).IsEqual(c);
    }

    /// <summary>
    /// After the subscription is disposed, further OnReceived raises do not reach the observer.
    /// </summary>
    [Fact]
    public void Observe_AfterDispose_NoFurtherEmissions()
    {
        var fake = new FakeReceivingSocket();
        var items = new List<byte[]>();

        var sub = fake.Observe().Subscribe(mem => items.Add(mem.ToArray()));

        fake.RaiseReceived(new byte[] { 1 });
        items.Has(1);

        // Dispose the subscription — the handler must be unsubscribed.
        sub.Dispose();

        fake.RaiseReceived(new byte[] { 2 });

        // Still only one item; the second raise was ignored.
        items.Has(1);
    }

    /// <summary>
    /// After the subscription is disposed, the underlying OnReceived event has no remaining
    /// subscribers (the handler added by Observe() was removed).
    /// </summary>
    [Fact]
    public void Observe_AfterDispose_HandlerIsUnsubscribed()
    {
        var fake = new FakeReceivingSocket();

        var sub = fake.Observe().Subscribe(_ => { });
        fake.HasSubscribers.IsTrue();

        sub.Dispose();

        fake.HasSubscribers.IsFalse();
    }

    /// <summary>
    /// Minimal <see cref="IReceivingSocket"/> fake with a raisable <see cref="OnReceived"/>
    /// event and a <see cref="HasSubscribers"/> helper.
    /// </summary>
    private sealed class FakeReceivingSocket : IReceivingSocket
    {
        /// <summary>Backing delegate for <see cref="OnReceived"/>, raised by <c>Raise</c>.</summary>
        private event Action<ReadOnlyMemory<byte>>? _onReceived;

        /// <summary>Event raised when the fake socket receives data.</summary>
        public event Action<ReadOnlyMemory<byte>> OnReceived
        {
            add { _onReceived += value; }
            remove { _onReceived -= value; }
        }

        /// <summary>
        /// Returns <see langword="true"/> when at least one handler is subscribed to
        /// <see cref="OnReceived"/>.
        /// </summary>
        public bool HasSubscribers => _onReceived is not null;

        /// <summary>Raises the <see cref="OnReceived"/> event with the given data.</summary>
        /// <param name="data">Data to deliver to subscribers.</param>
        public void RaiseReceived(ReadOnlyMemory<byte> data) => _onReceived?.Invoke(data);
    }
}
