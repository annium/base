using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell;

public interface IShellInstance
{
    IShellInstance Configure(Action<ProcessStartInfo> configure);

    IShellInstance Pipe(bool pipe);

    Task<ShellResult> RunAsync(TimeSpan timeout);

    Task<ShellResult> RunAsync(CancellationToken ct = default);

    ShellAsyncResult Start(CancellationToken ct = default);
}