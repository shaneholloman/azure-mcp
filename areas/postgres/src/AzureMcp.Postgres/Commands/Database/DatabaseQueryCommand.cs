// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.Core.Services.Telemetry;
using AzureMcp.Postgres.Commands;
using AzureMcp.Postgres.Options;
using AzureMcp.Postgres.Options.Database;
using AzureMcp.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Postgres.Commands.Database;

public sealed class DatabaseQueryCommand(ILogger<DatabaseQueryCommand> logger) : BaseDatabaseCommand<DatabaseQueryOptions>(logger)
{
    private const string CommandTitle = "Query PostgreSQL Database";
    private readonly Option<string> _queryOption = PostgresOptionDefinitions.Query;
    public override string Name => "query";

    public override string Description => "Executes a query on the PostgreSQL database.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_queryOption);
    }

    protected override DatabaseQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueForOption(_queryOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var options = BindOptions(parseResult);
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

            IPostgresService pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");
            List<string> queryResult = await pgService.ExecuteQueryAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Database!, options.Query!);
            context.Response.Results = queryResult?.Count > 0 ?
                ResponseResult.Create(
                    new DatabaseQueryCommandResult(queryResult),
                    PostgresJsonContext.Default.DatabaseQueryCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while executing the query.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record DatabaseQueryCommandResult(List<string> QueryResult);
}
