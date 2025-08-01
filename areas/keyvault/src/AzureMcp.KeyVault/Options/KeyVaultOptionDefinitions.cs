// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.KeyVault.Options;

public static class KeyVaultOptionDefinitions
{
    public const string VaultNameParam = "vault";
    public const string KeyNameParam = "key";
    public const string KeyTypeParam = "key-type";
    public const string IncludeManagedKeysParam = "include-managed";
    public const string SecretNameParam = "secret";
    public const string SecretValueParam = "value";
    public const string CertificateNameParam = "certificate";

    public static readonly Option<string> VaultName = new(
        $"--{VaultNameParam}",
        "The name of the Key Vault."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> KeyName = new(
        $"--{KeyNameParam}",
        "The name of the key to retrieve/modify from the Key Vault."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> KeyType = new(
        $"--{KeyTypeParam}",
        "The type of key to create (RSA, EC)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<bool> IncludeManagedKeys = new(
        $"--{IncludeManagedKeysParam}",
        "Whether or not to include managed keys in results."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> SecretName = new(
        $"--{SecretNameParam}",
        "The name of the secret."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> SecretValue = new(
        $"--{SecretValueParam}",
        "The value to set for the secret."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> CertificateName = new(
        $"--{CertificateNameParam}",
        "The name of the certificate."
    )
    {
        IsRequired = true
    };
}
