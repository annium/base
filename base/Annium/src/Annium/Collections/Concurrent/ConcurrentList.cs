// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace Annium.Collections.Concurrent;
//
// public class ConcurrentList<T> : IList<T>, IReadOnlyList<T>
// {
//     public bool IsReadOnly => false;
//     private readonly List<T> _values = new();
//
//     public void Add(T item)
//     {
//         lock (_values) _values.Add(item);
//     }
//
//     public void Clear()
//     {
//         lock (_values) _values.Clear();
//     }
//
//     public bool Contains(T item)
//     {
//         lock (_values) return _values.Contains(item);
//     }
//
//     public void CopyTo(T[] array, int arrayIndex)
//     {
//         lock (_values) _values.CopyTo(array, arrayIndex);
//     }
//
//     public bool Remove(T item)
//     {
//         lock (_values) return _values.Remove(item);
//     }
//
//     int ICollection<T>.Count
//     {
//         get
//         {
//             lock (_values)
//                 return _values.Count;
//         }
//     }
//
//
//     public int IndexOf(T item)
//     {
//         lock (_values) return _values.IndexOf(item);
//     }
//
//     public void Insert(int index, T item)
//     {
//         lock (_values) _values.Insert(index, item);
//     }
//
//     public void RemoveAt(int index)
//     {
//         lock (_values) _values.RemoveAt(index);
//     }
//
//     public T this[int index]
//     {
//         get
//         {
//             lock (_values)
//                 return _values[index];
//         }
//         set
//         {
//             lock (_values)
//                 _values[index] = value;
//         }
//     }
//
//     int IReadOnlyCollection<T>.Count
//     {
//         get
//         {
//             lock (_values)
//                 return _values.Count;
//         }
//     }
//
//     public IEnumerator<T> GetEnumerator()
//     {
//         lock (_values)
//             return _values.ToList().GetEnumerator();
//     }
//
//     IEnumerator IEnumerable.GetEnumerator()
//     {
//         lock (_values)
//             return _values.ToArray().GetEnumerator();
//     }
// }

