using System.Collections.Generic;
using GonDraz.Events;

namespace GonDraz.Observable
{
    public class GObservableList<T>
    {
        private List<T> _items = new();
        public GEvent<T> OnAdd;
        public GEvent OnClear;
        public GEvent<T> OnRemove;

        public GObservableList(GEvent<T> onAdd = null, GEvent<T> onRemove = null, GEvent onClear = null)
        {
            OnAdd = onAdd;
            OnRemove = onRemove;
            OnClear = onClear;
        }

        public IEnumerable<T> Items => _items;

        public int Count => _items.Count;
        public T this[int index] => _items[index];

        public void Add(T item)
        {
            _items.Add(item);
            OnAdd?.Invoke(item);
        }

        public void Remove(T item)
        {
            _items.Remove(item);
            OnRemove?.Invoke(item);
        }

        public void Clear()
        {
            _items.Clear();
            OnClear?.Invoke();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }


        ~GObservableList()
        {
            _items = null;
            OnAdd = null;
            OnRemove = null;
            OnClear = null;
        }
    }
}