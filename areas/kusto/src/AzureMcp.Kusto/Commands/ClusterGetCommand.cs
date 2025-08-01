// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.Core.Services.Telemetry;
using AzureMcp.Kusto.Commands;
using AzureMcp.Kusto.Options;
using AzureMcp.Kusto.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Kusto.Commands;

public sealed class ClusterGetCommand(ILogger<ClusterGetCommand> logger) : BaseClusterCommand<ClusterGetOptions>
{
    private const string CommandTitle = "Get Kusto Cluster Details";
    private readonly ILogger<ClusterGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Get details for a specific Kusto cluster. Requires `subscription` and `cluster-name`.
        The response includes the `clusterUri` property for use in subsequent commands.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

            var kusto = context.GetService<IKustoService>();
            var cluster = await kusto.GetCluster(
                options.Subscription!,
                options.ClusterName!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = cluster is null ?
            null : ResponseResult.Create(new ClusterGetCommandResult(cluster), KustoJsonContext.Default.ClusterGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting Kusto cluster details. Cluster: {Cluster}.", options.ClusterName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ClusterGetCommandResult(KustoClusterResourceProxy Cluster);
}
