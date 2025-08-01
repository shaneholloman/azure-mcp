// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureMcp.Core.Models.Command;
using AzureMcp.Core.Options;
using AzureMcp.Storage.Commands.Blob.Batch;
using AzureMcp.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AzureMcp.Storage.UnitTests.Blob.Batch;

public class BatchSetTierCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<BatchSetTierCommand> _logger;
    private readonly BatchSetTierCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;
    private readonly string _knownAccountName = "account123";
    private readonly string _knownContainerName = "container123";
    private readonly string _knownSubscriptionId = "sub123";
    private readonly string _knownTier = "Cool";
    private readonly string[] _knownBlobNames = ["blob1.txt", "blob2.txt", "blob3.txt"];

    public BatchSetTierCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<BatchSetTierCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("set-tier", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_AllBlobsSucceed_ReturnsSuccess()
    {
        // Arrange
        var expectedResult = (
            SuccessfulBlobs: _knownBlobNames.ToList(),
            FailedBlobs: new List<string>()
        );

        _storageService.SetBlobTierBatch(
            Arg.Is(_knownAccountName),
            Arg.Is(_knownContainerName),
            Arg.Is(_knownTier),
            Arg.Is<string[]>(x => x.SequenceEqual(_knownBlobNames)),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()).Returns(expectedResult);

        var args = _parser.Parse([
            "--account-name", _knownAccountName,
            "--container-name", _knownContainerName,
            "--tier-name", _knownTier,
            "--blob-names", "blob1.txt", "blob2.txt", "blob3.txt",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<BatchSetTierResult>(json);

        Assert.NotNull(result);
        Assert.Equal(3, result.SuccessfulBlobs.Count);
        Assert.Empty(result.FailedBlobs);
        Assert.Contains("blob1.txt", result.SuccessfulBlobs);
        Assert.Contains("blob2.txt", result.SuccessfulBlobs);
        Assert.Contains("blob3.txt", result.SuccessfulBlobs);
    }

    [Fact]
    public async Task ExecuteAsync_SomeBlobsFail_ReturnsPartialSuccess()
    {
        // Arrange
        var expectedResult = (
            SuccessfulBlobs: new List<string> { "blob1.txt", "blob2.txt" },
            FailedBlobs: new List<string> { "blob3.txt: Blob not found" }
        );

        _storageService.SetBlobTierBatch(
            Arg.Is(_knownAccountName),
            Arg.Is(_knownContainerName),
            Arg.Is(_knownTier),
            Arg.Is<string[]>(x => x.SequenceEqual(_knownBlobNames)),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()).Returns(expectedResult);

        var args = _parser.Parse([
            "--account-name", _knownAccountName,
            "--container-name", _knownContainerName,
            "--tier-name", _knownTier,
            "--blob-names", "blob1.txt", "blob2.txt", "blob3.txt",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<BatchSetTierResult>(json);

        Assert.NotNull(result);
        Assert.Equal(2, result.SuccessfulBlobs.Count);
        Assert.Single(result.FailedBlobs);
        Assert.Contains("blob1.txt", result.SuccessfulBlobs);
        Assert.Contains("blob2.txt", result.SuccessfulBlobs);
        Assert.Contains("blob3.txt: Blob not found", result.FailedBlobs);
    }

    [Theory]
    [InlineData("", false, "required")]
    [InlineData("--account-name account --container-name container", false, "required")]
    [InlineData("--account-name account --container-name container --tier-name Cool", false, "required")]
    [InlineData("--account-name account --container-name container --tier-name Cool --blob-names blob1.txt --subscription sub", true, null)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed, string? expectedErrorKeyword)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedResult = (
                SuccessfulBlobs: new List<string> { "blob1.txt" },
                FailedBlobs: new List<string>()
            );

            _storageService.SetBlobTierBatch(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>()).Returns(expectedResult);
        }

        var parseResult = _parser.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(400, response.Status);
            if (expectedErrorKeyword != null)
            {
                Assert.Contains(expectedErrorKeyword, response.Message.ToLower());
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";

        _storageService.SetBlobTierBatch(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()).ThrowsAsync(new Exception(expectedError));

        var args = _parser.Parse([
            "--account-name", _knownAccountName,
            "--container-name", _knownContainerName,
            "--tier-name", _knownTier,
            "--blob-names", "blob1.txt",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_NotFound()
    {
        // Arrange
        var requestFailedException = new Azure.RequestFailedException(404, "Not Found");

        _storageService.SetBlobTierBatch(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()).ThrowsAsync(requestFailedException);

        var args = _parser.Parse([
            "--account-name", _knownAccountName,
            "--container-name", _knownContainerName,
            "--tier-name", _knownTier,
            "--blob-names", "blob1.txt",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_Forbidden()
    {
        // Arrange
        var requestFailedException = new Azure.RequestFailedException(403, "Forbidden");

        _storageService.SetBlobTierBatch(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()).ThrowsAsync(requestFailedException);

        var args = _parser.Parse([
            "--account-name", _knownAccountName,
            "--container-name", _knownContainerName,
            "--tier-name", _knownTier,
            "--blob-names", "blob1.txt",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(403, response.Status);
        Assert.Contains("forbidden", response.Message.ToLower());
        Assert.Contains("troubleshooting", response.Message);
    }

    private class BatchSetTierResult
    {
        [JsonPropertyName("successfulBlobs")]
        public List<string> SuccessfulBlobs { get; set; } = [];

        [JsonPropertyName("failedBlobs")]
        public List<string> FailedBlobs { get; set; } = [];
    }
}
