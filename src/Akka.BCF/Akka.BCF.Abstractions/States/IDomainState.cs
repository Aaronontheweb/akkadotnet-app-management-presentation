// -----------------------------------------------------------------------
//  <copyright file="IDomainState.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Akka.BCF.Abstractions.States;

public interface IDomainState<out TKey> : IHasEntityKey<TKey> where TKey : notnull
{
}

public interface IDomainStateWithSnapshot<out TKey, out TSnapshot> : IDomainState<TKey>, ITakeSnapshots<TSnapshot>
    where TKey : notnull 
    where TSnapshot : new()
{
    
}