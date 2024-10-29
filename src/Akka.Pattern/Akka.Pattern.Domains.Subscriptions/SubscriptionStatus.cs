namespace Akka.Pattern.Domains.Subscriptions;

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