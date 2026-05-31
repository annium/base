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
    public Mapping Repack(Expression ex) =>
        source =>
        {
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
        source =>
        {
            // a repacked lambda must be parameterized by the substituted source — only a ParameterExpression
            // can become a Lambda parameter; non-parameter sources here would silently produce a malformed tree.
            if (source is not ParameterExpression parameter)
                throw new InvalidOperationException(
                    $"Lambda repack requires a ParameterExpression source; got {source.GetType().Name}."
                );

            return Expression.Lambda(Repack(ex.Body)(parameter), parameter);
        };

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
        source =>
            // ex.Expression is null for static member access — there is no source-derived sub-expression to repack
            ex.Expression
                is null
                ? Expression.MakeMemberAccess(null, ex.Member)
                : Expression.MakeMemberAccess(Repack(ex.Expression)(source), ex.Member);

    /// <summary>
    /// Repacks a member initialization expression
    /// </summary>
    /// <param name="ex">The member init expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping MemberInit(MemberInitExpression ex) =>
        source =>
            Expression.MemberInit(
                (NewExpression)Repack(ex.NewExpression)(source),
                ex.Bindings.Select(b => RepackBinding(b, source))
            );

    /// <summary>
    /// Repacks a member binding, recursing into nested list and member bindings so that any
    /// embedded source expressions are substituted rather than silently passed through.
    /// </summary>
    /// <param name="binding">The member binding to repack.</param>
    /// <param name="source">The current source expression.</param>
    /// <returns>The repacked member binding.</returns>
    private MemberBinding RepackBinding(MemberBinding binding, Expression source) =>
        binding switch
        {
            MemberAssignment ma => ma.Update(Repack(ma.Expression)(source)),
            MemberListBinding ml => ml.Update(
                ml.Initializers.Select(i => i.Update(i.Arguments.Select(a => Repack(a)(source))))
            ),
            MemberMemberBinding mm => mm.Update(mm.Bindings.Select(inner => RepackBinding(inner, source))),
            _ => throw new InvalidOperationException($"Unsupported MemberBinding kind: {binding.BindingType}"),
        };

    /// <summary>
    /// Repacks a method call expression
    /// </summary>
    /// <param name="ex">The method call expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping MethodCall(MethodCallExpression ex) =>
        source =>
        {
            // ex.Object is null for static method calls — Expression.Call has a no-instance overload for that
            var args = ex.Arguments.Select(a => Repack(a)(source)).ToArray();
            return ex.Object is null
                ? Expression.Call(ex.Method, args)
                : Expression.Call(Repack(ex.Object)(source), ex.Method, args);
        };

    /// <summary>
    /// Repacks a new object expression
    /// </summary>
    /// <param name="ex">The new expression</param>
    /// <returns>The repacked mapping</returns>
    private Mapping New(NewExpression ex) =>
        source =>
            // NewExpression.Constructor is null when the expression names a value-type default ctor
            // via Expression.New(Type) — fall through to the typed New(Type) overload so struct-shaped
            // MapWith bodies (`x => new MyStruct { ... }`) repack without throwing.
            ex.Constructor
                is null
                ? Expression.New(ex.Type)
                : Expression.New(ex.Constructor, ex.Arguments.Select(a => Repack(a)(source)));

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
