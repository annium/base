using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper;

public interface IMapConfigurationBuilder<TS, TD>
{
    void With(Expression<Func<TS, TD>> map);
    void With(Func<IMapContext, Expression<Func<TS, TD>>> map);

    IMapConfigurationBuilder<TS, TD> For<TF>(
        Expression<Func<TD, object>> members,
        Expression<Func<TS, TF>> map
    );

    IMapConfigurationBuilder<TS, TD> For<TF>(
        Expression<Func<TD, object>> members,
        Func<IMapContext, Expression<Func<TS, TF>>> map
    );

    IMapConfigurationBuilder<TS, TD> Ignore(
        Expression<Func<TD, object>> members
    );
}