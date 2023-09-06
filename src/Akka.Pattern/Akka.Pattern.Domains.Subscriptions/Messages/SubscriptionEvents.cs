namespace Akka.Pattern.Domains.Subscriptions.Messages;

public interface ISubscriptionEvent : IWithSubscriptionId
{
}


public static class SubscriptionEvents
{
/*
 * These events really originate from the Payments domain, but we're re-modeling them into the Subscriptions domain for separation of concerns.
 */
    public sealed record SubscriptionPaymentProcessed(string SubscriptionId, DateTimeOffset PaymentDate,
        decimal PaymentAmount) : IWithSubscriptionId;

    public sealed record SubscriptionPaymentFailed(string SubscriptionId, DateTimeOffset PaymentDate, decimal PaymentAmount) : IWithSubscriptionId;

    public sealed record SubscriptionCreated(string SubscriptionId, string ProductId, string UserId, SubscriptionInterval Interval, decimal PaymentAmount) : IWithSubscriptionId;

    public sealed record SubscriptionCancelled(string SubscriptionId) : IWithSubscriptionId;

    public sealed record SubscriptionResumed(string SubscriptionId) : IWithSubscriptionId;

    /// <summary>
    /// Failure to pay
    /// </summary>
    public sealed record SubscriptionSuspended(string SubscriptionId) : IWithSubscriptionId;
}