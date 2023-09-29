using System;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal;

// Summary:
//      Repacks given expression with given source expression, replacing parameter expressions to given source expression
internal class Repacker : IRepacker
{
    public Mapping Repack(Expression? ex) => source =>
    {
        if (ex is null)
            return null!;

        return ex switch
        {
            BinaryExpression binary           => Binary(binary)(source),
            ConditionalExpression conditional => Conditional(conditional)(source),
            ConstantExpression constant       => constant,
            LambdaExpression lambda           => Lambda(lambda)(source),
            ListInitExpression listInit       => ListInit(listInit)(source),
            MemberExpression member           => Member(member)(source),
            MemberInitExpression memberInit   => MemberInit(memberInit)(source),
            MethodCallExpression call         => MethodCall(call)(source),
            NewExpression @new                => New(@new)(source),
            NewArrayExpression newArray       => NewArray(newArray)(source),
            ParameterExpression _             => source,
            UnaryExpression unary             => Unary(unary)(source),
            _                                 => throw new InvalidOperationException($"Can't repack {ex.NodeType} expression"),
        };
    };

    private Mapping Binary(BinaryExpression ex) => source =>
        Expression.MakeBinary(
            ex.NodeType,
            Repack(ex.Left)(source),
            Repack(ex.Right)(source),
            ex.IsLiftedToNull,
            ex.Method,
            ex.Conversion
        );

    private Mapping Conditional(ConditionalExpression ex) => source =>
        Expression.Condition(
            Repack(ex.Test)(source),
            Repack(ex.IfTrue)(source),
            Repack(ex.IfFalse)(source),
            ex.Type
        );

    private Mapping Lambda(LambdaExpression ex) => source =>
        Expression.Lambda(Repack(ex.Body)(source), (ParameterExpression)source);

    private Mapping ListInit(ListInitExpression ex) => source =>
        Expression.ListInit(
            (NewExpression)Repack(ex.NewExpression)(source),
            ex.Initializers.Select(x => x.Update(x.Arguments.Select(a => Repack(a)(source))))
        );

    private Mapping Member(MemberExpression ex) => source =>
        Expression.MakeMemberAccess(Repack(ex.Expression)(source), ex.Member);

    private Mapping MemberInit(MemberInitExpression ex) => source =>
        Expression.MemberInit(
            (NewExpression)Repack(ex.NewExpression)(source),
            ex.Bindings.Select(b =>
            {
                if (b is MemberAssignment ma)
                    return ma.Update(Repack(ma.Expression)(source));

                return b;
            })
        );

    private Mapping MethodCall(MethodCallExpression ex) => source =>
        Expression.Call(Repack(ex.Object)(source), ex.Method, ex.Arguments.Select(a => Repack(a)(source)).ToArray());

    private Mapping New(NewExpression ex) => source =>
        Expression.New(ex.Constructor!, ex.Arguments.Select(a => Repack(a)(source)));

    private Mapping NewArray(NewArrayExpression ex) => source =>
        ex.Update(ex.Expressions.Select(e => Repack(e)(source)));

    private Mapping Unary(UnaryExpression ex) => source =>
        Expression.MakeUnary(ex.NodeType, Repack(ex.Operand)(source), ex.Type, ex.Method);
}