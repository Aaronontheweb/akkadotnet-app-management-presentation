using Akka.BCF.Abstractions.States;
using Akka.BCF.Domains.Payments;
using Akka.BCF.Domains.Subscriptions.Messages;
using Akka.BCF.Domains.Subscriptions.Messages.Commands;
using Microsoft.Extensions.Logging;

namespace Akka.BCF.Domains.Subscriptions.State;

public sealed class
    SubscriptionProcessor : StateProcessorBase<string, SubscriptionState, ISubscriptionCommand, ISubscriptionEvent>
{
    private readonly IPaymentsService _paymentsService;
    
    public SubscriptionProcessor(IStateValidator<string, SubscriptionState, ISubscriptionCommand> validator,
        IPaymentsService paymentsService,
        ILogger<StateProcessorBase<string, SubscriptionState, ISubscriptionCommand, ISubscriptionEvent>> logger) : base(
        validator, logger)
    {
        _paymentsService = paymentsService;
    }

    protected override async ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)> ProcessInternalAsync(
        SubscriptionState state, ISubscriptionCommand command)
    {
        return command switch
        {
            CreateSubscription create => await ProcessCreateSubscription(state, create),
            CancelSubscription cancel => await ProcessCancelSubscription(state, cancel),
            ResumeSubscription resume => await ProcessResumeSubscription(state, resume),
            CheckSubscriptionStatus suspend => await CheckSubscription(state, suspend),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }

    private async ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)> CheckSubscription(SubscriptionState state, CheckSubscriptionStatus suspend)
    {
        if(state.Status != SubscriptionStatus.Active && state.Status != SubscriptionStatus.SuspendedNotPaid)
            return (false, Array.Empty<ISubscriptionEvent>(), "Subscription is not active.");
        
        // if we're in an active status AND we're past the next payment date, we need to attempt to process the payment.
        // If that fails, we need to suspend the subscription
        if (state.Status is SubscriptionStatus.Active && state.NextPaymentDate <= DateTimeOffset.UtcNow)
        {
            // run payment
            var paymentResult = await _paymentsService.CreatePayment(state.EntityId, state.ProductId, state.UserId, state.UpcomingPaymentAmount);
            if (paymentResult.Success)
            {
                return (true, new ISubscriptionEvent[] { new SubscriptionPaymentProcessed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, string.Empty);
            }
            else
            {
                // need to record the failed payment attempt
                var subscriptionSuspended = new SubscriptionSuspended(suspend.EntityId);
                return (true, new ISubscriptionEvent[] { subscriptionSuspended, new SubscriptionPaymentFailed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, paymentResult.Message ?? "Failed to process payment");
            }
        }
        
        // if we're in a suspended status AND we're past the next payment date, we need to attempt to process the payment.
        // If that fails, we need to suspend the subscription
        if (state.Status is SubscriptionStatus.SuspendedNotPaid && state.NextPaymentDate <= DateTimeOffset.UtcNow)
        {
            // run payment
            var paymentResult = await _paymentsService.CreatePayment(state.EntityId, state.ProductId, state.UserId, state.UpcomingPaymentAmount);
            if (paymentResult.Success)
            {
                var subscriptionResumed = new SubscriptionResumed(suspend.EntityId);
                return (true, new ISubscriptionEvent[] {subscriptionResumed, new SubscriptionPaymentProcessed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, string.Empty);
            }
            else
            {
                // need to record the failed payment attempt
                return (true, new ISubscriptionEvent[] { new SubscriptionPaymentFailed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, paymentResult.Message ?? "Failed to process payment");
            }
        }
        
        return (false, Array.Empty<ISubscriptionEvent>(), "Subscription is not active.");
    }

    private async ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)> ProcessResumeSubscription(SubscriptionState state, ResumeSubscription resume)
    {
        if (state.Status is SubscriptionStatus.SuspendedNotPaid or SubscriptionStatus.Cancelled)
        {
            // need to try to run a payment
            var paymentResult = await _paymentsService.CreatePayment(state.EntityId, state.ProductId, state.UserId, state.UpcomingPaymentAmount);
            if (paymentResult.Success)
            {
                var subscriptionResumed = new SubscriptionResumed(resume.EntityId);
                return (true, new ISubscriptionEvent[] {subscriptionResumed, new SubscriptionPaymentProcessed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, string.Empty);
            }
            else
            {
                // need to record the failed payment attempt
                return (true, new ISubscriptionEvent[] { new SubscriptionPaymentFailed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, paymentResult.Message ?? "Failed to process payment");
            }
        }

        return (false, Array.Empty<ISubscriptionEvent>(), "Cannot resume subscription.");
    }

    private ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)> ProcessCancelSubscription(SubscriptionState state, CancelSubscription cancel)
    {
        return state.Status switch
        {
            SubscriptionStatus.Cancelled => new ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)>((false, Array.Empty<ISubscriptionEvent>(), "Subscription already cancelled.")),
            SubscriptionStatus.NotStarted => new ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)>((false, Array.Empty<ISubscriptionEvent>(), "Subscription does not exist.")),
            SubscriptionStatus.Active => new ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)>((true, new ISubscriptionEvent[] {new SubscriptionCancelled(cancel.EntityId)}, string.Empty)),
            _ => new ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)>((false, Array.Empty<ISubscriptionEvent>(), "Cannot cancel subscription."))
        };
    }

    private async ValueTask<(bool canProcess, ISubscriptionEvent[] events, string message)> ProcessCreateSubscription(SubscriptionState state, CreateSubscription create)
    {
        // if we're here, we already know that the subscription doesn't exist
        var subscription = new SubscriptionCreated(create.EntityId, create.ProductId, create.UserId, create.Interval, create.PaymentAmount);
        
        // need to process first payment
        var paymentResult = await _paymentsService.CreatePayment(state.EntityId, state.ProductId, state.UserId, state.UpcomingPaymentAmount);
        if (paymentResult.Success)
        {
            return new (true, new ISubscriptionEvent[] {subscription, new SubscriptionPaymentProcessed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, string.Empty);
        }
        else
        {
            // need to record the failed payment attempt
            return new (true, new ISubscriptionEvent[] {subscription, new SubscriptionPaymentFailed(paymentResult.EntityId, DateTimeOffset.UtcNow, state.UpcomingPaymentAmount)}, paymentResult.Message ?? "Failed to process payment");
        }
    }
}