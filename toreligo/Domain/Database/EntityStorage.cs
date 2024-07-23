using System.Collections.Concurrent;
using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Mapping;

namespace toreligo.Domain.Database;

public class EntityStorage
{
    private readonly AppDataConnection appDataConnection;

    public EntityStorage(AppDataConnection appDataConnection)
    {
        this.appDataConnection = appDataConnection;
    }

    public IQueryable<T> Select<T>() where T : class, IEntityWithId
    {
        return appDataConnection.GetTable<T>();
    }

    public long Create<T>(T item) where T : class, IEntityWithId
    {
        if (HasId(typeof(T)))
            item.Id = Convert.ToInt64(appDataConnection.InsertWithIdentity(item));
        else
            appDataConnection.Insert(item);

        return item.Id;
    }

    public void Remove<T>(Expression<Func<T, bool>> filter) where T : class, IEntityWithId
    {
        appDataConnection.GetTable<T>().Delete(filter);
    }

    public T CreateEntity<T>(T entity)
        where T : class, IEntityWithId
    {
        Create(entity);
        return entity;
    }

    public void Save<T>(T item) where T : class, IEntityWithId
    {
        if (item.Id == default)
            throw new InvalidOperationException("Empty id");

        appDataConnection.Update(item);
    }

    public int Update<T>(Expression<Func<T, bool>> filter, Expression<Func<T, T>> setter)
        where T : class, IEntityWithId
    {
        if (setter.Body is not MemberInitExpression)
            throw new InvalidOperationException(
                $"setter must be lambda expression with init entity body, current: [{setter}]");

        return appDataConnection.GetTable<T>().Update(filter, setter);
    }

    private static readonly ConcurrentDictionary<Type, bool> idProps = new();

    private bool HasId(Type type)
    {
        if (idProps.TryGetValue(type, out var hasId))
            return hasId;
        var property = type.GetProperty("Id");
        hasId = property != null && property.IsDefined(typeof(SequenceNameAttribute), false);
        idProps.TryAdd(type, hasId);
        return hasId;
    }
}