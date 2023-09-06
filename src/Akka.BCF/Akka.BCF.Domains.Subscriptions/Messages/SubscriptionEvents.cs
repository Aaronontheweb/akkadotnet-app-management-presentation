using Akka.BCF.Abstractions.Messages.Events;
using Akka.BCF.Domains.Subscriptions.State;

namespace Akka.BCF.Domains.Subscriptions.Messages;

public interface ISubscriptionEvent : IDomainEvent<string>
{
}

public abstract record SubscriptionEventBase(string EntityId) : ISubscriptionEvent
{
}

/*
 * These events really originate from the Payments domain, but we're re-modeling them into the Subscriptions domain for separation of concerns.
 */
public sealed record SubscriptionPaymentProcessed(string EntityId, DateTimeOffset PaymentDate, decimal PaymentAmount) : SubscriptionEventBase(EntityId);

public sealed record SubscriptionPaymentFailed(string EntityId, DateTimeOffset PaymentDate, decimal PaymentAmount) : SubscriptionEventBase(EntityId);

public sealed record SubscriptionCreated(string EntityId, string ProductId, string UserId, SubscriptionInterval Interval, decimal PaymentAmount) : SubscriptionEventBase(EntityId);

public sealed record SubscriptionCancelled(string EntityId) : SubscriptionEventBase(EntityId);

public sealed record SubscriptionResumed(string EntityId) : SubscriptionEventBase(EntityId);

/// <summary>
/// Failure to pay
/// </summary>
public sealed record SubscriptionSuspended(string EntityId) : SubscriptionEventBase(EntityId);