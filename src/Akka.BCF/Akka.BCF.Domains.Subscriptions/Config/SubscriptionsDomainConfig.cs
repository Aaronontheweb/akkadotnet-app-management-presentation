using Akka.BCF.Abstractions.Config;
using Akka.BCF.Domains.Subscriptions.Actors;
using Akka.BCF.Domains.Subscriptions.Messages;
using Akka.BCF.Domains.Subscriptions.State;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Akka.BCF.Domains.Subscriptions.Config;

public static class SubscriptionsDomainConfig
{
    public static IServiceCollection ConfigureSubscriptionsServices(this IServiceCollection services)
    {
        return services
            .AddDomainServices<string, SubscriptionState, ISubscriptionEvent, ISubscriptionCommand,
                SubscriptionStateBuilder, SubscriptionValidator, SubscriptionProcessor>();
    }
    
    public static AkkaConfigurationBuilder AddSubscriptionsEntities(this AkkaConfigurationBuilder builder, string entityRole)
    {
        return builder.AddDomainEntity<string, SubscriptionState, SubscriptionSnapshot, ISubscriptionEvent, ISubscriptionCommand, SubscriptionStateActor,
            SubscriptionStateBuilder, SubscriptionValidator, SubscriptionProcessor>("subscriptions", entityRole);
    }
}