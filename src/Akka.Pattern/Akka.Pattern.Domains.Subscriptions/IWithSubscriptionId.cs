namespace Akka.Pattern.Domains.Subscriptions;

public readonly record struct SubscriptionId(string Id);

/// <summary>
/// All messages that are keyed to a specific Subscription
/// </summary>
public interface IWithSubscriptionId
{
    SubscriptionId SubscriptionId { get; }
}