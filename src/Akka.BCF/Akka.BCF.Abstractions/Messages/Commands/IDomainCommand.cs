// -----------------------------------------------------------------------
//  <copyright file="IDomainCommand.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;

namespace Akka.BCF.Abstractions.Messages.Commands;

public interface IDomainCommand<out TKey> : IHasEntityKey<TKey> where TKey : notnull
{
    public IActorRef? ReplyTo { get; }
}

public interface IDomainCommandWithPayload<out TKey, out TPayload> : IHasEntityKey<TKey>
    where TPayload:notnull where TKey : notnull
{
    TPayload Payload { get; }
    
    public IActorRef? ReplyTo { get; }
}