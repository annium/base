// using Annium.Core.DependencyInjection;
// using Annium.Serialization.Abstractions;
// using Annium.Testing;
// using Xunit;
//
// namespace Annium.Data.Tables.Tests;
//
// TODO: separate package for json serialization support
//
// public class ChangeEventSerializationTests
// {
//     [Fact]
//     public void InitEvent_Ok()
//     {
//         // arrange
//         var serializer = GetSerializer();
//         var e = ChangeEvent.Init(new[] { 1, 2 });
//
//         // act
//         var result = serializer.Deserialize<ChangeEvent<int>>(serializer.Serialize(e));
//
//         // assert
//         result.Type.Is(ChangeEventType.Init);
//         result.Items.IsEqual(new[] { 1, 2 });
//     }
//
//     [Fact]
//     public void SetEvent_Ok()
//     {
//         // arrange
//         var serializer = GetSerializer();
//         var e = ChangeEvent.Set(3);
//
//         // act
//         var result = serializer.Deserialize<ChangeEvent<int>>(serializer.Serialize(e));
//
//         // assert
//         result.Type.Is(ChangeEventType.Set);
//         result.Item.Is(3);
//     }
//
//     [Fact]
//     public void DeleteEvent_Ok()
//     {
//         // arrange
//         var serializer = GetSerializer();
//         var e = ChangeEvent.Delete(3);
//
//         // act
//         var result = serializer.Deserialize<ChangeEvent<int>>(serializer.Serialize(e));
//
//         // assert
//         result.Type.Is(ChangeEventType.Delete);
//         result.Item.Is(3);
//     }
//
//     private ISerializer<string> GetSerializer()
//     {
//         var container = new ServiceContainer();
//         container.AddRuntime(GetType().Assembly);
//         container.AddTime().WithRealTime().SetDefault();
//         container.AddSerializers().WithJson(isDefault: true);
//         container.AddLogging();
//
//         var sp = container.BuildServiceProvider();
//
//         sp.UseLogging(route => route.UseInMemory());
//
//         return sp.Resolve<ISerializer<string>>();
//     }
// }
