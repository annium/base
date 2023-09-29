using System.Collections.Generic;
using Annium.Data.Operations.Internal;

namespace Annium.Data.Operations;

public static class Result
{
    public static IResult Join(params IResultBase[] results) =>
        new Internal.Result().Join(results);

    public static IResult Join(IReadOnlyCollection<IResultBase> results) =>
        new Internal.Result().Join(results);

    public static IResult New() =>
        new Internal.Result();

    public static IResult<TD> New<TD>(TD data) =>
        new Result<TD>(data);

    public static IBooleanResult Success() =>
        new BooleanResult(true);

    public static IBooleanResult Failure() =>
        new BooleanResult(false);

    public static IBooleanResult<TD> Success<TD>(TD data) =>
        new BooleanResult<TD>(true, data);

    public static IBooleanResult<TD> Failure<TD>(TD data) =>
        new BooleanResult<TD>(false, data);

    public static IStatusResult<TS> Status<TS>(TS status) =>
        new StatusResult<TS>(status);

    public static IStatusResult<TS, TD> Status<TS, TD>(TS status, TD data) =>
        new StatusResult<TS, TD>(status, data);
}