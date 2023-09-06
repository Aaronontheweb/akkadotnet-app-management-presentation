using Akka.BCF.Abstractions.Actors;
using Akka.BCF.Abstractions.States;
using Akka.BCF.Domains.Subscriptions.Messages;
using Akka.BCF.Domains.Subscriptions.State;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Akka.BCF.Domains.Subscriptions.Actors;

public sealed class SubscriptionStateActor : EventSourcedActorBase<string, SubscriptionState, SubscriptionSnapshot, ISubscriptionEvent, ISubscriptionCommand>
{
    public SubscriptionStateActor(string key, IStateProcessor<string, SubscriptionState, ISubscriptionCommand, ISubscriptionEvent> stateProcessor, IStateBuilder<string, SubscriptionState, ISubscriptionEvent> stateBuilder, IOptions<ActorConfig> actorOptions, ILogger<EventSourcedActorBase<string, SubscriptionState, SubscriptionSnapshot, ISubscriptionEvent, ISubscriptionCommand>> logger) : base(key, stateProcessor, stateBuilder, actorOptions, logger)
    {
        PersistenceId = $"subscription-{key}";
    }

    public override string PersistenceId { get; }
    protected override SubscriptionState CreateInitialState(string key)
    {
        return new SubscriptionState(key);
    }

    protected override SubscriptionState FromSnapshot(SubscriptionSnapshot snapshot)
    {
        return new SubscriptionState(snapshot.SubscriptionId)
        {
            ProductId = snapshot.ProductId,
            UserId = snapshot.UserId,
            Status = snapshot.Status,
            UpcomingPaymentAmount = snapshot.UpcomingPaymentAmount,
            NextPaymentDate = snapshot.NextPaymentDate,
            Interval = snapshot.Interval
        };
    }
}