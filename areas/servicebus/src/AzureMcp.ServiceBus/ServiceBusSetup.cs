// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using AzureMcp.ServiceBus.Commands.Queue;
using AzureMcp.ServiceBus.Commands.Topic;
using AzureMcp.ServiceBus.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.ServiceBus;

public class ServiceBusSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceBusService, ServiceBusService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var serviceBus = new CommandGroup("servicebus", "Service Bus operations - Commands for managing Azure Service Bus resources including queues, topics, and subscriptions. Includes operations for managing message queues, topic subscriptions, and retrieving details about Service Bus entities.");
        rootGroup.AddSubGroup(serviceBus);

        var queue = new CommandGroup("queue", "Queue operations - Commands for using Azure Service Bus queues.");
        // queue.AddCommand("peek", new QueuePeekCommand());
        queue.AddCommand("details", new QueueDetailsCommand(
            loggerFactory.CreateLogger<QueueDetailsCommand>()));

        var topic = new CommandGroup("topic", "Topic operations - Commands for using Azure Service Bus topics and subscriptions.");
        topic.AddCommand("details", new TopicDetailsCommand(
            loggerFactory.CreateLogger<TopicDetailsCommand>()));

        var subscription = new CommandGroup("subscription", "Subscription operations - Commands for using subscriptions within a Service Bus topic.");
        // subscription.AddCommand("peek", new SubscriptionPeekCommand());
        subscription.AddCommand("details", new SubscriptionDetailsCommand(
            loggerFactory.CreateLogger<SubscriptionDetailsCommand>()));

        serviceBus.AddSubGroup(queue);
        serviceBus.AddSubGroup(topic);

        topic.AddSubGroup(subscription);
    }
}
