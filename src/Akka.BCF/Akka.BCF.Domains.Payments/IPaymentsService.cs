using Akka.BCF.Abstractions.Messages.Commands;

namespace Akka.BCF.Domains.Payments;

public interface IPaymentsService
{
    Task<ICommandResponse<string>> CreatePayment(string subscriptionId, string productId, string userId, decimal amountToCharge);
}

