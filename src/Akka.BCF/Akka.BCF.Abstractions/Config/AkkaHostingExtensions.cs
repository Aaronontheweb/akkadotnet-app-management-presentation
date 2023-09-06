using Akka.BCF.Abstractions.Actors;
using Akka.BCF.Abstractions.Messages.Commands;
using Akka.BCF.Abstractions.Messages.Events;
using Akka.BCF.Abstractions.Sharding;
using Akka.BCF.Abstractions.States;
using Akka.Hosting;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Microsoft.Extensions.DependencyInjection;

namespace Akka.BCF.Abstractions.Config;

public static class AkkaHostingExtensions
{
    // TODO: make configurable via options class
    public const int ShardCount = 50;
    
    public static IServiceCollection AddDomainServices<TKey, TState, TEvent, TCommand,
        TStateBuilder, TStateValidator, TStateProcessor>(this IServiceCollection services)
        where TStateBuilder : class, IStateBuilder<TKey, TState, TEvent>
        where TStateValidator : class, IStateValidator<TKey, TState, TCommand>
        where TStateProcessor : class, IStateProcessor<TKey, TState, TCommand, TEvent>
        where TKey : notnull
        where TState : IDomainState<TKey>
        where TEvent : IDomainEvent<TKey>
        where TCommand : IDomainCommand<TKey>
    {
        services.AddSingleton<IStateBuilder<TKey, TState, TEvent>, TStateBuilder>();
        services.AddSingleton<IStateValidator<TKey, TState, TCommand>, TStateValidator>();
        services.AddSingleton<IStateProcessor<TKey, TState, TCommand, TEvent>, TStateProcessor>();
        return services;
    }

    
    public static AkkaConfigurationBuilder AddDomainEntity<TKey, TState, TSnapshot, TEvent, TCommand, TActor,
        TStateBuilder, TStateValidator, TStateProcessor>(
        this AkkaConfigurationBuilder builder,
        string shardRole)
        where TActor : EventSourcedActorBase<TKey, TState, TSnapshot, TEvent, TCommand>
        where TKey : notnull
        where TState : IDomainStateWithSnapshot<TKey, TSnapshot>
        where TSnapshot : new()
        where TEvent : IDomainEvent<TKey>
        where TCommand : IDomainCommand<TKey>
    {
        return builder.WithShardRegion<TActor>(nameof(TActor), 
            (system, registry, resolver) => s => resolver.Props<TActor>(s),
            new DomainEntityShardMessageExtractor<TKey>(ShardCount), new ShardOptions()
            {
                StateStoreMode = StateStoreMode.DData,
                Role = shardRole
            });
    }
}