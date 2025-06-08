using System;

namespace Annium.Data.Tables;

/// <summary>
/// Flags enumeration defining the operations allowed on a table
/// </summary>
[Flags]
public enum TablePermission
{
    /// <summary>
    /// All operations are allowed
    /// </summary>
    All = Init | Add | Update | Delete,

    /// <summary>
    /// Initialization operations are allowed
    /// </summary>
    Init = 1 << 0,

    /// <summary>
    /// Add operations are allowed
    /// </summary>
    Add = 1 << 1,

    /// <summary>
    /// Update operations are allowed
    /// </summary>
    Update = 1 << 2,

    /// <summary>
    /// Delete operations are allowed
    /// </summary>
    Delete = 1 << 3,
}
