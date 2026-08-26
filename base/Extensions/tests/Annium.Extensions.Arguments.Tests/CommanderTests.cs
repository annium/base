using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Arguments.Tests;

/// <summary>
/// Tests for how a command line reaches a command: which command a group picks for the given arguments,
/// what that command is handed, and what the user is told when nothing matches.
/// </summary>
[Collection("console")]
public class CommanderTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommanderTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public CommanderTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.Add<Trace>().AsSelf().Singleton();
            container.AddMapper();
            container.AddArguments();
        });
    }

    /// <summary>
    /// A named command runs, with its options bound from the arguments that followed it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_KnownCommand_RunsItWithItsOptions()
    {
        // arrange
        var trace = Get<Trace>();

        // act
        await Commander.RunAsync<PlainGroup>(
            Provider,
            ["greet", "-name", "world"],
            TestContext.Current.CancellationToken
        );

        // assert
        trace.Calls.Has(1).At(0).Is("greet world");
    }

    /// <summary>
    /// An unknown command is reported, together with the usage of the group it was asked of. Exiting
    /// silently leaves a mistyped command looking exactly like a successful one.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_UnknownCommand_ReportsItAndPrintsHelp()
    {
        // arrange
        var trace = Get<Trace>();

        // act
        var output = await CaptureAsync(() =>
            Commander.RunAsync<PlainGroup>(Provider, ["gret"], TestContext.Current.CancellationToken)
        );

        // assert
        trace.Calls.IsEmpty("nothing must run for a command that does not exist");
        output.Contains("gret").IsTrue("the output must name the command that was not understood");
        output.Contains("greet").IsTrue("the output must list the commands that do exist");
    }

    /// <summary>
    /// A group invoked with no arguments and no default command prints its usage rather than nothing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_NoArguments_PrintsHelp()
    {
        // arrange

        // act
        var output = await CaptureAsync(() =>
            Commander.RunAsync<PlainGroup>(Provider, [], TestContext.Current.CancellationToken)
        );

        // assert
        output.Contains("greet").IsTrue("the output must list the commands that exist");
    }

    /// <summary>
    /// A group with a default command hands it whatever did not name a command, so the group can be used
    /// as a command in its own right.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_GroupWithDefault_FallsBackToIt()
    {
        // arrange
        var trace = Get<Trace>();

        // act
        await Commander.RunAsync<DefaultingGroup>(Provider, ["-name", "world"], TestContext.Current.CancellationToken);

        // assert
        trace.Calls.Has(1).At(0).Is("greet world");
    }

    /// <summary>
    /// Asking a command for help prints its usage rather than running it: what it takes, which parts are
    /// required, and what each one is for.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_CommandWithHelp_PrintsItsUsage()
    {
        // arrange
        var trace = Get<Trace>();

        // act
        var output = await CaptureAsync(() =>
            Commander.RunAsync<HelpGroup>(Provider, ["deploy", "-help"], TestContext.Current.CancellationToken)
        );

        // assert
        trace.Calls.IsEmpty("asking for help must not run the command");
        output.Contains("deploy").IsTrue("the usage line must name the command");
        output.Contains("target").IsTrue("and its required position");
        output.Contains("[tag]").IsTrue("with an optional position in brackets");
        output.Contains("-force").IsTrue("flags are listed");
        output.Contains("-o|-output").IsTrue("and an aliased option shows both spellings");
        output.Contains("where to deploy to").IsTrue("each argument's description is shown");
    }

    /// <summary>
    /// A group holding no commands still prints something rather than throwing. Nothing matches in an
    /// empty group, and this path now always prints the group's help, so an empty one reaches the help
    /// builder on every invocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_EmptyGroup_PrintsHelpRatherThanThrowing()
    {
        // act
        var output = await CaptureAsync(() =>
            Commander.RunAsync<EmptyGroup>(Provider, [], TestContext.Current.CancellationToken)
        );

        // assert
        output.Contains("a group with nothing in it").IsTrue("the group's own description must still show");
    }

    /// <summary>
    /// Two commands answering to one id is a wiring mistake: dispatch takes the first registered, so every
    /// later one is unreachable with nothing said about it. Adding the second is refused instead.
    /// </summary>
    [Fact]
    public void Add_TwoCommandsWithOneId_Throws()
    {
        // act & assert
        var error = Wrap.It(() => new ClashingGroup()).Throws<ArgumentParseException>();
        error.Message.Contains("greet").IsTrue("the message must name the id both commands answer to");
    }

    /// <summary>
    /// A command built from more than one configuration type binds them all from the same command line, so
    /// what one of them takes is not surplus to another. Checking that per type made every multi-config
    /// command with a positional argument fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_MultiConfigCommandWithAPosition_Runs()
    {
        // arrange
        var trace = Get<Trace>();

        // act
        await Commander.RunAsync<PairGroup>(
            Provider,
            ["pair", "here", "-name", "world"],
            TestContext.Current.CancellationToken
        );

        // assert
        trace.Calls.Has(1).At(0).Is("pair here world");
    }

    /// <summary>
    /// A command whose configuration types would bind one command line differently fails, saying so. Each
    /// type is bound from the same arguments using only its own options, so a flag declared by one of them
    /// is an unknown option to another, which swallows the token after it - here the very token the other
    /// type declares as its position. Binding used to produce whatever each type made of the line on its
    /// own, or a misleading error about a missing argument.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_ConfigurationsThatReadTheLineDifferently_Fails()
    {
        // arrange - Loud declares the flag, Named declares the position that follows it
        var trace = Get<Trace>();

        // act
        var run = Commander.RunAsync<DivergentGroup>(
            Provider,
            ["divergent", "-loud", "here"],
            TestContext.Current.CancellationToken
        );

        // assert
        // VSTHRD003: `run` is this test's own call, awaited to see how the failure surfaces
#pragma warning disable VSTHRD003
        var error = await Wrap.It(async () => await run).ThrowsAsync<ArgumentParseException>();
#pragma warning restore VSTHRD003
        error.Message.Contains("PlaceConfiguration").IsTrue("the message must name the configuration that differs");
        error.Message.Contains("position 1").IsTrue("and what it would bind differently");
        trace.Calls.IsEmpty("nothing must run on a command line the command cannot agree on");
    }

    /// <summary>
    /// A command whose configuration types do agree still runs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RunAsync_ConfigurationsThatAgree_Run()
    {
        // arrange
        var trace = Get<Trace>();

        // act
        await Commander.RunAsync<PairGroup>(
            Provider,
            ["pair", "here", "-name", "world"],
            TestContext.Current.CancellationToken
        );

        // assert
        trace.Calls.Has(1).At(0).Is("pair here world");
    }

    /// <summary>
    /// Runs the given call with the console redirected, and returns what it printed.
    /// </summary>
    /// <param name="act">The call to run.</param>
    /// <returns>Everything written to the console while the call ran.</returns>
    private static async Task<string> CaptureAsync(Func<Task> act)
    {
        var previous = Console.Out;
        await using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await act();
        }
        finally
        {
            Console.SetOut(previous);
        }

        return writer.ToString();
    }
}

/// <summary>
/// Records what the test commands were asked to do.
/// </summary>
public class Trace
{
    /// <summary>
    /// Gets the calls recorded so far, in order.
    /// </summary>
    public IReadOnlyList<string> Calls => _calls;

    /// <summary>
    /// The recorded calls.
    /// </summary>
    private readonly List<string> _calls = new();

    /// <summary>
    /// Records a call.
    /// </summary>
    /// <param name="call">What was called.</param>
    public void Add(string call) => _calls.Add(call);
}

/// <summary>
/// Configuration of the greet command.
/// </summary>
public class GreetConfiguration
{
    /// <summary>
    /// Gets or sets who to greet.
    /// </summary>
    [Option]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Command recording who it was asked to greet.
/// </summary>
public class GreetCommand : Command<GreetConfiguration>, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier this command is invoked by.
    /// </summary>
    public static string Id => "greet";

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    public static string Description => "greets someone";

    /// <summary>
    /// The trace this command records into.
    /// </summary>
    private readonly Trace _trace;

    /// <summary>
    /// Initializes a new instance of the <see cref="GreetCommand"/> class.
    /// </summary>
    /// <param name="trace">The trace to record into.</param>
    public GreetCommand(Trace trace)
    {
        _trace = trace;
    }

    /// <summary>
    /// Records the greeting.
    /// </summary>
    /// <param name="cfg">The command configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public override void Handle(GreetConfiguration cfg, CancellationToken ct) => _trace.Add($"greet {cfg.Name}");
}

/// <summary>
/// The same command, registered as a group's default.
/// </summary>
public class DefaultGreetCommand : Command<GreetConfiguration>, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier this command is invoked by - empty, making it the group's default.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    public static string Description => "greets someone by default";

    /// <summary>
    /// The trace this command records into.
    /// </summary>
    private readonly Trace _trace;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGreetCommand"/> class.
    /// </summary>
    /// <param name="trace">The trace to record into.</param>
    public DefaultGreetCommand(Trace trace)
    {
        _trace = trace;
    }

    /// <summary>
    /// Records the greeting.
    /// </summary>
    /// <param name="cfg">The command configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public override void Handle(GreetConfiguration cfg, CancellationToken ct) => _trace.Add($"greet {cfg.Name}");
}

/// <summary>
/// A group holding one named command and no default.
/// </summary>
public class PlainGroup : Group, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier of this group.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this group.
    /// </summary>
    public static string Description => "a group of commands";

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainGroup"/> class.
    /// </summary>
    public PlainGroup()
    {
        Add<GreetCommand>();
    }
}

/// <summary>
/// A group holding a default command.
/// </summary>
public class DefaultingGroup : Group, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier of this group.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this group.
    /// </summary>
    public static string Description => "a group with a default command";

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultingGroup"/> class.
    /// </summary>
    public DefaultingGroup()
    {
        Add<DefaultGreetCommand>();
    }
}

/// <summary>
/// Configuration exercising every shape the help builder renders.
/// </summary>
public class DeployConfiguration
{
    /// <summary>
    /// Gets or sets where to deploy to.
    /// </summary>
    [Position(1)]
    [Help("where to deploy to")]
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional tag to deploy.
    /// </summary>
    [Position(2, isRequired: false)]
    [Help("which tag to deploy")]
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output directory.
    /// </summary>
    [Option("o")]
    [Help("where to write the result")]
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to deploy regardless.
    /// </summary>
    [Option]
    [Help("deploy even if checks fail")]
    public bool Force { get; set; }
}

/// <summary>
/// Command that records the deployment it was asked for.
/// </summary>
public class DeployCommand : Command<DeployConfiguration>, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier this command is invoked by.
    /// </summary>
    public static string Id => "deploy";

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    public static string Description => "deploys the thing";

    /// <summary>
    /// The trace this command records into.
    /// </summary>
    private readonly Trace _trace;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeployCommand"/> class.
    /// </summary>
    /// <param name="trace">The trace to record into.</param>
    public DeployCommand(Trace trace)
    {
        _trace = trace;
    }

    /// <summary>
    /// Records the deployment.
    /// </summary>
    /// <param name="cfg">The command configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public override void Handle(DeployConfiguration cfg, CancellationToken ct) => _trace.Add($"deploy {cfg.Target}");
}

/// <summary>
/// A group holding the command whose help is under test.
/// </summary>
public class HelpGroup : Group, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier of this group.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this group.
    /// </summary>
    public static string Description => "a group with a documented command";

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpGroup"/> class.
    /// </summary>
    public HelpGroup()
    {
        Add<DeployCommand>();
    }
}

/// <summary>
/// A group with no commands registered.
/// </summary>
public class EmptyGroup : Group, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier of this group.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this group.
    /// </summary>
    public static string Description => "a group with nothing in it";
}

/// <summary>
/// A second command answering to the same id as GreetCommand.
/// </summary>
public class OtherGreetCommand : Command<GreetConfiguration>, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier this command is invoked by - the same as GreetCommand's.
    /// </summary>
    public static string Id => "greet";

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    public static string Description => "greets someone else";

    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="cfg">The command configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public override void Handle(GreetConfiguration cfg, CancellationToken ct) { }
}

/// <summary>
/// A group registering two commands that answer to one id.
/// </summary>
public class ClashingGroup : Group, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier of this group.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this group.
    /// </summary>
    public static string Description => "a group with two commands of one id";

    /// <summary>
    /// Initializes a new instance of the <see cref="ClashingGroup"/> class.
    /// </summary>
    public ClashingGroup()
    {
        Add<GreetCommand>();
        Add<OtherGreetCommand>();
    }
}

/// <summary>
/// The positional half of a two-part configuration.
/// </summary>
public class WhereConfiguration
{
    /// <summary>
    /// Gets or sets where to work.
    /// </summary>
    [Position(1)]
    public string Where { get; set; } = string.Empty;
}

/// <summary>
/// Command taking two configuration types, only one of which declares a position.
/// </summary>
public class PairCommand : Command<WhereConfiguration, GreetConfiguration>, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier this command is invoked by.
    /// </summary>
    public static string Id => "pair";

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    public static string Description => "takes two configurations";

    /// <summary>
    /// The trace this command records into.
    /// </summary>
    private readonly Trace _trace;

    /// <summary>
    /// Initializes a new instance of the <see cref="PairCommand"/> class.
    /// </summary>
    /// <param name="trace">The trace to record into.</param>
    public PairCommand(Trace trace)
    {
        _trace = trace;
    }

    /// <summary>
    /// Records both halves.
    /// </summary>
    /// <param name="where">The positional half.</param>
    /// <param name="greet">The option half.</param>
    /// <param name="ct">Cancellation token.</param>
    public override void Handle(WhereConfiguration where, GreetConfiguration greet, CancellationToken ct) =>
        _trace.Add($"pair {where.Where} {greet.Name}");
}

/// <summary>
/// A group holding the two-configuration command.
/// </summary>
public class PairGroup : Group, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier of this group.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this group.
    /// </summary>
    public static string Description => "a group with a two-configuration command";

    /// <summary>
    /// Initializes a new instance of the <see cref="PairGroup"/> class.
    /// </summary>
    public PairGroup()
    {
        Add<PairCommand>();
    }
}

/// <summary>
/// Configuration declaring a flag.
/// </summary>
public class LoudConfiguration
{
    /// <summary>
    /// Gets or sets whether to be loud.
    /// </summary>
    [Option]
    public bool Loud { get; set; }
}

/// <summary>
/// Configuration declaring the position that follows that flag.
/// </summary>
public class PlaceConfiguration
{
    /// <summary>
    /// Gets or sets where.
    /// </summary>
    [Position(1)]
    public string Place { get; set; } = string.Empty;
}

/// <summary>
/// Command whose two configuration types read one command line differently.
/// </summary>
public class DivergentCommand : Command<LoudConfiguration, PlaceConfiguration>, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier this command is invoked by.
    /// </summary>
    public static string Id => "divergent";

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    public static string Description => "cannot agree with itself";

    /// <summary>
    /// The trace this command records into.
    /// </summary>
    private readonly Trace _trace;

    /// <summary>
    /// Initializes a new instance of the <see cref="DivergentCommand"/> class.
    /// </summary>
    /// <param name="trace">The trace to record into.</param>
    public DivergentCommand(Trace trace)
    {
        _trace = trace;
    }

    /// <summary>
    /// Records the call, if it ever gets that far.
    /// </summary>
    /// <param name="loud">The flag half.</param>
    /// <param name="place">The position half.</param>
    /// <param name="ct">Cancellation token.</param>
    public override void Handle(LoudConfiguration loud, PlaceConfiguration place, CancellationToken ct) =>
        _trace.Add($"divergent {loud.Loud} {place.Place}");
}

/// <summary>
/// A group holding the command that cannot agree with itself.
/// </summary>
public class DivergentGroup : Group, ICommandDescriptor
{
    /// <summary>
    /// Gets the identifier of this group.
    /// </summary>
    public static string Id => string.Empty;

    /// <summary>
    /// Gets the description of this group.
    /// </summary>
    public static string Description => "a group with a self-contradicting command";

    /// <summary>
    /// Initializes a new instance of the <see cref="DivergentGroup"/> class.
    /// </summary>
    public DivergentGroup()
    {
        Add<DivergentCommand>();
    }
}
