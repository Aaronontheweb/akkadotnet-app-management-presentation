// -----------------------------------------------------------------------
//  <copyright file="IDomainEvent.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.BCF.Abstractions.Messages;

public interface IDomainEvent<out TKey> : IHasEntityKey<TKey> where TKey : notnull
{
    
}