﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class Group : CommandBase
{
    private static readonly string _configurationTypesName = typeof(IConfigurationTypes<>).PureName();
    private readonly List<CommandInfo> _commands = new();

    public Group Add<T>()
        where T : CommandBase, ICommandDescriptor
    {
        var configurationTypes =
            typeof(T)
                .GetInterfaces()
                .SingleOrDefault(x => x.PureName() == _configurationTypesName)
                ?.GetGenericArguments() ?? Type.EmptyTypes;
        _commands.Add(new CommandInfo(T.Id, T.Description, typeof(T), configurationTypes));

        return this;
    }

    public override async Task ProcessAsync(string id, string description, string[] args, CancellationToken ct)
    {
        var (provider, configurationBuilder, helpBuilder) = Root;
        CommandInfo? cmdInfo;

        // if any args - try to find command by id and execute it
        if (args.Length > 0)
        {
            // find command to execute by id
            var childId = args[0];
            cmdInfo = _commands.FirstOrDefault(e => e.Id == childId);
            if (cmdInfo != null)
            {
                var cmd = provider.Resolve(cmdInfo.Type).CastTo<CommandBase>();
                cmd.SetRoot(Root);
                await cmd.ProcessAsync($"{id} {childId}".Trim(), cmdInfo.Description, args.Skip(1).ToArray(), ct);
                return;
            }
        }

        // if no command found, or no args - try to find default command and execute it
        cmdInfo = _commands.FirstOrDefault(e => e.Id == string.Empty);
        if (cmdInfo != null)
        {
            var cmd = provider.Resolve(cmdInfo.Type).CastTo<CommandBase>();
            cmd.SetRoot(Root);
            await cmd.ProcessAsync(id, cmdInfo.Description, args, ct);
            return;
        }

        if (configurationBuilder.Build<HelpConfiguration>(args).Help)
            Console.WriteLine(helpBuilder.BuildHelp(id, description, _commands));
    }
}
