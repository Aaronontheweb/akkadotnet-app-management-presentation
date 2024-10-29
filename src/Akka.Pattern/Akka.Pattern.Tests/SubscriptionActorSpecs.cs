using Akka.Actor;
using Akka.Configuration;
using Akka.Pattern.Common;
using Akka.Pattern.Domains.Subscriptions;
using Akka.Pattern.Domains.Subscriptions.Actors;
using Akka.Pattern.Domains.Subscriptions.Messages;
using Akka.Pattern.Tests.Utilities;
using Akka.TestKit;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere

namespace Akka.Pattern.Tests;

public class SubscriptionActorSpecs : TestKit.Xunit2.TestKit
{
    // create HOCON config that sets the journal and the snapshot store to use in-memory
    public static readonly Config InMemoryPersistence = """"
                                                        # in-mem journal is the default but we're going to be explicit since this is a sample
                                                        akka.persistence.journal.plugin = akka.persistence.journal.inmem
                                                        akka.persistence.journal.inmem {
                                                            class = "Akka.Persistence.Journal.MemoryJournal, Akka.Persistence"
                                                        }

                                                        akka.persistence.snapshot-store.plugin = akka.persistence.snapshot-store.inmem
                                                        akka.persistence.snapshot-store.inmem {
                                                            class = "Akka.Persistence.Snapshot.MemorySnapshotStore, Akka.Persistence"
                                                        }";
                                                        """";

    public SubscriptionActorSpecs(ITestOutputHelper outputHelper) : base(InMemoryPersistence, output: outputHelper)
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
    public async Task InitialQueryStateShouldBeNotFound()
    {
        // arrange
        TestActorRef<SubscriptionStateActor> subscriptionActor =
            ActorOfAsTestActorRef<SubscriptionStateActor>(SubscriptionStateActorProps);

        // pre-conditions
        var initialState = subscriptionActor.UnderlyingActor.State;
        initialState.Status.Should().Be(SubscriptionStatus.NotStarted);

        // act
        var stateResponse = await subscriptionActor.Ask<SubscriptionQueries.GetSubscriptionStateResponse>(
            new SubscriptionQueries.GetSubscriptionState(TestSubscriptionId), RemainingOrDefault);
        
        // assert
        
        /* an important but subtle detail - actors that have been instantiated but not
         actually populated should always respond "not found" initially */
        stateResponse.ResponseCode.Should().Be(QueryResponseCode.NotFound);
    }
}