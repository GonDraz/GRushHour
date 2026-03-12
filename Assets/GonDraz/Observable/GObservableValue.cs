using System.Collections.Generic;
using GonDraz.Events;

namespace GonDraz.Observable
{
    public class GObservableValue<T>
    {
        private T _value;
        public GEvent<T, T> OnChanged;

        public GObservableValue(T initialValue = default, GEvent<T, T> onChanged = null)
        {
            _value = initialValue;
            OnChanged = onChanged;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                var prev = _value;
                _value = value;
                OnChanged?.Invoke(prev, _value);
            }
        }

        public override string ToString()
        {
            return _value != null ? _value.ToString() : "null";
        }

        ~GObservableValue()
        {
            OnChanged = null;
        }
    }
}