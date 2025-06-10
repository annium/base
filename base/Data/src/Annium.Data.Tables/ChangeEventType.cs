namespace Annium.Data.Tables;

/// <summary>
/// Specifies the type of change event that occurred in a table
/// </summary>
public enum ChangeEventType
{
    /// <summary>
    /// Initial population of the table with data
    /// </summary>
    Init,

    /// <summary>
    /// Addition or update of an item in the table
    /// </summary>
    Set,

    /// <summary>
    /// Deletion of an item from the table
    /// </summary>
    Delete,
}
