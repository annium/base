namespace Annium.Data.Operations;

/// <summary>
/// Represents a result with associated data
/// </summary>
/// <typeparam name="TD">The type of the associated data</typeparam>
public interface IResult<TD> : IResultBase<IResult<TD>>, IDataResultBase<TD>;

/// <summary>
/// Represents a result without associated data
/// </summary>
public interface IResult : IResultBase<IResult>, IResultBase;
