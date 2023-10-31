using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading.Tasks;

namespace Annium.Testing;

public static class Expect
{
    public static async Task To(Action validate, CancellationToken ct, int pollDelay = 25)
    {
        await Wait.UntilAsync(
            () =>
            {
                try
                {
                    validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            },
            ct,
            pollDelay
        );
        validate();
    }

    public static async Task To(Func<ValueTask> validate, CancellationToken ct, int pollDelay = 25)
    {
        await Wait.UntilAsync(
            async () =>
            {
                try
                {
                    await validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            },
            ct,
            pollDelay
        );
        await validate();
    }

    public static Task To(Action validate, int ms = 10_000, int pollDelay = 25) =>
        To(validate, new CancellationTokenSource(ms).Token, pollDelay);

    public static Task To(Func<ValueTask> validate, int ms = 10_000, int pollDelay = 25) =>
        To(validate, new CancellationTokenSource(ms).Token, pollDelay);
}
