using Akka.Pattern.Common;

namespace Akka.Pattern.Domains.Payments;

public sealed record PaymentsResponse
    (string PaymentId, decimal AmountCharged, CommandResult Result, string? Message = null) : ICommandResponse;
public interface IPaymentsService
{
    Task<PaymentsResponse> CreatePayment(SubscriptionId subscriptionId, ProductId productId, UserId userId, decimal amountToCharge);
}


public sealed class DefaultPaymentsService : IPaymentsService
{
    public Task<PaymentsResponse> CreatePayment(SubscriptionId subscriptionId, ProductId productId, UserId userId, decimal amountToCharge)
    {
        var paymentId = Guid.NewGuid().ToString();
        
        // create a random change of payments succeeding or failing
        var random = new Random();
        var paymentSucceeded = random.Next(0, 2) == 1;
        var payment = new PaymentsResponse(paymentId, amountToCharge, CommandResult.Success);
        if (paymentSucceeded)
        {
            return Task.FromResult(payment);
        }
        else
        {
            return Task.FromResult(payment with { Result = CommandResult.Failure, Message = "Payment failed" });
        }
    }
}

