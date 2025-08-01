// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Security.KeyVault.Secrets;
using AzureMcp.Core.Models.Command;
using AzureMcp.Core.Options;
using AzureMcp.KeyVault.Commands.Secret;
using AzureMcp.KeyVault.Services;
using AzureMcp.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AzureMcp.KeyVault.UnitTests.Secret;

public class SecretGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<SecretGetCommand> _logger;
    private readonly SecretGetCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownSecretName = "knownSecretName";
    private const string _knownSecretValue = "knownSecretValue";
    private readonly KeyVaultSecret _knownKeyVaultSecret;

    public SecretGetCommandTests()
    {
        _keyVaultService = Substitute.For<IKeyVaultService>();
        _logger = Substitute.For<ILogger<SecretGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_keyVaultService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());

        _knownKeyVaultSecret = new KeyVaultSecret(_knownSecretName, _knownSecretValue);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSecret()
    {
        // Arrange
        _keyVaultService.GetSecret(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSecretName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(_knownKeyVaultSecret);

        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var retrievedSecret = JsonSerializer.Deserialize<SecretGetResult>(json);

        Assert.NotNull(retrievedSecret);
        Assert.Equal(_knownSecretName, retrievedSecret.Name);
        Assert.Equal(_knownSecretValue, retrievedSecret.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidObject_IfSecretNameIsEmpty()
    {
        // Arrange - No need to mock service since validation should fail before service is called
        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--secret", "",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert - Should return validation error response
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _keyVaultService.GetSecret(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(expectedError, response.Message);
    }

    private class SecretGetResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        [JsonPropertyName("notBefore")]
        public DateTimeOffset? NotBefore { get; set; }

        [JsonPropertyName("expiresOn")]
        public DateTimeOffset? ExpiresOn { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonPropertyName("updatedOn")]
        public DateTimeOffset? UpdatedOn { get; set; }
    }
}
