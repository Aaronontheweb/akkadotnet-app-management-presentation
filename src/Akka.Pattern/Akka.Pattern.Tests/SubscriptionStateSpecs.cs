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
    public static readonly SubscriptionId TestSubscriptionId = new("test-subscription");
    public static readonly ProductId TestProductId1 = new("test-product");
    public static readonly ProductId TestProductId2 = new("test-product-2");
    public static readonly UserId TestUserId = new("test-user");
    public static readonly SubscriptionState InitialState = new(TestSubscriptionId);
    
    public class WhenInitializingSubscriptionState 
    {
        public readonly FakePaymentsService PaymentsService = new() { ShouldPass = true };
        
        [Fact]
        public void StatusShouldBeNotStarted()
        {
            InitialState.Status.Should().Be(SubscriptionStatus.NotStarted);
        }
        
        [Fact]
        public async Task ShouldProcessCreationEventForSameSubscriptionId()
        {
            var cmd = new SubscriptionCommands.CreateSubscription(TestSubscriptionId, TestProductId1, TestUserId, 
                SubscriptionInterval.Monthly, 100.0m);

            var (resp, events) = await InitialState.ProcessCommandAsync(cmd, PaymentsService);
            
            resp.Result.Should().Be(CommandResult.Success);
            events.Should().HaveCount(2); // subscription created + payment processed
            events[0].Should().BeOfType<SubscriptionEvents.SubscriptionCreated>();
            
            var created = (SubscriptionEvents.SubscriptionCreated) events[0];
            created.SubscriptionId.Should().Be(TestSubscriptionId);
            created.ProductId.Should().Be(TestProductId1);
            created.UserId.Should().Be(TestUserId);
            
            events[1].Should().BeOfType<SubscriptionEvents.SubscriptionPaymentProcessed>();
            var payment = (SubscriptionEvents.SubscriptionPaymentProcessed) events[1];
            payment.SubscriptionId.Should().Be(TestSubscriptionId);
            payment.PaymentAmount.Should().Be(cmd.PaymentAmount);
        }
    }
}