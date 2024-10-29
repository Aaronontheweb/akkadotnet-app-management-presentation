using Akka.Pattern.Common;

namespace Akka.Pattern.Domains.Subscriptions;

/// <summary>
/// All messages that are keyed to a specific Subscription
/// </summary>
public interface IWithSubscriptionId
{
    SubscriptionId SubscriptionId { get; }
}