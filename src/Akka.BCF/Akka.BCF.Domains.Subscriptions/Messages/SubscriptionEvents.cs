using Akka.Actor;
using Akka.BCF.Abstractions.Messages.Events;
using Akka.BCF.Domains.Subscriptions.State;

namespace Akka.BCF.Domains.Subscriptions.Messages;

public interface ISubscriptionEvent : IDomainEvent<string>
{
}

public abstract record SubscriptionEventBase(string EntityId, IActorRef? ReplyTo) : ISubscriptionEvent
{
}

/*
 * These events really originate from the Payments domain, but we're re-modeling them into the Subscriptions domain for separation of concerns.
 */
public sealed record SubscriptionPaymentProcessed(string EntityId, DateTimeOffset PaymentDate, decimal PaymentAmount, IActorRef? ReplyTo = null) : SubscriptionEventBase(EntityId, ReplyTo);

public sealed record SubscriptionPaymentFailed(string EntityId, DateTimeOffset PaymentDate, decimal PaymentAmount, IActorRef? ReplyTo = null) : SubscriptionEventBase(EntityId, ReplyTo);

public sealed record SubscriptionCreated(string EntityId, string ProductId, string UserId, SubscriptionInterval Interval, IActorRef? ReplyTo = null) : SubscriptionEventBase(EntityId, ReplyTo);

public sealed record SubscriptionCancelled(string EntityId, IActorRef? ReplyTo = null) : SubscriptionEventBase(EntityId, ReplyTo);

public sealed record SubscriptionResumed(string EntityId, IActorRef? ReplyTo = null) : SubscriptionEventBase(EntityId, ReplyTo);

/// <summary>
/// Failure to pay
/// </summary>
public sealed record SubscriptionSuspended(string EntityId, IActorRef? ReplyTo = null) : SubscriptionEventBase(EntityId, ReplyTo);