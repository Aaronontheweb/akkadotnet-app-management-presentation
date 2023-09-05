// -----------------------------------------------------------------------
//  <copyright file="EventSourcedActorBase.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.BCF.Abstractions.Messages.Commands;
using Akka.BCF.Abstractions.Messages.Events;
using Akka.BCF.Abstractions.States;
using Akka.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Akka.BCF.Abstractions.Actors.ActorTestCommands;

namespace Akka.BCF.Abstractions.Actors;

public abstract class EventSourcedActorBase<TKey, TState, TSnapshot, TEvent, TCommand> : ReceivePersistentActor
    where TState : IDomainStateWithSnapshot<TKey, TSnapshot>
    where TSnapshot : new()
    where TEvent : IDomainEvent<TKey>
    where TCommand : IDomainCommand<TKey>
    where TKey : notnull
{
    protected EventSourcedActorBase(TKey key, 
        IStateProcessor<TKey, TState, TCommand, TEvent> stateProcessor,
        IStateBuilder<TKey, TState, TEvent> stateBuilder,
        IOptions<ActorConfig> actorOptions,
        ILogger<EventSourcedActorBase<TKey, TState, TSnapshot, TEvent, TCommand>> logger)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        State = CreateInitialState(key);
        StateProcessor = stateProcessor;
        StateBuilder = stateBuilder;
        Config = actorOptions.Value;
        Logger = logger;
        
        DefaultRecovers();
        DefaultCommands();
    }
    
    protected ActorConfig Config { get; }

    protected virtual ILogger<EventSourcedActorBase<TKey, TState, TSnapshot, TEvent, TCommand>> Logger { get; }

    protected virtual IStateProcessor<TKey, TState, TCommand, TEvent> StateProcessor { get; }
    protected virtual IStateBuilder<TKey, TState, TEvent> StateBuilder { get; }

    /// <summary>
    /// Used to populate the default state of this actor.
    /// </summary>
    /// <param name="key">The unique id of this actor</param>
    protected abstract TState CreateInitialState(TKey key);

    protected TState State { get; set; }
    
    /// <summary>
    /// Creates a new instance of this state from a snapshot
    /// </summary>
    /// <param name="snapshot">The snapshot, typically recovered from Akka.Persistence.</param>
    /// <returns>A new instance of this state from a snapshot - should replace the current instance inside actor state.</returns>
    protected abstract TState FromSnapshot(TSnapshot snapshot);

    protected override bool AroundReceive(Receive receive, object message)
    {
        if(Config.LogMessages)
            Logger.LogDebug("Received message {@Message} for {ActorType} while in state {@State}", message, GetType(), State);
        
        return base.AroundReceive(receive, message);
    }

    private void DefaultRecovers()
    {
        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is TSnapshot snap)
            {
                Logger.LogDebug("Recovering snapshot for {ActorType} {@Snapshot}", GetType(), offer);
                State = FromSnapshot(snap);
                return;
            }
            
            Logger.LogCritical("Received snapshot of type {SnapshotType} but expected {ExpectedType}", offer.Snapshot.GetType(), typeof(TSnapshot));
            if(Config.ThrowOnSnapshotFailure)
                throw new InvalidOperationException($"Received snapshot of type {offer.Snapshot.GetType()} but expected {typeof(TSnapshot)}");
        });
        
        Recover<TEvent>(e =>
        {
            StateBuilder.Apply(State, e);
            Logger.LogDebug("Recovering event for {ActorType} {@Event}", GetType(), e);
        });
    }

    private void DefaultCommands()
    {
        CommandAsync<TCommand>(async cmd =>
        {
            var (canProcess, @event, message) = await StateProcessor.ProcessAsync(State, cmd);
            if (!canProcess)
            {
                Logger.LogWarning("Cannot process command {@Command} for {ActorType} {@State} - {Message}", cmd, GetType(), State, message);
                cmd.ReplyTo?.Tell(new CommandFailed<TKey>(cmd, message));
                return;
            }

            if (@event == null)
            {
                Logger.LogWarning("Cannot process command {@Command} for {ActorType} {@State} - {Message}. Event payload was null.", cmd, GetType(), State, message);
                return;
            }
            
            Persist(@event, e =>
            {
                StateBuilder.Apply(State, e);
                Logger.LogDebug("Persisted event {@Event} for {ActorType} {@State}", e, GetType(), State);
                cmd.ReplyTo?.Tell(new CommandSucceeded<TKey>(cmd));

                if (LastSequenceNr % Config.MessagesPerSnapshot == 0)
                    SaveSnapshot(State.ToSnapshot());
            });
        });
        
        DomainCommandHandlers();

        Command<FetchCurrentState<TKey>>(fetchCurrentState =>
        {
            fetchCurrentState.ReplyTo?.Tell(new CurrentStateSnapshot<TKey, TSnapshot>(fetchCurrentState.EntityId, State.ToSnapshot(), fetchCurrentState));
        });
        
        Command<SetState<TKey, TState>>(setState =>
        {
            State = setState.NextState;
            setState.ReplyTo?.Tell(new CommandSucceeded<TKey>(setState));
        });
        
        Command<SaveSnapshotSuccess>(success =>
        {
            Logger.LogDebug("Successfully saved snapshot for {ActorType} {@State}", GetType(), State);
            
            // delete all snapshots and events that happened before this one
            if(Config.DeleteSnapshotsOnSuccessfulSnapshot)
                DeleteSnapshots(new SnapshotSelectionCriteria(success.Metadata.SequenceNr, success.Metadata.Timestamp));
            
            if(Config.DeleteMessagesOnSuccessfulSnapshot)
                DeleteMessages(success.Metadata.SequenceNr);
        });
        
        Command<SaveSnapshotFailure>(failure =>
        {
            Logger.LogError(failure.Cause, "Failed to save snapshot for {ActorType} {@State}", GetType(), State);
        });
        
        Command<DeleteMessagesSuccess>(success =>
        {
            Logger.LogDebug("Successfully deleted messages for {ActorType} {@State}", GetType(), State);
        });
        
        Command<DeleteMessagesFailure>(failure =>
        {
            Logger.LogError(failure.Cause, "Failed to delete messages for {ActorType} {@State}", GetType(), State);
        });
    }

    /// <summary>
    /// All domain-specific commands go here, if needed
    /// </summary>
    protected virtual void DomainCommandHandlers()
    {
        
    }
}