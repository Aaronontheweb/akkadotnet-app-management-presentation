using Akka.Cluster.Sharding;

namespace Akka.BCF.Abstractions.Sharding;

public sealed class DomainEntityShardMessageExtractor<TKey> : HashCodeMessageExtractor where TKey : notnull
{
    public DomainEntityShardMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
    {
    }

    public override string EntityId(object message)
    {
        if(message is IHasEntityKey<TKey> str) 
            return str.EntityId.ToString()!;

        return string.Empty;
    }
}