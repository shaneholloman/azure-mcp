// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Core.Models.Option;

namespace AzureMcp.Core.Options;

public class SubscriptionOptions : GlobalOptions
{
    [JsonPropertyName(OptionDefinitions.Common.SubscriptionName)]
    public string? Subscription { get; set; }
}
