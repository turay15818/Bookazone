using System.Linq.Expressions;

namespace Bookazone.Application.Common.Shared.Utils;

public static class SearchExpressionBuilder
{
    public static Expression<Func<T, bool>> BuildSearchExpression<T>(string search)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? body = null;

        foreach (var property in typeof(T).GetProperties()
                     .Where(p => p.PropertyType == typeof(string)))
        {
            var prop = Expression.Property(parameter, property);
            var searchConstant = Expression.Constant(search.ToLower());
            var toLowerCall = Expression.Call(prop, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            var containsCall = Expression.Call(toLowerCall, nameof(string.Contains), Type.EmptyTypes, searchConstant);
            body = body == null ? containsCall : Expression.OrElse(body, containsCall);
        }
        if (body == null)
            body = Expression.Constant(true);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}