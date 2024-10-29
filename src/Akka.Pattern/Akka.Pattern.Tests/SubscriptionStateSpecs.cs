using Akka.Pattern.Common;
using Akka.Pattern.Domains.Subscriptions;
using Akka.Pattern.Domains.Subscriptions.Actors;
using Akka.Pattern.Domains.Subscriptions.Messages;
using Akka.Pattern.Tests.Utilities;
using FluentAssertions;
using Xunit;

namespace Akka.Pattern.Tests;

public class SubscriptionStateSpecs
{
    public readonly FakePaymentsService PaymentsService = new() { ShouldPass = true };
    
    public static readonly SubscriptionId TestSubscriptionId = new SubscriptionId("test-subscription");
    public static readonly SubscriptionState InitialState = new SubscriptionState(TestSubscriptionId);
    
    public class WhenInitializingSubscriptionState 
    {
        [Fact]
        public void StatusShouldBeNotStarted()
        {
            InitialState.Status.Should().Be(SubscriptionStatus.NotStarted);
        }
        
        [Fact]
        public void ShouldProcessCreationEventForSameSubscriptionId()
        {
            // var cmd = SubscriptionCommands.CreateSubscription(TestSubscriptionId);
            // state.Status.Should().Be(SubscriptionStatus.Created);
        }
    }
}