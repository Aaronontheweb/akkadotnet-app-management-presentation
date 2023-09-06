using Akka.Actor;
using Akka.BCF.Abstractions.Messages.Commands;
using Akka.BCF.Domains.Payments.Messages;

namespace Akka.BCF.Domains.Payments;

public interface IPaymentsService
{
    Task<ICommandResponse<string>> CreatePayment(string subscriptionId, string productId, string userId, decimal amountToCharge);
}


public sealed class DefaultPaymentsService : IPaymentsService
{
    public Task<ICommandResponse<string>> CreatePayment(string subscriptionId, string productId, string userId, decimal amountToCharge)
    {
        var paymentId = Guid.NewGuid().ToString();
        var payment = new CreatePayment(paymentId, amountToCharge, null);
        
        // create a random change of payments succeeding or failing
        var random = new Random();
        var paymentSucceeded = random.Next(0, 2) == 1;
        if (paymentSucceeded)
        {
            return Task.FromResult<ICommandResponse<string>>(new CommandSucceeded<string>(payment));
        }
        else
        {
            return Task.FromResult<ICommandResponse<string>>(new CommandFailed<string>(payment, "Payment failed."));
        }
    }
}

