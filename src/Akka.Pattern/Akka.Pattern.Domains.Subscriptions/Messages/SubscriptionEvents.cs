using Akka.Pattern.Common;

namespace Akka.Pattern.Domains.Subscriptions.Messages;

public interface ISubscriptionEvent : IWithSubscriptionId
{
}


public static class SubscriptionEvents
{
/*
 * These events really originate from the Payments domain, but we're re-modeling them into the Subscriptions domain for separation of concerns.
 */
    public sealed record SubscriptionPaymentProcessed(SubscriptionId SubscriptionId, DateTimeOffset PaymentDate,
        decimal PaymentAmount) : ISubscriptionEvent;

    public sealed record SubscriptionPaymentFailed(SubscriptionId SubscriptionId, DateTimeOffset PaymentDate, decimal PaymentAmount) : ISubscriptionEvent;

    public sealed record SubscriptionCreated(SubscriptionId SubscriptionId, ProductId ProductId, UserId UserId, SubscriptionInterval Interval, decimal PaymentAmount) : ISubscriptionEvent;

    public sealed record SubscriptionCancelled(SubscriptionId SubscriptionId) :ISubscriptionEvent;

    public sealed record SubscriptionResumed(SubscriptionId SubscriptionId) : ISubscriptionEvent;

    /// <summary>
    /// Failure to pay
    /// </summary>
    public sealed record SubscriptionSuspended(SubscriptionId SubscriptionId) : ISubscriptionEvent;
}