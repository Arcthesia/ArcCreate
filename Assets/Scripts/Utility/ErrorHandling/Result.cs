using System;

public readonly struct Result<T, E>
    where E : class
{
    public readonly T Value;

    public readonly E Error;

    private Result(T value, E error)
    {
        Value = value;
        Error = error;
    }

    public bool IsOk => Error == null;

    public bool IsError => Error != null;

    public static implicit operator Result<T, E>(T value)
    {
        return new Result<T, E>(value, null);
    }

    public static implicit operator Result<T, E>(E error)
    {
        return new Result<T, E>(default, error);
    }

    public static Result<T, E> Ok(T value) => value;

    public static Result<T, E> Err(E e) => e;

    public T UnwrapOrElse(Func<E, T> errorHandler)
    {
        if (Error == null)
        {
            return Value;
        }

        if (errorHandler == null)
        {
            return default;
        }

        return errorHandler.Invoke(Error);
    }

    public bool TryUnwrap(out T value, out E error)
    {
        value = Value;
        error = Error;

        if (IsError)
        {
            return false;
        }

        return true;
    }

    public Result<T, E> Map(Func<T, T> mapper)
    {
        if (Error == null)
        {
            if (mapper == null)
            {
                return Value;
            }

            return mapper.Invoke(Value);
        }

        return Error;
    }
}

public readonly struct Result<E>
    where E : class
{
    public readonly E Error;

    private Result(E error)
    {
        Error = error;
    }

    public bool IsOk => Error == null;

    public bool IsError => Error != null;

    public static implicit operator Result<E>(E error)
    {
        return new Result<E>(error);
    }

    public static Result<E> Ok() => new Result<E>(null);

    public static Result<E> Err(E e) => e;
}