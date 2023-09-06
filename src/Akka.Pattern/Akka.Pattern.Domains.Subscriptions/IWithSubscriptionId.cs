namespace Akka.Pattern.Domains.Subscriptions.Messages;

/// <summary>
/// All messages that are keyed to a specific Subscription
/// </summary>
public interface IWithSubscriptionId
{
    string SubscriptionId { get; }
}