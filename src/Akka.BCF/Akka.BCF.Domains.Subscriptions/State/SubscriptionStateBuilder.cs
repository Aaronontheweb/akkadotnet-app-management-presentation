using Akka.BCF.Abstractions.States;
using Akka.BCF.Domains.Subscriptions.Messages;

namespace Akka.BCF.Domains.Subscriptions.State;

public class SubscriptionStateBuilder : IStateBuilder<string, SubscriptionState, ISubscriptionEvent>
{
    public IStateBuilder<string, SubscriptionState, ISubscriptionEvent> Apply(SubscriptionState state, ISubscriptionEvent @event)
    {
        switch (@event)
        {
            case SubscriptionCreated created:
            {
                state.UserId = created.UserId;
                state.ProductId = created.ProductId;
                state.Status = SubscriptionStatus.Active;
                state.Interval = created.Interval;
                state.UpcomingPaymentAmount = created.PaymentAmount;
                state.NextPaymentDate = DateTimeOffset.UtcNow; // payment is owed upfront
                break;
            }
            case SubscriptionCancelled cancelled:
            {
                state.Status = SubscriptionStatus.Cancelled;
                break;
            }
            case SubscriptionResumed resumed:
            {
                state.Status = SubscriptionStatus.Active;
                break;
            }
            case SubscriptionPaymentProcessed processed:
            {
                state.NextPaymentDate = state.Interval == SubscriptionInterval.Yearly ? processed.PaymentDate.AddYears(1) : processed.PaymentDate.AddMonths(1);
                break;
            }
            case SubscriptionPaymentFailed failed:
            {
                state.Status = SubscriptionStatus.SuspendedNotPaid;
                break;
            }
        }

        return this;
    }
}