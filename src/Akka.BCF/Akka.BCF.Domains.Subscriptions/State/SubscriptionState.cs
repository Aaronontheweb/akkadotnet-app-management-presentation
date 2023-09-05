// -----------------------------------------------------------------------
//  <copyright file="SubscriptionState.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.BCF.Abstractions.States;

namespace Akka.BCF.Domains.Subscriptions.State;

public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled,
    SuspendedNotPaid
}

public enum SubscriptionInterval
{
    Monthly,
    Yearly
}

public sealed record SubscriptionSnapshot
{
    public string SubscriptionId { get; init; } = default!;
    
    public string ProductId { get; init; } = default!;
    
    public string UserId { get; init; } = default!;
    
    public DateTimeOffset? ExpirationDate { get; init; }
    
    public SubscriptionStatus Status { get; init; }
    
    public decimal UpcomingPaymentAmount { get; init; }
    
    public DateTimeOffset? NextPaymentDate { get; init; }
}

public class SubscriptionState : IDomainStateWithSnapshot<string, SubscriptionSnapshot>
{
    public SubscriptionState(string entityId)
    {
        EntityId = entityId;
    }

    public string EntityId { get; }
    
    public string ProductId { get; set; } = default!;
    
    public string UserId { get; set; } = default!;
    
    public DateTimeOffset? ExpirationDate { get; set; }
    
    public SubscriptionStatus Status { get; set; }
    
    public decimal UpcomingPaymentAmount { get; set; }
    
    public DateTimeOffset? NextPaymentDate { get; set; }
    public SubscriptionSnapshot ToSnapshot()
    {
        return new SubscriptionSnapshot
        {
            SubscriptionId = EntityId,
            ProductId = ProductId,
            UserId = UserId,
            ExpirationDate = ExpirationDate,
            Status = Status,
            UpcomingPaymentAmount = UpcomingPaymentAmount,
            NextPaymentDate = NextPaymentDate
        };
    }
}