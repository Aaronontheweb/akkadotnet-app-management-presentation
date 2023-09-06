using Akka.BCF.Abstractions.Actors;
using Akka.Hosting;
using Akka.BCF.Domains.Payments.Config;
using Akka.BCF.Domains.Subscriptions.Actors;
using Akka.BCF.Domains.Subscriptions.Config;
using Akka.BCF.Domains.Subscriptions.Messages.Commands;
using Akka.BCF.Domains.Subscriptions.State;
using Akka.Cluster.Hosting;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Cluster.Sharding;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureLogging(logBuilder =>
{
    logBuilder.ClearProviders();
    logBuilder.AddConsole();
});

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
            .AddSubscriptionsEntities(clusterOptions.Value.Roles!.First())
            .AddPetabridgeCmd(cmd =>
            {
                cmd.RegisterCommandPalette(ClusterShardingCommands.Instance);
                cmd.RegisterCommandPalette(new RemoteCommands());
                cmd.RegisterCommandPalette(ClusterCommands.Instance);
            })
            .AddStartup(async (system, registry) =>
            {
                var subscriptionRegion = await registry.GetAsync<SubscriptionStateActor>();
                
                // create 10 "CreateSubscription" commands
                for (var i = 0; i < 10; i++)
                {
                    var command = new CreateSubscription($"subscription-{i}", "test", $"test-{i}", SubscriptionInterval.Monthly, 100.0m);
                    subscriptionRegion.Tell(command);
                }
            });
    });
});

var host = hostBuilder.Build();

await host.RunAsync();