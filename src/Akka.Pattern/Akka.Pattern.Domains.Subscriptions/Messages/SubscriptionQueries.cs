using Akka.Pattern.Common;
using Akka.Pattern.Domains.Subscriptions.Actors;

namespace Akka.Pattern.Domains.Subscriptions.Messages;

public interface ISubscriptionQuery : IWithSubscriptionId, IDomainQuery
{
}

public interface ISubscriptionQueryResponse : IWithSubscriptionId, IDomainQueryResponse
{
    
}

public static class SubscriptionQueries
{
    public sealed record GetSubscriptionState(SubscriptionId SubscriptionId) : ISubscriptionQuery;

    public sealed record GetSubscriptionStateResponse(SubscriptionId SubscriptionId, SubscriptionState? State,
        QueryResponseCode ResponseCode, string? Message = null) : ISubscriptionQueryResponse;
}