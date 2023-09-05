// -----------------------------------------------------------------------
//  <copyright file="IStateBuilder.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.BCF.Abstractions.Messages;

namespace Akka.BCF.Abstractions.States;

/// <summary>
/// Used to fluently build state from the sum of many events - typically combined with Akka.Persistence
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TEvent"></typeparam>
public interface IStateBuilder<TKey, in TState, in TEvent> 
    where TState:IDomainState<TKey>
    where TEvent:IDomainEvent<TKey>
    where TKey:notnull
{
    IStateBuilder<TKey, TState, TEvent> Apply(TState state, TEvent @event);
}