using Akka.Cluster.Sharding;
using Akka.Pattern.Domains.Subscriptions.Messages;

namespace Akka.Pattern.Domains.Subscriptions.Config;

public sealed class SubscriptionMessageExtractor : HashCodeMessageExtractor
{
    public const int ShardCount = 50;
    
    public SubscriptionMessageExtractor() : base(ShardCount)
    {
    }
    
    public SubscriptionMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
    {
    }

    public override string EntityId(object message)
    {
        return (message switch
        {
            IWithSubscriptionId withSubscriptionId => withSubscriptionId.SubscriptionId,
            _ => string.Empty
        });
    }
}