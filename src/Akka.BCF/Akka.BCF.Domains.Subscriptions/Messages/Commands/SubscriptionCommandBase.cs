// -----------------------------------------------------------------------
//  <copyright file="SubscriptionCommandBase.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.BCF.Abstractions.Messages.Commands;

namespace Akka.BCF.Domains.Subscriptions.Messages.Commands;

public abstract record SubscriptionCommandBase(string EntityId, IActorRef? ReplyTo) : IDomainCommand<string>
{
}

public sealed record CreateSubscription(string EntityId, string ProductId, string UserId, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);

public sealed record CancelSubscription(string EntityId, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);

public sealed record SuspendSubscription(string EntityId, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);

public sealed record ResumeSubscription(string EntityId, IActorRef? ReplyTo = null) : SubscriptionCommandBase(EntityId, ReplyTo);