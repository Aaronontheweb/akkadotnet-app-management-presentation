// -----------------------------------------------------------------------
//  <copyright file="SubscriptionCommandBase.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.BCF.Abstractions.Messages.Commands;
using Akka.BCF.Domains.Subscriptions.State;

namespace Akka.BCF.Domains.Subscriptions.Messages;

public interface ISubscriptionCommand : IDomainCommand<string>
{
}

public abstract record SubscriptionCommandBase(string EntityId, IActorRef? ReplyTo) : ISubscriptionCommand
{
}

public sealed record CreateSubscription(string EntityId, string ProductId, string UserId, SubscriptionInterval Interval, decimal PaymentAmount, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);

public sealed record CancelSubscription(string EntityId, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);

public sealed record CheckSubscriptionStatus(string EntityId, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);

/// <summary>
/// Reactivates a cancelled subscription
/// </summary>am>
public sealed record ResumeSubscription(string EntityId, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);