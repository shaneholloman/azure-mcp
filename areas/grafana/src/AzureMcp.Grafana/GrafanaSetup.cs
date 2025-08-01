// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using AzureMcp.Grafana.Commands.Workspace;
using AzureMcp.Grafana.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Grafana;

public class GrafanaSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGrafanaService, GrafanaService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var grafana = new CommandGroup("grafana", "Grafana workspace operations - Commands for managing and accessing Azure Managed Grafana resources and monitoring dashboards. Includes operations for listing Grafana workspaces and managing data visualization and monitoring capabilities.");
        rootGroup.AddSubGroup(grafana);

        grafana.AddCommand("list", new WorkspaceListCommand(loggerFactory.CreateLogger<WorkspaceListCommand>()));
    }
}
