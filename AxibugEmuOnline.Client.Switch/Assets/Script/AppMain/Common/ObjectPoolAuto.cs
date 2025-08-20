using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AxibugEmuOnline.Client.Common
{
    public static class ObjectPoolAuto
    {
        /************************************************************************************************************************/

        /// <summary>
        /// 获取或者创建一个新的
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(T)"/> 需要回收参见这个</remarks>
        public static T Acquire<T>()
            where T : class, new()
            => ObjectPool<T>.Acquire();

        /// <summary>
        /// 获取或者创建一个新的
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(T)"/> 需要回收参见这个</remarks>
        public static void Acquire<T>(out T item)
            where T : class, new()
            => item = ObjectPool<T>.Acquire();
        /************************************************************************************************************************/

        /// <summary>
        /// 回收对象
        /// </summary>
        public static void Release<T>(T item)
            where T : class, new()
            => ObjectPool<T>.Release(item);

        /// <summary>
        /// 回收对象
        /// </summary>
        public static void Release<T>(ref T item) where T : class, new()
        {
            if (item != null)
            {
                ObjectPool<T>.Release(item);
                item = null;
            }
        }

        /************************************************************************************************************************/
        public const string
            NotClearError = " They must be cleared before being released to the pool and not modified after that.";

        /************************************************************************************************************************/

        /// <summary>
        /// 获取或创建List
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(List{T})"/> 回收参见此方法</remarks>
        public static List<T> AcquireList<T>()
        {
            var list = ObjectPool<List<T>>.Acquire();
            App.log.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
            return list;
        }

        /// <summary>
        /// 回收List
        /// </summary>
        public static void Release<T>(List<T> list)
        {
            list.Clear();
            ObjectPool<List<T>>.Release(list);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 获取或创建Queue
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(Queue{T})"/> 回收参见此方法</remarks>
        public static Queue<T> AcquireQueue<T>()
        {
            var queue = ObjectPool<Queue<T>>.Acquire();
            App.log.Assert(queue.Count == 0, "A pooled list is not empty." + NotClearError);
            return queue;
        }

        /// <summary>
        /// 回收Queue
        /// </summary>
        public static void Release<T>(Queue<T> list)
        {
            list.Clear();
            ObjectPool<Queue<T>>.Release(list);
        }


        /************************************************************************************************************************/

        /// <summary>
        /// 获取或创建HashSet
        /// </summary>
        public static HashSet<T> AcquireSet<T>()
        {
            var set = ObjectPool<HashSet<T>>.Acquire();
            App.log.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
            return set;
        }

        /// <summary>
        /// 释放HashSet
        /// </summary>
        public static void Release<T>(HashSet<T> set)
        {
            set.Clear();
            ObjectPool<HashSet<T>>.Release(set);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 获取一个字符串StringBuilder
        /// </summary>
        /// <remarks>Remember to <see cref="Release(StringBuilder)"/>回收参见这个</remarks>
        public static StringBuilder AcquireStringBuilder()
        {
            var builder = ObjectPool<StringBuilder>.Acquire();
            App.log.Assert(builder.Length == 0, $"A pooled {nameof(StringBuilder)} is not empty." + NotClearError);
            return builder;
        }

        /// <summary>
        /// 回收 StringBuilder
        /// </summary>
        public static void Release(StringBuilder builder)
        {
            builder.Length = 0;
            ObjectPool<StringBuilder>.Release(builder);
        }

        /// <summary>
        /// 回收 StringBuilder
        /// </summary>
        public static string ReleaseToString(this StringBuilder builder)
        {
            var result = builder.ToString();
            Release(builder);
            return result;
        }

        /************************************************************************************************************************/

        private static class Cache<T>
        {
            public static readonly Dictionary<MethodInfo, KeyValuePair<Func<T>, T>>
                Results = new Dictionary<MethodInfo, KeyValuePair<Func<T>, T>>();
        }

        /// <summary>
        /// 此方法主要用于频繁绘制缓存，比如说GUI绘制
        /// </summary>
        public static T GetCachedResult<T>(Func<T> function)
        {
            var method = function.Method;
            if (!Cache<T>.Results.TryGetValue(method, out var result))
            {

                result = new KeyValuePair<Func<T>, T>(function, function());
                Cache<T>.Results.Add(method, result);
            }
            else if (result.Key != function)
            {
                App.log.Warning(
                    $"{nameof(GetCachedResult)}<{typeof(T).Name}>" +
                    $" was previously called on {method.Name} with a different target." +
                    " This likely means that a new delegate is being passed into every call" +
                    " so it can't actually return the same cached object.");
            }

            return result.Value;
        }

        /************************************************************************************************************************/

        public static class Disposable
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Disposable.Acquire"/> to get a spare <see cref="List{T}"/> if
            /// </summary>
            public static IDisposable Acquire<T>(out T item)
                where T : class, new()
                => ObjectPool<T>.Disposable.Acquire(out item);

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Disposable.Acquire"/> to get a spare <see cref="List{T}"/> if
            /// </summary>
            public static IDisposable AcquireList<T>(out List<T> list)
            {
                var disposable = ObjectPool<List<T>>.Disposable.Acquire(out list, onRelease: (l) => l.Clear());
                App.log.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
                return disposable;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Disposable.Acquire"/> to get a spare <see cref="HashSet{T}"/> if
            /// </summary>
            public static IDisposable AcquireSet<T>(out HashSet<T> set)
            {
                var disposable = ObjectPool<HashSet<T>>.Disposable.Acquire(out set, onRelease: (s) => s.Clear());
                App.log.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
                return disposable;
            }

            /************************************************************************************************************************/
        }
        /************************************************************************************************************************/
    }

    public static class ObjectPool<T> where T : class, new()
    {
        /************************************************************************************************************************/

        private static readonly List<T>
            Items = new List<T>();

        /************************************************************************************************************************/

        /// <summary>The number of spare items currently in the pool.</summary>
        public static int Count
        {
            get => Items.Count;
            set
            {
                var count = Items.Count;
                if (count < value)
                {
                    if (Items.Capacity < value)
                        Items.Capacity = NextPowerOfTwo(value);

                    do
                    {
                        Items.Add(new T());
                        count++;
                    }
                    while (count < value);

                }
                else if (count > value)
                {
                    Items.RemoveRange(value, count - value);
                }
            }
        }

        public static int NextPowerOfTwo(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value must be greater than zero.");
            }

            int powerOfTwo = 1;
            while (powerOfTwo < value)
            {
                powerOfTwo <<= 1; // equivalent to multiplying by 2
            }

            return powerOfTwo;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Count"/> is less than the specified value, this method increases it to that value by
        /// creating new objects.
        /// </summary>
        public static void SetMinCount(int count)
        {
            if (Count < count)
                Count = count;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="List{T}.Capacity"/> of the internal list of spare items.</summary>
        public static int Capacity
        {
            get => Items.Capacity;
            set
            {
                if (Items.Count > value)
                    Items.RemoveRange(value, Items.Count - value);
                Items.Capacity = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns a spare item if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release(T)"/> it when you are done.</remarks>
        public static T Acquire()
        {
            var count = Items.Count;
            if (count == 0)
            {
                return new T();
            }
            else
            {
                count--;
                var item = Items[count];
                Items.RemoveAt(count);

                return item;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `item` to the list of spares so it can be reused.</summary>
        public static void Release(T item)
        {
            Items.Add(item);

        }

        /************************************************************************************************************************/

        /// <summary>Returns a description of the state of this pool.</summary>
        public static string GetDetails()
        {
            return
                $"{typeof(T).Name}" +
                $" ({nameof(Count)} = {Items.Count}" +
                $", {nameof(Capacity)} = {Items.Capacity}" +
                ")";
        }

        /************************************************************************************************************************/

        /// <summary>
        /// An <see cref="IDisposable"/> system to allow pooled objects to be acquired and released within <c>using</c>
        /// statements instead of needing to manually release everything.
        /// </summary>
        public sealed class Disposable : IDisposable
        {
            /************************************************************************************************************************/

            private static readonly List<Disposable> LazyStack = new List<Disposable>();

            private static int _ActiveDisposables;

            private T _Item;
            private Action<T> _OnRelease;

            /************************************************************************************************************************/

            private Disposable() { }

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Acquire"/> to set the `item` and returns an <see cref="IDisposable"/>
            /// that will call <see cref="Release(T)"/> on the `item` when disposed.
            /// </summary>
            public static IDisposable Acquire(out T item, Action<T> onRelease = null)
            {
                Disposable disposable;

                if (LazyStack.Count <= _ActiveDisposables)
                {
                    LazyStack.Add(disposable = new Disposable());
                }
                else
                {
                    disposable = LazyStack[_ActiveDisposables];
                }

                _ActiveDisposables++;

                disposable._Item = item = ObjectPool<T>.Acquire();
                disposable._OnRelease = onRelease;
                return disposable;
            }

            /************************************************************************************************************************/

            void IDisposable.Dispose()
            {
                _OnRelease?.Invoke(_Item);
                Release(_Item);
                _ActiveDisposables--;
            }

            /************************************************************************************************************************/
        }
    }
}


