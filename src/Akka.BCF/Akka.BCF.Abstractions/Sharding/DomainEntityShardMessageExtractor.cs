using Akka.Cluster.Sharding;

namespace Akka.BCF.Abstractions.Sharding;

public sealed class DomainEntityShardMessageExtractor : HashCodeMessageExtractor
{
    public DomainEntityShardMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
    {
    }

    public override string EntityId(object message)
    {
        if(message is IHasEntityKey<string> str) 
            return str.EntityId;
        
        if(message is IHasEntityKey<Guid> guid) 
            return guid.EntityId.ToString();
        
        throw new ArgumentException($"Message {message.GetType().Name} does not implement {nameof(IHasEntityKey<string>)}");
    }
}