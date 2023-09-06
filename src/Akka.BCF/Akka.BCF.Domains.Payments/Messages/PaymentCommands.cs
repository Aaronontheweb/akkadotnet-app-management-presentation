using Akka.Actor;
using Akka.BCF.Abstractions.Messages.Commands;

namespace Akka.BCF.Domains.Payments.Messages;

public interface IPaymentCommand : IDomainCommand<string>
{
}

public sealed record CreatePayment(string PaymentId, decimal AmountToCharge, IActorRef? ReplyTo) : IPaymentCommand
{
    public string EntityId => PaymentId;
}