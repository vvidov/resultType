using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace ResultType;

public class Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;
    private readonly bool _isSuccess;

    private Result(TValue value)
    {
        _value = value;
        _error = default;
        _isSuccess = true;
    }

    private Result(TError error)
    {
        _value = default;
        _error = error;
        _isSuccess = false;
    }

    public TValue Value => _value!;
    public TError? Error => _error;
    public bool HasResult() => _isSuccess;

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public Result<TNewValue, TError> OnSuccess<TNewValue>(Func<TValue, Result<TNewValue, TError>> func)
    {
        if (!_isSuccess)
            return new Result<TNewValue, TError>(_error!);

        return func(_value!);
    }

    public Result<TNewValue, TError> OnSuccess<TNewValue>(Func<TValue, TNewValue> func)
    {
        if (!_isSuccess)
            return new Result<TNewValue, TError>(_error!);

        return func(_value!);
    }

    public void Match(Action<TValue> success, Action<TError> failure)
    {
        if (_isSuccess)
            success(_value!);
        else
            failure(_error!);
    }

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure)
    {
        return _isSuccess ? success(_value!) : failure(_error!);
    }
}