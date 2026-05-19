using System;
using System.Threading.Channels;
using Annium.Testing;
using Annium.Threading.Channels;
using Xunit;

namespace Annium.Tests.Threading.Channels;

/// <summary>
/// Contains unit tests for <see cref="ChannelWriterExtensions"/> to verify channel write behavior.
/// </summary>
public class ChannelWriterExtensionsTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelWriterExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ChannelWriterExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that Write adds an item to an open unbounded channel.
    /// </summary>
    [Fact]
    public void Write_OpenChannel_AddsItem()
    {
        // arrange
        var channel = Channel.CreateUnbounded<int>();

        // act
        channel.Writer.Write(7);

        // assert
        channel.Reader.Read().Is(7);
    }

    /// <summary>
    /// Verifies that Write throws InvalidOperationException when TryWrite returns false (channel completed).
    /// </summary>
    [Fact]
    public void Write_CompletedChannel_ThrowsInvalidOperationException()
    {
        // arrange — complete the writer so subsequent TryWrite calls return false
        var channel = Channel.CreateUnbounded<int>();
        channel.Writer.Complete();

        // act & assert
        Wrap.It(() => channel.Writer.Write(99)).Throws<InvalidOperationException>();
    }
}
