using System.Linq.Expressions;

namespace toreligo.Domain.Database;

public static class EntityStorageHelpers
{
    public static T SinglePretty<T>(this IQueryable<T> source)
    {
        var result = source
            .Take(2)
            .ToArray();
        if (result.Length == 1)
            return result[0];

        throw new Exception($"Find {result.Length} entity");
    }

    public static T Single<T>(this EntityStorage entityStorage, params Expression<Func<T, bool>>[] filters)
        where T : class, IEntityWithId
    {
        return filters.Aggregate(entityStorage.Select<T>(), (q, f) => q.Where(f))
            .SinglePretty();
    }

    public static T GetById<T>(this EntityStorage entityStorage, long id)
        where T : class, IEntityWithId
    {
        return entityStorage.Single<T>(x => x.Id == id);
    }
}