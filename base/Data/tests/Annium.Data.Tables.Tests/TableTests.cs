using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Data.Tables.Tests;

public class TableTests : TestBase
{
    public TableTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(x => x.AddTables());
    }

    [Fact]
    public async Task Works()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Keep(x => x.IsAlive)
            .UpdateWith((a, b) => a.IsAlive != b.IsAlive, (s, v) => s.Update(v.IsAlive))
            .Build();
        var log1 = new List<IChangeEvent<Sample>>();
        var log2 = new List<IChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log1.Add);
        table.Subscribe(log2.Add);
        log1.Has(1);
        log1.At(0).IsEqual(ChangeEvent.Init(Array.Empty<Sample>()));
        log2.At(0).IsEqual(log1.At(0));

        // init with some data
        var initValues = new[] { new Sample(1, true) };
        table.Init(initValues);
        await Expect.To(() => log1.Count.IsGreater(1));
        log1.Has(2);
        log1.At(1).IsEqual(ChangeEvent.Init(initValues));
        log2.At(1).Is(log1.At(1));
    }
}

file record Sample(int Key, bool IsAlive) : ICopyable<Sample>
{
    public bool IsAlive { get; private set; } = IsAlive;

    public void Update(bool isAlive)
    {
        IsAlive = isAlive;
    }

    public Sample Copy() => this with { };
}
