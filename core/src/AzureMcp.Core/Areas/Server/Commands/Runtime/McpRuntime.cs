// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using AzureMcp.Core.Areas.Server.Commands.ToolLoading;
using AzureMcp.Core.Areas.Server.Options;
using AzureMcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using static AzureMcp.Core.Services.Telemetry.TelemetryConstants;

namespace AzureMcp.Core.Areas.Server.Commands.Runtime;

/// <summary>
/// Implementation of the MCP runtime that delegates tool discovery and invocation to a tool loader.
/// Provides logging and configuration support for the MCP server.
/// </summary>
public sealed class McpRuntime : IMcpRuntime
{
    private readonly IToolLoader _toolLoader;
    private readonly IOptions<ServiceStartOptions> _options;
    private readonly ILogger<McpRuntime> _logger;

    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the McpRuntime class.
    /// </summary>
    /// <param name="toolLoader">The tool loader responsible for discovering and loading tools.</param>
    /// <param name="options">Configuration options for the MCP server.</param>
    /// <param name="logger">Logger for runtime operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required dependencies are null.</exception>
    public McpRuntime(
        IToolLoader toolLoader,
        IOptions<ServiceStartOptions> options,
        ITelemetryService telemetry,
        ILogger<McpRuntime> logger)
    {
        _toolLoader = toolLoader ?? throw new ArgumentNullException(nameof(toolLoader));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("McpRuntime initialized with tool loader of type {ToolLoaderType}.", _toolLoader.GetType().Name);
        _logger.LogInformation("ReadOnly mode is set to {ReadOnly}.", _options.Value.ReadOnly ?? false);
        _logger.LogInformation("Namespace is set to {Namespace}.", string.Join(",", _options.Value.Namespace ?? Array.Empty<string>()));
    }

    /// <summary>
    /// Delegates tool invocation requests to the configured tool loader.
    /// </summary>
    /// <param name="request">The request context containing the tool name and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the output of the tool invocation.</returns>
    public async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        using var activity = _telemetry.StartActivity(ActivityName.ToolExecuted, request?.Server?.ClientInfo);

        if (request?.Params == null)
        {
            var content = new TextContentBlock
            {
                Text = "Cannot call tools with null parameters.",
            };

            activity?.SetStatus(ActivityStatusCode.Error)?.AddTag(TagName.ErrorDetails, content.Text);

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        return await _toolLoader.CallToolHandler(request!, cancellationToken);
    }

    /// <summary>
    /// Delegates tool discovery requests to the configured tool loader.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public async ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        using var activity = _telemetry.StartActivity(nameof(ListToolsHandler), request?.Server?.ClientInfo);
        return await _toolLoader.ListToolsHandler(request!, cancellationToken);
    }

    /// <summary>
    /// Disposes the tool loader and releases associated resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _toolLoader.DisposeAsync();
    }
}
