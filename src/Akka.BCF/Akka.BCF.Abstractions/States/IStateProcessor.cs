// -----------------------------------------------------------------------
//  <copyright file="IStateProcessor.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.BCF.Abstractions.Messages.Commands;
using Akka.BCF.Abstractions.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Akka.BCF.Abstractions.States;

/// <summary>
/// Used to process commands and generate events from a given state.
/// </summary>
public interface IStateProcessor<TKey, in TState, in TCommand, TEvent> 
    where TState : IDomainState<TKey> 
    where TCommand : IDomainCommand<TKey>
    where TEvent : IDomainEvent<TKey>
    where TKey : notnull
{
    Task<(bool canProcess, TEvent[] events, string message)> ProcessAsync(TState state, TCommand command);
}

public abstract class StateProcessorBase<TKey, TState, TCommand, TEvent> : IStateProcessor<TKey, TState, TCommand, TEvent>
    where TState : IDomainState<TKey>
    where TCommand : IDomainCommand<TKey>
    where TEvent : IDomainEvent<TKey>
    where TKey : notnull
{
    protected readonly IStateValidator<TKey, TState, TCommand> Validator;
    protected readonly ILogger<StateProcessorBase<TKey, TState, TCommand, TEvent>> Logger;

    protected StateProcessorBase(IStateValidator<TKey, TState, TCommand> validator, ILogger<StateProcessorBase<TKey, TState, TCommand, TEvent>> logger)
    {
        Validator = validator;
        Logger = logger;
    }

    public virtual async Task<(bool canProcess, TEvent[] events, string message)> ProcessAsync(TState state,
        TCommand command)
    {
        var validationResult = await Validator.ValidateAsync(state, command);
        if (!validationResult.IsValid)
        {
            var message = string.Join(Environment.NewLine, validationResult.ValidationErrors ?? Enumerable.Empty<string>());
            return (false, Array.Empty<TEvent>(), message);
        }
        
        return await ProcessInternalAsync(state, command);
    }
    
    protected abstract ValueTask<(bool canProcess, TEvent[] events, string message)> ProcessInternalAsync(TState state, TCommand command);
}