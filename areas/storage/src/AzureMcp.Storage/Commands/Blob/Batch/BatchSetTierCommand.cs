// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.Core.Services.Telemetry;
using AzureMcp.Storage.Commands.Blob.Container;
using AzureMcp.Storage.Options;
using AzureMcp.Storage.Options.Blob.Batch;
using AzureMcp.Storage.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Storage.Commands.Blob.Batch;

public sealed class BatchSetTierCommand(ILogger<BatchSetTierCommand> logger) : BaseContainerCommand<BatchSetTierOptions>()
{
    private const string CommandTitle = "Set Access Tier for Multiple Blobs";
    private readonly ILogger<BatchSetTierCommand> _logger = logger;

    private readonly Option<string> _tierOption = StorageOptionDefinitions.Tier;
    private readonly Option<string[]> _blobNamesOption = StorageOptionDefinitions.BlobNames;

    public override string Name => "set-tier";

    public override string Description =>
        $"""
        Set access tier for multiple blobs in a single batch operation. This tool efficiently changes the 
        storage tier for multiple blobs simultaneously in a single request. Different tiers offer different 
        trade-offs between storage costs, access costs, and retrieval latency. Requires {StorageOptionDefinitions.AccountName}, 
        {StorageOptionDefinitions.ContainerName}, {StorageOptionDefinitions.TierName}, and {StorageOptionDefinitions.BlobNames}.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_tierOption);
        command.AddOption(_blobNamesOption);
    }

    protected override BatchSetTierOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Tier = parseResult.GetValueForOption(_tierOption);
        options.BlobNames = parseResult.GetValueForOption(_blobNamesOption);
        return options;
    }

    [McpServerTool(Destructive = false, ReadOnly = false, Title = CommandTitle)]
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

            var storageService = context.GetService<IStorageService>();
            var result = await storageService.SetBlobTierBatch(
                options.Account!,
                options.Container!,
                options.Tier!,
                options.BlobNames!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new BatchSetTierCommandResult(result.SuccessfulBlobs, result.FailedBlobs),
                StorageJsonContext.Default.BatchSetTierCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error setting blob tier batch. Account: {Account}, Container: {Container}, Tier: {Tier}, BlobCount: {BlobCount}.",
                options.Account, options.Container, options.Tier, options.BlobNames?.Length ?? 0);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record BatchSetTierCommandResult(List<string> SuccessfulBlobs, List<string> FailedBlobs);
}
