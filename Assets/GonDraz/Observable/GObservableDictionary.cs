using System.Collections.Generic;
using GonDraz.Events;

namespace GonDraz.Observable
{
    public class GObservableDictionary<TKey, TValue>
    {
        public GEvent OnCleared;

        public GEvent<TKey, TValue> OnItemAdded;
        public GEvent<TKey, TValue> OnItemRemoved;
        public GEvent<TKey, TValue> OnItemUpdated;

        public GObservableDictionary()
        {
            Dictionary = new Dictionary<TKey, TValue>();
        }

        public GObservableDictionary(Dictionary<TKey, TValue> initialDictionary)
        {
            Dictionary = new Dictionary<TKey, TValue>(initialDictionary);
        }

        public Dictionary<TKey, TValue> Dictionary { get; }

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set
            {
                var exists = Dictionary.ContainsKey(key);
                Dictionary[key] = value;

                if (exists)
                    OnItemUpdated?.Invoke(key, value);
                else
                    OnItemAdded?.Invoke(key, value);
            }
        }

        public int Count => Dictionary.Count;

        public ICollection<TKey> Keys => Dictionary.Keys;

        public ICollection<TValue> Values => Dictionary.Values;

        public void Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
            OnItemAdded?.Invoke(key, value);
        }

        public bool Remove(TKey key)
        {
            if (Dictionary.TryGetValue(key, out var value))
            {
                Dictionary.Remove(key);
                OnItemRemoved?.Invoke(key, value);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public void Clear()
        {
            Dictionary.Clear();
            OnCleared?.Invoke();
        }

        ~GObservableDictionary()
        {
            OnItemAdded = null;
            OnItemUpdated = null;
            OnItemRemoved = null;
            OnCleared = null;
        }
    }
}