using Akka.Pattern.Common;
using Akka.Pattern.Domains.Payments;

namespace Akka.Pattern.Tests.Utilities;

public sealed class FakePaymentsService : IPaymentsService
{
    /// <summary>
    /// Determines if payment operations are going to succeed or fail
    /// </summary>
    public bool ShouldPass { get; set; } = true;
    
    public Task<PaymentsResponse> CreatePayment(SubscriptionId subscriptionId, ProductId productId, UserId userId, decimal amountToCharge)
    {
        var paymentId = Guid.NewGuid().ToString();
        var result = ShouldPass ? CommandResult.Success : CommandResult.Failure;
        var payment = new PaymentsResponse(paymentId, amountToCharge, result);
        
        return Task.FromResult(payment);
    }
}