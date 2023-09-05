// -----------------------------------------------------------------------
//  <copyright file="IStateValidator.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.BCF.Abstractions.Messages;
using Akka.BCF.Abstractions.Messages.Commands;

namespace Akka.BCF.Abstractions.States;

public sealed record ValidationResults(bool IsValid, ImmutableList<string>? ValidationErrors = null);

/// <summary>
/// Used to determine if a given <see cref="IDomainCommand{TKey}"/> is valid for a given <see cref="IDomainState{TKey}"/>.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public interface IStateValidator<TKey, in TState, in TCommand> 
    where TState : IDomainState<TKey> 
    where TCommand : IDomainCommand<TKey>
    where TKey : notnull
{
    ValueTask<ValidationResults> ValidateAsync(TState state, TCommand command);
}

