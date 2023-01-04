using System;

namespace ArcCreate
{
    public class State<T>
    {
        private T value;

        public event Action<T> OnValueChange;

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChange?.Invoke(value);
            }
        }
    }
}