using System.Collections.Immutable;
using Akka.BCF.Abstractions.States;
using Akka.BCF.Domains.Subscriptions.Messages.Commands;

namespace Akka.BCF.Domains.Subscriptions.State;

public sealed class SubscriptionValidator : IStateValidator<string, SubscriptionState, ISubscriptionCommand>
{
    public async ValueTask<ValidationResults> ValidateAsync(SubscriptionState state, ISubscriptionCommand command)
    {
        switch (command)
        {
            case CreateSubscription when state.Status == SubscriptionStatus.NotStarted:
                return new ValidationResults(true, ImmutableList<string>.Empty);
            case CreateSubscription create:
                return new ValidationResults(false, ImmutableList<string>.Empty.Add("Subscription already exists."));
            case CancelSubscription when state.Status == SubscriptionStatus.Cancelled:
                return new ValidationResults(true, ImmutableList<string>.Empty.Add("Subscription already cancelled."));
            case CancelSubscription when state.Status == SubscriptionStatus.NotStarted:
                return new ValidationResults(false, ImmutableList<string>.Empty.Add("Subscription does not exist."));
            /* Imagine the rest of the test cases here */
            default:
                return new ValidationResults(true, ImmutableList<string>.Empty.Add("Need to add more test cases."));
        }
    }
}