// -----------------------------------------------------------------------
//  <copyright file="ActorTestCommands.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.BCF.Abstractions.Messages.Commands;
using Akka.BCF.Abstractions.States;

namespace Akka.BCF.Abstractions.Actors;

public static class ActorTestCommands
{
    public sealed record SetState<TKey, TState>(TState NextState, IActorRef? ReplyTo = null) : IDomainCommand<TKey>
        where TKey : notnull
        where TState : IDomainState<TKey>
    {
        public TKey EntityId => NextState.EntityId;
    }
    
    public sealed record FetchCurrentState<TKey>(IActorRef? ReplyTo = null) : IDomainCommand<TKey> where TKey : notnull
    {
        public TKey EntityId { get; init; } = default!;
    }
    
    public sealed record CurrentStateSnapshot<TKey, TSnapshot>(TKey EntityId, TSnapshot Snapshot, IDomainCommand<TKey> Command, string? Message = null) : ICommandResponse<TKey>
        where TKey : notnull
        where TSnapshot : new()
    {
        public TKey EntityId { get; init; } = default!;
        public bool Success => true;
    }
}