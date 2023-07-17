public readonly struct Option<T>
{
    public readonly bool HasValue;

    public readonly T Value;

    private Option(T value, bool hasValue)
    {
        Value = value;
        HasValue = hasValue;
    }

    public static implicit operator Option<T>(T value)
        => new Option<T>(value, true);

    public static Option<T> Some(T value)
        => new Option<T>(value, true);

    public static Option<T> None()
        => new Option<T>(default, false);

    public T Or(T alternate)
    {
        return HasValue ? Value : alternate;
    }
}