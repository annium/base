// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Annium.Core.Primitives;
//
// namespace Annium.Extensions.Reactive.Internal.Creation.Instance;
//
// internal class DynamicObservableInstance<T> : ObservableInstanceBase<T>, IObservableInstance<T>
// {
//     private readonly Func<ObserverContext<T>, Task<Func<Task>>> _factory;
//     private Task _factoryTask = Task.CompletedTask;
//     private CancellationTokenSource _factoryCts = new();
//
//     internal DynamicObservableInstance(
//         Func<ObserverContext<T>, Task<Func<Task>>> factory
//     )
//     {
//         _factory = WrapFactory(factory);
//     }
//
//     public IDisposable Subscribe(IObserver<T> observer)
//     {
//         lock (Lock)
//         {
//             Subscribers.Add(observer);
//
//             // init factory if first subscriber
//             if (Subscribers.Count == 1)
//             {
//                 var factoryTask = _factoryTask.IsCompleted ? Task.CompletedTask : _factoryTask;
//                 _factoryTask = Task.Run(async () =>
//                 {
//                     await factoryTask;
//                     var disposeAsync = await _factory(GetObserverContext(_factoryCts.Token));
//                     await disposeAsync();
//                 });
//             }
//         }
//
//         return Disposable.Create(() =>
//         {
//             lock (Lock)
//             {
//                 Subscribers.Remove(observer);
//
//                 // cancel factory if last subscriber
//                 if (Subscribers.Count == 0 && !_factoryCts.IsCancellationRequested)
//                     _factoryCts.Cancel();
//             }
//         });
//     }
//
//     public async ValueTask DisposeAsync()
//     {
//         InitDisposal();
//
//         _factoryCts.Cancel();
//         Task factoryTask;
//         lock (Lock) factoryTask = _factoryTask;
//
//         await factoryTask;
//
//         AfterDispose();
//     }
//
//     private Func<ObserverContext<T>, Task<Func<Task>>> WrapFactory(
//         Func<ObserverContext<T>, Task<Func<Task>>> factory
//     ) => async ctx =>
//     {
//         _factoryCts = new CancellationTokenSource();
//         return await factory(ctx with { Ct = _factoryCts.Token });
//     };
// }

