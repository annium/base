using System;

namespace Annium.Net.Types;

public interface IMapperConfig
{
    #region base

    IMapperConfig SetBaseType(Type type, string name);
    bool IsBaseType(Type type);

    #endregion

    #region ignore

    IMapperConfig Ignore(Predicate<Type> matcher);
    bool IsIgnored(Type type);

    #endregion

    #region include

    IMapperConfig Include(Type type);

    #endregion

    #region exclude

    IMapperConfig Exclude(Predicate<Type> matcher);
    bool IsExcluded(Type type);

    #endregion

    #region array

    IMapperConfig SetArray(Type type);
    bool IsArray(Type type);

    #endregion

    #region record

    IMapperConfig SetRecord(Type type);
    bool IsRecord(Type type);

    #endregion
}