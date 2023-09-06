namespace Akka.Pattern.Domains.Subscriptions.Messages;

public enum SubscriptionStatus
{
    /// <summary>
    /// We're in the process of being created
    /// </summary>
    NotStarted,
    Active,
    Cancelled,
    SuspendedNotPaid
}