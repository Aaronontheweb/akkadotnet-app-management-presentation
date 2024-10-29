using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Hosting;
using Akka.Pattern.Common;
using Akka.Pattern.Domains.Subscriptions.Actors;

namespace Akka.Pattern.Domains.Subscriptions.Config;

public static class SubscriptionDomainConfig
{
    public static AkkaConfigurationBuilder AddSubscriptionsEntities(this AkkaConfigurationBuilder builder,
        string entityRole)
    {
        return builder.WithShardRegion<SubscriptionStateActor>("subscriptions",
            (system, registry, resolver) => s => resolver.Props<SubscriptionStateActor>(new SubscriptionId(s)),
            new SubscriptionMessageExtractor(), new ShardOptions()
            {
                StateStoreMode = StateStoreMode.DData,
                Role = entityRole
            }
        );
    }
}