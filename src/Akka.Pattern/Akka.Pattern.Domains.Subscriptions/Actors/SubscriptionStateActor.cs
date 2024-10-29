using Akka.Actor;
using Akka.Event;
using Akka.Pattern.Common;
using Akka.Pattern.Domains.Payments;
using Akka.Pattern.Domains.Subscriptions.Messages;
using Akka.Persistence;
using static Akka.Pattern.Domains.Subscriptions.Messages.SubscriptionQueries;
using static Akka.Pattern.Domains.Subscriptions.Messages.SubscriptionCommands;
using static Akka.Pattern.Domains.Subscriptions.Messages.SubscriptionEvents;

namespace Akka.Pattern.Domains.Subscriptions.Actors;

public sealed class SubscriptionStateActor : ReceivePersistentActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IPaymentsService _paymentsService;
    
    public SubscriptionState State { get; set; }
    
    public override string PersistenceId { get; }

    public SubscriptionStateActor(string subscriptionId, IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
        PersistenceId = $"subscription-{subscriptionId}";
        State = new SubscriptionState(subscriptionId);
        
        Recovers();
        Commands();
        Queries();
    }

    private void Recovers()
    {
        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is SubscriptionState state)
            {
                State = state;
            }
        });

        Recover<ISubscriptionEvent>(e =>
        {
            State = State.Apply(e);
        });
    }

    private void Commands()
    {
        CommandAsync<ISubscriptionCommand>(async cmd =>
        {
            _log.Debug("Processing command {0} in State {1}", cmd, State);
            var (resp, events) = await State
                .ProcessCommandAsync(cmd, _paymentsService);

            if (events.Length == 0)
            {
                _log.Debug("No events produced in response to command {0} for State {1}", cmd, State);
                Sender.Tell(resp);
                return;
            }

            var replied = false;
            PersistAll(events, e =>
            {
                _log.Info("Persisted event {0} for command {1} in State {2}", e, cmd, State);
                State = State.Apply(e);
                if (!replied)
                {
                    Sender.Tell(resp);
                    replied = true;
                }
                
                if (LastSequenceNr % MessagesPerSnapshot == 0)
                    SaveSnapshot(State);
            });
        });
    }

    private const int MessagesPerSnapshot = 30;

    private void Queries()
    {
        Command<GetSubscriptionState>(get =>
        {
            // this subscription hasn't been created yet, so no state to return
            if (State.Status == SubscriptionStatus.NotStarted)
            {
                Sender.Tell(new GetSubscriptionStateResponse(get.SubscriptionId, null, QueryResponseCode.NotFound));
                return;
            }
            
            Sender.Tell(new GetSubscriptionStateResponse(get.SubscriptionId, State, QueryResponseCode.Found));
        });
    }
}