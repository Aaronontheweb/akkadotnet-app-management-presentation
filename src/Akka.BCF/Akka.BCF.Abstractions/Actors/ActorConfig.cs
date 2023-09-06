// -----------------------------------------------------------------------
//  <copyright file="ActorConfig.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Akka.BCF.Abstractions.Actors;

/// <summary>
/// Config for all actor base classes.
/// </summary>
public class ActorConfig : IValidatableObject
{
    public bool LogMessages { get; set; } = false;
    
    public bool ThrowOnSnapshotFailure { get; set; } = true;
    
    [Range(1, int.MaxValue)]
    public int MessagesPerSnapshot { get; set; } = 100;
    
    public bool DeleteSnapshotsOnSuccessfulSnapshot { get; set; } = true;
    
    public bool DeleteMessagesOnSuccessfulSnapshot { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MessagesPerSnapshot < 1)
            yield return new ValidationResult("MessagesPerSnapshot must be greater than 0.");
    }
}