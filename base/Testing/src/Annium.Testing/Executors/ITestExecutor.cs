using System.Threading.Tasks;

namespace Annium.Testing.Executors;

public interface ITestExecutor
{
    uint Order { get; }

    Task ExecuteAsync(Target target);
}