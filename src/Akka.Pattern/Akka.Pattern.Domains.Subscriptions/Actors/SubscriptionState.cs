using Akka.Pattern.Common;
using Akka.Pattern.Domains.Payments;
using Akka.Pattern.Domains.Subscriptions.Messages;
using static Akka.Pattern.Domains.Subscriptions.Messages.SubscriptionCommands;
using static Akka.Pattern.Domains.Subscriptions.Messages.SubscriptionEvents;

namespace Akka.Pattern.Domains.Subscriptions.Actors;

public sealed record SubscriptionState(string SubscriptionId) : IWithSubscriptionId
{
    public string ProductId { get; init; } = default!;

    public string UserId { get; init; } = default!;

    public SubscriptionStatus Status { get; init; }

    public decimal UpcomingPaymentAmount { get; init; }

    public DateTimeOffset? NextPaymentDate { get; init; }

    public SubscriptionInterval Interval { get; init; }
}

public sealed record SubscriptionCommandResponse
    (string SubscriptionId, CommandResult Result, string? Message = null) : ISubscriptionCommandResponse
{
    public static ISubscriptionCommandResponse Success(string subscriptionId) =>
        new SubscriptionCommandResponse(subscriptionId, CommandResult.Success);

    public static ISubscriptionCommandResponse Failure(string subscriptionId, string? message = null) =>
        new SubscriptionCommandResponse(subscriptionId, CommandResult.Failure, message);

    public static ISubscriptionCommandResponse NoOp(string subscriptionId, string? message = null) =>
        new SubscriptionCommandResponse(subscriptionId, CommandResult.NoOp, message);
}

public static class SubscriptionStateExtensions
{
    public static async Task<(ISubscriptionCommandResponse resp, ISubscriptionEvent[] @events)> ProcessCommandAsync(
        this SubscriptionState state, ISubscriptionCommand cmd, IPaymentsService paymentsService)
    {
        switch (cmd)
        {
            case CreateSubscription create:
                return await ProcessCreateSubscription(state, create, paymentsService);
            case CheckSubscriptionStatus check:
                return await ProcessCheckSubscription(state, check, paymentsService);
            case CancelSubscription cancel:
                return await ProcessCancelSubscription(state, cancel, paymentsService);
            case ResumeSubscription resume:
                return await ProcessResumeSubscription(state, resume, paymentsService);
            default:
                throw new ArgumentOutOfRangeException(nameof(cmd));
        }
    }

    private static async Task<(ISubscriptionCommandResponse resp, ISubscriptionEvent[] events)>
        ProcessResumeSubscription(SubscriptionState state, ResumeSubscription resume, IPaymentsService paymentsService)
    {
        if (state.Status is SubscriptionStatus.SuspendedNotPaid or SubscriptionStatus.Cancelled)
        {
            // need to try to run a payment
            var paymentResult = await paymentsService.CreatePayment(state.SubscriptionId, state.ProductId, state.UserId,
                state.UpcomingPaymentAmount);
            if (paymentResult.IsSuccess())
            {
                var subscriptionResumed = new SubscriptionResumed(resume.SubscriptionId);
                return (SubscriptionCommandResponse.Success(state.SubscriptionId),
                    new ISubscriptionEvent[]
                    {
                        subscriptionResumed,
                        new SubscriptionPaymentProcessed(state.SubscriptionId, DateTimeOffset.UtcNow,
                            state.UpcomingPaymentAmount)
                    });
            }
            else
            {
                // need to record the failed payment attempt
                return (SubscriptionCommandResponse.Failure(state.SubscriptionId, paymentResult.Message ?? "Failed to process payment"),
                    new ISubscriptionEvent[]
                    {
                        new SubscriptionPaymentFailed(state.SubscriptionId, DateTimeOffset.UtcNow,
                            state.UpcomingPaymentAmount)
                    });
            }
        }

        return (SubscriptionCommandResponse.Failure(state.SubscriptionId, "Cannot resume subscription."), Array.Empty<ISubscriptionEvent>());
    }

    private static async Task<(ISubscriptionCommandResponse resp, ISubscriptionEvent[] events)>
        ProcessCancelSubscription(SubscriptionState state, CancelSubscription cancel, IPaymentsService paymentsService)
    {
        return state.Status switch
        {
            SubscriptionStatus.Cancelled => (
                SubscriptionCommandResponse.Failure(state.SubscriptionId, "Subscription already cancelled."),
                Array.Empty<ISubscriptionEvent>()),
            SubscriptionStatus.NotStarted => (
                SubscriptionCommandResponse.Failure(state.SubscriptionId, "Subscription does not exist."),
                Array.Empty<ISubscriptionEvent>()),
            SubscriptionStatus.Active => (SubscriptionCommandResponse.Success(state.SubscriptionId),
                new ISubscriptionEvent[] { new SubscriptionCancelled(cancel.SubscriptionId) }),
            _ => ((SubscriptionCommandResponse.Failure(state.SubscriptionId, "Cannot cancel subscription."),
                Array.Empty<ISubscriptionEvent>()))
        };
    }

    private static async Task<(ISubscriptionCommandResponse resp, ISubscriptionEvent[] events)>
        ProcessCheckSubscription(SubscriptionState state, CheckSubscriptionStatus check,
            IPaymentsService paymentsService)
    {
        if (state.Status != SubscriptionStatus.Active && state.Status != SubscriptionStatus.SuspendedNotPaid)
            return (SubscriptionCommandResponse.Failure(state.SubscriptionId, "Subscription is not active."),
                Array.Empty<ISubscriptionEvent>());

        // if we're in an active status AND we're past the next payment date, we need to attempt to process the payment.
        // If that fails, we need to suspend the subscription
        if (state.Status is SubscriptionStatus.Active && state.NextPaymentDate <= DateTimeOffset.UtcNow)
        {
            // run payment
            var paymentResult = await paymentsService.CreatePayment(state.SubscriptionId, state.ProductId, state.UserId,
                state.UpcomingPaymentAmount);
            if (paymentResult.IsSuccess())
            {
                return (SubscriptionCommandResponse.Success(state.SubscriptionId),
                    new ISubscriptionEvent[]
                    {
                        new SubscriptionPaymentProcessed(paymentResult.PaymentId,
                            DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)
                    });
            }
            else
            {
                // need to record the failed payment attempt
                var subscriptionSuspended = new SubscriptionSuspended(state.SubscriptionId);
                return (
                    SubscriptionCommandResponse.NoOp(state.SubscriptionId,
                        paymentResult.Message ?? "Failed to process payment"),
                    new ISubscriptionEvent[]
                    {
                        subscriptionSuspended,
                        new SubscriptionPaymentFailed(state.SubscriptionId, DateTimeOffset.UtcNow,
                            state.UpcomingPaymentAmount)
                    });
            }
        }

        // if we're in a suspended status AND we're past the next payment date, we need to attempt to process the payment.
        // If that fails, we need to suspend the subscription
        if (state.Status is SubscriptionStatus.SuspendedNotPaid && state.NextPaymentDate <= DateTimeOffset.UtcNow)
        {
            // run payment
            var paymentResult = await paymentsService.CreatePayment(state.SubscriptionId, state.ProductId, state.UserId,
                state.UpcomingPaymentAmount);
            if (paymentResult.IsSuccess())
            {
                var subscriptionResumed = new SubscriptionResumed(state.SubscriptionId);
                return (SubscriptionCommandResponse.Success(state.SubscriptionId),
                    new ISubscriptionEvent[]
                    {
                        subscriptionResumed,
                        new SubscriptionPaymentProcessed(state.SubscriptionId,
                            DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)
                    });
            }
            else
            {
                // need to record the failed payment attempt
                return (
                    SubscriptionCommandResponse.NoOp(state.SubscriptionId,
                        paymentResult.Message ?? "Failed to process payment"),
                    new ISubscriptionEvent[]
                    {
                        new SubscriptionPaymentFailed(state.SubscriptionId, DateTimeOffset.UtcNow,
                            state.UpcomingPaymentAmount)
                    });
            }
        }
        
        return (SubscriptionCommandResponse.Failure(state.SubscriptionId, "Subscription is not active."),
            Array.Empty<ISubscriptionEvent>());
    }

    private static async Task<(ISubscriptionCommandResponse resp, ISubscriptionEvent[] events)>
        ProcessCreateSubscription(SubscriptionState state, SubscriptionCommands.CreateSubscription create,
            IPaymentsService paymentsService)
    {
        // if we're here, we already know that the subscription doesn't exist
        var subscription = new SubscriptionCreated(create.SubscriptionId, create.ProductId, create.UserId,
            create.Interval, create.PaymentAmount);

        // need to process first payment
        var paymentResult = await paymentsService.CreatePayment(state.SubscriptionId, state.ProductId, state.UserId,
            state.UpcomingPaymentAmount);
        if (paymentResult.IsSuccess())
        {
            return new(SubscriptionCommandResponse.Success(state.SubscriptionId),
                new ISubscriptionEvent[]
                {
                    subscription,
                    new SubscriptionPaymentProcessed(paymentResult.PaymentId, DateTimeOffset.UtcNow,
                        state.UpcomingPaymentAmount)
                });
        }
        else
        {
            // need to record the failed payment attempt
            return new(
                SubscriptionCommandResponse.Failure(state.SubscriptionId,
                    paymentResult.Message ?? "Failed to process payment"),
                new ISubscriptionEvent[]
                {
                    subscription,
                    new SubscriptionPaymentFailed(state.SubscriptionId, DateTimeOffset.UtcNow,
                        state.UpcomingPaymentAmount)
                });
        }
    }
    
    public static SubscriptionState Apply(this SubscriptionState state, ISubscriptionEvent @event)
    {
        return @event switch
        {
            SubscriptionCreated created => state with
            {
                ProductId = created.ProductId,
                UserId = created.UserId,
                Status = SubscriptionStatus.Active,
                UpcomingPaymentAmount = created.PaymentAmount,
                NextPaymentDate = DateTimeOffset.UtcNow, // payment is owed upfront
                Interval = created.Interval
            },
            SubscriptionPaymentProcessed processed => state with
            {
                NextPaymentDate = state.Interval == SubscriptionInterval.Yearly ? processed.PaymentDate.AddYears(1) : processed.PaymentDate.AddMonths(1),
                Status = SubscriptionStatus.Active
            },
            SubscriptionPaymentFailed failed => state with
            {
                Status = SubscriptionStatus.SuspendedNotPaid
            },
            SubscriptionCancelled cancelled => state with
            {
                Status = SubscriptionStatus.Cancelled
            },
            SubscriptionResumed resumed => state with
            {
                Status = SubscriptionStatus.Active
            },
            SubscriptionSuspended suspended => state with
            {
                Status = SubscriptionStatus.SuspendedNotPaid
            },
            _ => state
        };
    }
}