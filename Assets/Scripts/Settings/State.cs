using System;

namespace ArcCreate
{
    public class State<T>
    {
        private T value;

        public State()
        {
            value = default;
        }

        public State(T defaultValue)
        {
            value = defaultValue;
        }

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

        public void SetValueWithoutNotify(T value)
        {
            this.value = value;
        }
    }
}