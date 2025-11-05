using System;
using System.Collections;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 双向字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DualWayDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _forward = new();
        private readonly Dictionary<TValue, TKey> _reverse = new();

        // 显式实现非泛型接口
        bool IDictionary.IsFixedSize => false;
        bool IDictionary.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => ((ICollection)_forward).SyncRoot;

        // 泛型接口实现
        public TValue this[TKey key]
        {
            get => _forward[key];
            set
            {
                TValue oldValue;
                if (_forward.TryGetValue(key, out oldValue))
                    _reverse.Remove(oldValue);
                _forward[key] = value;
                _reverse[value] = key;
            }
        }

        object? IDictionary.this[object key]
        {
            get => key is TKey k ? _forward[k] : null;
            set
            {
                if (key is TKey tk && value is TValue tv)
                    this[tk] = tv;
            }
        }

        public ICollection<TKey> Keys => _forward.Keys;
        public ICollection<TValue> Values => _forward.Values;

        ICollection IDictionary.Keys => _forward.Keys;
        ICollection IDictionary.Values => _forward.Values;

        public int Count => _forward.Count;
        public bool IsReadOnly => false;

        // 双向查询扩展
        public bool TryGetKey(TValue value, out TKey key) => _reverse.TryGetValue(value, out key);
        public bool TryGetValue(TKey key, out TValue value) => _forward.TryGetValue(key, out value);

        // 接口方法实现
        public void Add(TKey key, TValue value)
        {
            if (_forward.ContainsKey(key) || _reverse.ContainsKey(value))
                throw new ArgumentException("键或值已存在");
            _forward.Add(key, value);
            _reverse.Add(value, key);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        void IDictionary.Add(object key, object value)
        {
            if (key is TKey k && value is TValue v)
                Add(k, v);
            else
                throw new ArgumentException("类型不匹配");
        }

        public bool Contains(object key) => key is TKey k && _forward.ContainsKey(k);
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue v;
            return _forward.TryGetValue(item.Key, out v) && v.Equals(item.Value);
        }
        public bool ContainsKey(TKey key) => _forward.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_forward).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_forward).CopyTo(array, index);
        }

        public bool Remove(TKey key)
        {
            TValue value;
            if (!_forward.Remove(key, out value)) return false;
            _reverse.Remove(value);
            return true;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        void IDictionary.Remove(object key)
        {
            if (key is TKey k) Remove(k);
        }

        public void Clear()
        {
            _forward.Clear();
            _reverse.Clear();
        }

        // 枚举器实现
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _forward.GetEnumerator();
        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(_forward.GetEnumerator());
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // 非泛型枚举器适配器
        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;
            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator) => _enumerator = enumerator;

            public DictionaryEntry Entry => new(_enumerator.Current.Key, _enumerator.Current.Value);
            public object Key => _enumerator.Current.Key;
            public object? Value => _enumerator.Current.Value;
            public object Current => Entry;
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
        }
    }
}