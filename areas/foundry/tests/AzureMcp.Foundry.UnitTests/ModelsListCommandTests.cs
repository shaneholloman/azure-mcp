// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureMcp.Core.Models.Command;
using AzureMcp.Core.Options;
using AzureMcp.Foundry.Commands;
using AzureMcp.Foundry.Models;
using AzureMcp.Foundry.Services;
using AzureMcp.Tests;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AzureMcp.Foundry.UnitTests;

public class ModelsListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public ModelsListCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsModels_WhenModelsExist()
    {
        var expectedModels = new List<ModelInformation>
        {
            new() { Id = "model1", Name = "Model 1", Publisher = "Publisher 1" },
            new() { Id = "model2", Name = "Model 2", Publisher = "Publisher 2" }
        };

        _foundryService.ListModels(
                Arg.Any<bool>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedModels);

        var command = new ModelsListCommand();
        var args = command.GetCommand().Parse("");
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ModelsListCommandResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Models);
        Assert.Equal(expectedModels.Count, result.Models.Count());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsModels_WhenModelsExistWithFlags()
    {
        string publisherName = "TestPublisher";
        string license = "TestLicense";
        string modelName = "TestModel";
        var expectedModels = new List<ModelInformation>
        {
            new() { Id = "model1", Name = "Model 1", Publisher = "Publisher 1" },
            new() { Id = "model2", Name = "Model 2", Publisher = "Publisher 2" }
        };

        _foundryService.ListModels(
                Arg.Any<bool>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedModels);

        var command = new ModelsListCommand();
        var args = command.GetCommand().Parse(["--search-for-free-playground", "--publisher-name", publisherName, "--license-name", license, "--model-name", modelName]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ModelsListCommandResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Models);
        Assert.Equal(expectedModels.Count, result.Models.Count());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoModels()
    {
        _foundryService.ListModels(
                Arg.Any<bool>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(new List<ModelInformation>());

        var command = new ModelsListCommand();
        var args = command.GetCommand().Parse("");
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";

        _foundryService.ListModels(
                Arg.Any<bool>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new ModelsListCommand();
        var args = command.GetCommand().Parse("");
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class ModelsListCommandResult
    {
        [JsonPropertyName("models")]
        public IEnumerable<ModelInformation> Models { get; set; } = [];
    }
}
