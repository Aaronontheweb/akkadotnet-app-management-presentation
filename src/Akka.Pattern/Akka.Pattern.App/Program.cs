using Akka.Actor;
using Akka.Hosting;
using Akka.Cluster.Hosting;
using Akka.Pattern.Common;
using Akka.Pattern.Domains.Payments.Config;
using Akka.Pattern.Domains.Subscriptions;
using Akka.Pattern.Domains.Subscriptions.Actors;
using Akka.Pattern.Domains.Subscriptions.Config;
using Akka.Pattern.Domains.Subscriptions.Messages;
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
using LogLevel = Akka.Event.LogLevel;

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
    services.AddOptions<RemoteOptions>()
        .Bind(context.Configuration.GetSection(nameof(RemoteOptions)));
    services.AddOptions<ClusterOptions>()
        .Bind(context.Configuration.GetSection(nameof(ClusterOptions)));

    services.ConfigurePaymentsServices();

    services.AddAkka("SubscriptionsService", (builder, sp) =>
    {
        var remoteOptions = sp.GetRequiredService<IOptionsSnapshot<RemoteOptions>>();
        var clusterOptions = sp.GetRequiredService<IOptionsSnapshot<ClusterOptions>>();

        builder
            .ConfigureLoggers(setup =>
            {
                setup.LogLevel = LogLevel.InfoLevel;
                setup.ClearLoggers();

                // Example: Add the ILoggerFactory logger
                setup.AddLoggerFactory();
            })
            .WithRemoting(remoteOptions.Value)
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
                    var command = new SubscriptionCommands.CreateSubscription(new SubscriptionId($"subscription-{i}"),
                        new ProductId("test"), new UserId($"test-{i}"), SubscriptionInterval.Monthly, 100.0m);
                    subscriptionRegion.Tell(command);
                }
            });
    });
});

var host = hostBuilder.Build();

await host.RunAsync();