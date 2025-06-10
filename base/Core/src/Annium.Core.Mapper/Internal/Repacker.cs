using System;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Repacks given expression with given source expression, replacing parameter expressions to given source expression
/// </summary>
internal class Repacker : IRepacker
{
    /// <summary>
    /// Repacks an expression into a mapping configuration
    /// </summary>
    /// <param name="ex">The expression to repack</param>
    /// <returns>The repacked mapping</returns>
    public Mapping Repack(Expression? ex) =>
        source =>
        {
            if (ex is null)
                return null!;

            return ex switch
            {
                BinaryExpression binary => Binary(binary)(source),
                ConditionalExpression conditional => Conditional(conditional)(source),
                ConstantExpression constant => constant,
                LambdaExpression lambda => Lambda(lambda)(source),
                ListInitExpression listInit => ListInit(listInit)(source),
                MemberExpression member => Member(member)(source),
                MemberInitExpression memberInit => MemberInit(memberInit)(source),
                MethodCallExpression call => MethodCall(call)(source),
                NewExpression @new => New(@new)(source),
                NewArrayExpression newArray => NewArray(newArray)(source),
                ParameterExpression => source,
                UnaryExpression unary => Unary(unary)(source),
                _ => throw new InvalidOperationException($"Can't repack {ex.NodeType} expression"),
            };
        };

    /// <summary>
    /// Repacks a binary expression
    /// </summary>
    /// <param name="ex">The binary expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping Binary(BinaryExpression ex) =>
        source =>
            Expression.MakeBinary(
                ex.NodeType,
                Repack(ex.Left)(source),
                Repack(ex.Right)(source),
                ex.IsLiftedToNull,
                ex.Method,
                ex.Conversion
            );

    /// <summary>
    /// Repacks a conditional expression
    /// </summary>
    /// <param name="ex">The conditional expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping Conditional(ConditionalExpression ex) =>
        source =>
            Expression.Condition(
                Repack(ex.Test)(source),
                Repack(ex.IfTrue)(source),
                Repack(ex.IfFalse)(source),
                ex.Type
            );

    /// <summary>
    /// Repacks a lambda expression
    /// </summary>
    /// <param name="ex">The lambda expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping Lambda(LambdaExpression ex) =>
        source => Expression.Lambda(Repack(ex.Body)(source), (ParameterExpression)source);

    /// <summary>
    /// Repacks a list initialization expression
    /// </summary>
    /// <param name="ex">The list init expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping ListInit(ListInitExpression ex) =>
        source =>
            Expression.ListInit(
                (NewExpression)Repack(ex.NewExpression)(source),
                ex.Initializers.Select(x => x.Update(x.Arguments.Select(a => Repack(a)(source))))
            );

    /// <summary>
    /// Repacks a member access expression
    /// </summary>
    /// <param name="ex">The member expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping Member(MemberExpression ex) =>
        source => Expression.MakeMemberAccess(Repack(ex.Expression)(source), ex.Member);

    /// <summary>
    /// Repacks a member initialization expression
    /// </summary>
    /// <param name="ex">The member init expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping MemberInit(MemberInitExpression ex) =>
        source =>
            Expression.MemberInit(
                (NewExpression)Repack(ex.NewExpression)(source),
                ex.Bindings.Select(b =>
                {
                    if (b is MemberAssignment ma)
                        return ma.Update(Repack(ma.Expression)(source));

                    return b;
                })
            );

    /// <summary>
    /// Repacks a method call expression
    /// </summary>
    /// <param name="ex">The method call expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping MethodCall(MethodCallExpression ex) =>
        source =>
            Expression.Call(
                Repack(ex.Object)(source),
                ex.Method,
                ex.Arguments.Select(a => Repack(a)(source)).ToArray()
            );

    /// <summary>
    /// Repacks a new object expression
    /// </summary>
    /// <param name="ex">The new expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping New(NewExpression ex) =>
        source => Expression.New(ex.Constructor!, ex.Arguments.Select(a => Repack(a)(source)));

    /// <summary>
    /// Repacks a new array expression
    /// </summary>
    /// <param name="ex">The new array expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping NewArray(NewArrayExpression ex) =>
        source => ex.Update(ex.Expressions.Select(e => Repack(e)(source)));

    /// <summary>
    /// Repacks a unary expression
    /// </summary>
    /// <param name="ex">The unary expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping Unary(UnaryExpression ex) =>
        source => Expression.MakeUnary(ex.NodeType, Repack(ex.Operand)(source), ex.Type, ex.Method);
}
