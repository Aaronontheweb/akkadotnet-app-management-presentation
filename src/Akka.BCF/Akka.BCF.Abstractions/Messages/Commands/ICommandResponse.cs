// -----------------------------------------------------------------------
//  <copyright file="ICommandResponse.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.BCF.Abstractions.Messages.Commands;

public interface ICommandResponse<out TKey> : IHasEntityKey<TKey> where TKey : notnull
{
    bool Success { get; }
    
    IDomainCommand<TKey> Command { get; }
    
    string? Message { get; }
}

public sealed record CommandFailed<TKey>
    (IDomainCommand<TKey> Command, string? Message = null) : ICommandResponse<TKey> where TKey : notnull
{
    public bool Success => false;
    public TKey EntityId => Command.EntityId;
}

public sealed record CommandSucceeded<TKey>
    (IDomainCommand<TKey> Command, string? Message = null) : ICommandResponse<TKey> where TKey : notnull
{
    public bool Success => true;
    public TKey EntityId => Command.EntityId;
}