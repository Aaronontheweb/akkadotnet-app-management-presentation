using Akka.Actor;
using Akka.Pattern.Common;

namespace Akka.Pattern.Domains.Subscriptions.Messages;

public interface ISubscriptionCommand : IWithSubscriptionId, IDomainCommand
{
}

/// <summary>
/// All commands that can be received by the SubscriptionStateActor
/// </summary>
public static class SubscriptionCommands
{
    public sealed record CreateSubscription(string SubscriptionId, string ProductId, string UserId,
        SubscriptionInterval Interval, decimal PaymentAmount, IActorRef? ReplyTo = null) : ISubscriptionCommand;

    public sealed record CancelSubscription(string SubscriptionId, IActorRef? ReplyTo = null) : ISubscriptionCommand;

    public sealed record CheckSubscriptionStatus(string SubscriptionId, IActorRef? ReplyTo = null) : ISubscriptionCommand;

    /// <summary>
    /// Reactivates a cancelled subscription
    /// </summary>am>
    public sealed record ResumeSubscription(string SubscriptionId, IActorRef? ReplyTo = null) : ISubscriptionCommand;
}

public interface ISubscriptionCommandResponse : ICommandResponse, IWithSubscriptionId{}
