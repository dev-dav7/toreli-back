using LinqToDB.Mapping;

namespace toreligo.Domain.Database;

public abstract class EntityWithId : IEntityWithId
{
    [Column("id")]
    [PrimaryKey]
    [SequenceName("ids")]
    [Identity]
    public long Id { get; set; }
}