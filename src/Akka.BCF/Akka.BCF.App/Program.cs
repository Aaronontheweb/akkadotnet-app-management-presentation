using Akka.BCF.Abstractions.Actors;
using Akka.Hosting;
using Akka.BCF.Domains.Payments.Config;
using Akka.BCF.Domains.Subscriptions.Config;
using Akka.Cluster.Hosting;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureAppConfiguration((context, builder) =>
{
    builder.AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{environment}.json", optional: true)
        .AddEnvironmentVariables();
});

hostBuilder.ConfigureServices((context, services) =>
{
    services.AddSingleton<IValidateOptions<ActorConfig>>(new ValidateActorConfig());
    services.AddOptions<RemoteOptions>()
        .Bind(context.Configuration.GetSection(nameof(RemoteOptions)));
    services.AddOptions<ClusterOptions>()
        .Bind(context.Configuration.GetSection(nameof(ClusterOptions)));
    services.AddOptions<ActorConfig>()
        .Bind(context.Configuration.GetSection(nameof(ActorConfig)))
        .ValidateOnStart();
    
    services.ConfigurePaymentsServices();
    services.ConfigureSubscriptionsServices();

    services.AddAkka("SubscriptionsService", (builder, sp) =>
    {
        var remoteOptions = sp.GetRequiredService<IOptionsSnapshot<RemoteOptions>>();
        var clusterOptions = sp.GetRequiredService<IOptionsSnapshot<ClusterOptions>>();
        
        builder.WithRemoting(remoteOptions.Value)
            .WithClustering(clusterOptions.Value)
            .AddSubscriptionsEntities(clusterOptions.Value.Roles!.First());
    });
});

var host = hostBuilder.Build();

await host.RunAsync();