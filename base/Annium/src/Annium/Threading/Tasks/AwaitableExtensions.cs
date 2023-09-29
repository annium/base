using System.Threading.Tasks;

namespace Annium.Threading.Tasks;

public static class AwaitableExtensions
{
    public static void Await(this Task task) => task.GetAwaiter().GetResult();
    public static T Await<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    public static void Await(this ValueTask task) => task.GetAwaiter().GetResult();
    public static T Await<T>(this ValueTask<T> task) => task.GetAwaiter().GetResult();
}