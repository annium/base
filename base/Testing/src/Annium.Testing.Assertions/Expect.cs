using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading.Tasks;

namespace Annium.Testing.Assertions;

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
            pollDelay);
        validate();
    }

    public static Task To(Action validate, int ms = 10_000, int pollDelay = 25) => To(validate, new CancellationTokenSource(ms).Token, pollDelay);
}