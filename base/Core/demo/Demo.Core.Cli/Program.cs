using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;
using Annium;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Runtime.Loader;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Demo.Core.Cli;
using Demo.Core.Cli.Lib;

await using var entry = Entrypoint.Default.Setup();
var provider = entry.Provider;

var solutionRoot = Path.GetFullPath(Path.Combine(
    Path.GetDirectoryName(
        Path.GetDirectoryName(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(typeof(Program).Assembly.Location)!
                )!
            )!
        )!
    )!
));

var pluginsSet = new Dictionary<string, string>
{
    { "Demo.Core.Cli.PluginHello", @"Demo.Core.Cli.PluginHello/bin/Debug/net6.0/Demo.Core.Cli.PluginHello.dll" }
};

Console.WriteLine("---BEFORE---");
Trace.App();

using var gate = new ManualResetEventSlim();
Wrap(() =>
{
    var loaded = 0;
    var plugins = pluginsSet.ToDictionary(x => x.Key, x => LoadPlugin(provider, x.Value));
    foreach (var (name, plugin) in plugins)
    {
        loaded++;
        Console.WriteLine($"PLUGIN {name} LOADED");
        AssemblyLoadContext.GetLoadContext(plugin)!.Unloading += _ => Console.WriteLine($"PLUGIN {name} UNLOADING");
        TrackingWeakReference.Get(plugin).Collected += () =>
        {
            Console.WriteLine($"PLUGIN {name} COLLECTED");
            // ReSharper disable once AccessToModifiedClosure
            loaded--;
            if (loaded == 0)
                // ReSharper disable once AccessToDisposedClosure
                gate.Set();
        };
    }

    Console.WriteLine("---WITH PLUGINS---");
    Trace.App();

    Console.WriteLine("---PLUGINS---");
    Trace.Assemblies(plugins.Values.ToArray());

    Console.WriteLine("---COMMANDS---");
    var commands = plugins.Values.SelectMany(CreateCommands).ToArray();
    foreach (ICommand command in commands)
        Console.WriteLine($"{command.Name} - {command.Description}: {command.Execute()}");

    Console.WriteLine("---TYPES---");
    foreach (var (name, plugin) in plugins)
    {
        Console.WriteLine($"{name} - {TypeManager.GetInstance(plugin, provider.Resolve<ILogger>()).Types.Count}");
        TypeManager.Release(plugin);
    }

    foreach (var plugin in plugins.Values)
        AssemblyLoadContext.GetLoadContext(plugin)!.Unload();
});

Console.WriteLine("---AFTER---");
Trace.App();

for (var i = 1; i <= 5; i++)
{
    Console.WriteLine($"---GC {i}---");
    GC.Collect();
    GC.WaitForPendingFinalizers();
}

try
{
    gate.Wait(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
    Console.WriteLine("---UNLOAD SUCCEED---");
}
catch (OperationCanceledException)
{
    Console.WriteLine("---UNLOAD TIMED OUT---");
}

Console.WriteLine("---CLEAN---");
Trace.App();

Assembly LoadPlugin(IServiceProvider sp, string relativePath)
{
    // Navigate up to the solution root
    var directory = Path.Combine(solutionRoot, Path.GetDirectoryName(relativePath)!);
    var name = Path.GetFileNameWithoutExtension(relativePath);

    var loader = sp.Resolve<IAssemblyLoaderBuilder>().UseFileSystemLoader(directory).Build();

    var assembly = loader.Load(name);

    return assembly;
}

IEnumerable<ICommand> CreateCommands(Assembly assembly)
{
    int count = 0;

    foreach (Type type in assembly.GetTypes())
    {
        if (typeof(ICommand).IsAssignableFrom(type) && Activator.CreateInstance(type) is ICommand result)
        {
            count++;
            yield return result;
        }
    }

    if (count == 0)
    {
        string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
        throw new ApplicationException(
            $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
            $"Available types: {availableTypes}");
    }
}

[MethodImpl(MethodImplOptions.NoInlining)]
void Wrap(Action wrap) => wrap();