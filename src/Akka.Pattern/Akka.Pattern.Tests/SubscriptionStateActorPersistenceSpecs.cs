using Akka.Actor;
using Akka.Pattern.Common;
using Akka.Pattern.Domains.Subscriptions;
using Akka.Pattern.Domains.Subscriptions.Actors;
using Akka.Pattern.Domains.Subscriptions.Messages;
using Akka.Pattern.Tests.Utilities;
using Akka.Persistence;
using Akka.Persistence.TestKit;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Pattern.Tests;

public class SubscriptionStateActorPersistenceSpecs : PersistenceTestKit
{
    public SubscriptionStateActorPersistenceSpecs(ITestOutputHelper output) : base(output:output)
    {
    }

    public static readonly SubscriptionId TestSubscriptionId = new("test-subscription");
    public static readonly ProductId TestProductId1 = new("test-product");
    public static readonly ProductId TestProductId2 = new("test-product-2");
    public static readonly UserId TestUserId = new("test-user");

    private readonly FakePaymentsService _fakePaymentsService = new() { ShouldPass = true };

    private Props SubscriptionStateActorProps =>
        Props.Create(() => new SubscriptionStateActor(TestSubscriptionId, _fakePaymentsService));

    [Fact]
    public async Task ShouldPersistSubscriptionState()
    {
        // arrange
        var actor = ActorOf(SubscriptionStateActorProps);
        
        // query the actor to get the current state
        actor.Tell(new SubscriptionQueries.GetSubscriptionState(TestSubscriptionId));
        var resp1 = await ExpectMsgAsync<SubscriptionQueries.GetSubscriptionStateResponse>();
        resp1.ResponseCode.Should().Be(QueryResponseCode.NotFound);
        
        // act - try to get the actor to process a command
        var cmd = new SubscriptionCommands.CreateSubscription(TestSubscriptionId, TestProductId1, TestUserId, 
            SubscriptionInterval.Monthly, 100.0m);

        await WithJournalWrite(behavior => behavior.Fail(), async () =>
        {
            actor.Tell(cmd);
            await WatchAsync(actor);
            
            // actor should be killed as a result of write failure
            await ExpectTerminatedAsync(actor);
        });

    }

}