using System;

namespace Annium.Data.Tables;

[Flags]
public enum TablePermission
{
    All = Init | Add | Update | Delete,
    Init = 1 << 0,
    Add = 1 << 1,
    Update = 1 << 2,
    Delete = 1 << 3,
}