// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using AzureMcp.AppConfig.Commands.KeyValue;
using AzureMcp.AppConfig.Services;
using AzureMcp.Core.Models.Command;
using AzureMcp.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static AzureMcp.AppConfig.Commands.KeyValue.KeyValueSetCommand;

namespace AzureMcp.AppConfig.UnitTests.KeyValue;

[Trait("Area", "AppConfig")]
public class KeyValueSetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly ILogger<KeyValueSetCommand> _logger;
    private readonly KeyValueSetCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public KeyValueSetCommandTests()
    {
        _appConfigService = Substitute.For<IAppConfigService>();
        _logger = Substitute.For<ILogger<KeyValueSetCommand>>();

        _command = new(_logger);
        _parser = new(_command.GetCommand());
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_appConfigService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_SetsKeyValue_WhenValidParametersProvided()
    {
        // Arrange
        var args = _parser.Parse([
            "--subscription", "sub123",
            "--account-name", "account1",
            "--key", "my-key",
            "--value", "my-value"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _appConfigService.Received(1).SetKeyValue(
            "account1",
            "my-key",
            "my-value",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyValueSetCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("my-value", result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_SetsKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange
        var args = _parser.Parse([
            "--subscription", "sub123",
            "--account-name", "account1",
            "--key", "my-key",
            "--value", "my-value",
            "--label", "prod"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _appConfigService.Received(1).SetKeyValue(
            "account1",
            "my-key",
            "my-value",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            "prod");

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyValueSetCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("my-value", result.Value);
        Assert.Equal("prod", result.Label);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        _appConfigService.SetKeyValue(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>())
            .ThrowsAsync(new Exception("Failed to set key-value"));

        var args = _parser.Parse([
            "--subscription", "sub123",
            "--account-name", "account1",
            "--key", "my-key",
            "--value", "my-value"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Failed to set key-value", response.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--subscription sub123")]
    [InlineData("--subscription sub123 --account-name account1")]
    [InlineData("--subscription sub123 --account-name account1 --key my-key")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(string args)
    {
        // Arrange
        var parsedArgs = _parser.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
