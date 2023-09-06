using Microsoft.Extensions.DependencyInjection;

namespace Akka.Pattern.Domains.Payments.Config;

public static class PaymentsDomainConfig
{
    public static IServiceCollection ConfigurePaymentsServices(this IServiceCollection services)
    {
        return services.AddTransient<IPaymentsService, DefaultPaymentsService>();
    }
}