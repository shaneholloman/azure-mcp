// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureMcp.BicepSchema.Commands;
using AzureMcp.BicepSchema.Services;
using AzureMcp.Core.Models.Command;
using AzureMcp.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.BicepSchema.UnitTests;

public class BicepSchemaGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBicepSchemaService _bicepSchemaService;
    private readonly ILogger<BicepSchemaGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly BicepSchemaGetCommand _command;
    private readonly Parser _parser;

    public BicepSchemaGetCommandTests()
    {
        _bicepSchemaService = Substitute.For<IBicepSchemaService>();
        _logger = Substitute.For<ILogger<BicepSchemaGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_bicepSchemaService);

        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenResourceTypeExists()
    {
        var args = _parser.Parse([
        "--resource-type", "Microsoft.Sql/servers/databases/schemas"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);


        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<BicepSchemaResultWrapper>(json);
        var name = result?.BicepSchemaResult.FirstOrDefault()?.Name;

        Assert.Contains("Microsoft.Sql/servers/databases/schemas@2023-08-01", name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenResourceTypeDoesNotExist()
    {

        var args = _parser.Parse([
        "--resource-type", "Microsoft.Unknown/virtualRandom",
        "--subscription", "knownSubscription"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);


        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<BicepSchemaResultWrapper>(json);
        Assert.Contains("Resource type Microsoft.Unknown/virtualRandom not found.", result?.message);
    }

    private class BicepSchemaResultWrapper
    {
        [JsonPropertyName("BicepSchemaResult")]
        public List<BicepSchemaItem> BicepSchemaResult { get; set; } = new();

        public string? message { get; set; } = string.Empty;
    }

    private class BicepSchemaItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

}
