// -----------------------------------------------------------------------
//  <copyright file="IHasEntityKey.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.BCF.Abstractions;

public interface IHasEntityKey<out TKey> where TKey : notnull
{
    TKey EntityId { get; }
}