// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using AzureMcp.Storage.Commands.Account;
using AzureMcp.Storage.Commands.Blob;
using AzureMcp.Storage.Commands.Blob.Batch;
using AzureMcp.Storage.Commands.Blob.Container;
using AzureMcp.Storage.Commands.DataLake.Directory;
using AzureMcp.Storage.Commands.DataLake.FileSystem;
using AzureMcp.Storage.Commands.Table;
using AzureMcp.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Storage;

public class StorageSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IStorageService, StorageService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Storage command group
        var storage = new CommandGroup("storage", "Storage operations - Commands for managing and accessing Azure Storage resources. Includes operations for containers, blobs, and tables.");
        rootGroup.AddSubGroup(storage);

        // Create Storage subgroups
        var storageAccount = new CommandGroup("account", "Storage account operations - Commands for listing and managing Storage account in your Azure subscription.");
        storage.AddSubGroup(storageAccount);

        var tables = new CommandGroup("table", "Storage table operations - Commands for working with Azure Table Storage, including listing and querying table.");
        storage.AddSubGroup(tables);

        var blobs = new CommandGroup("blob", "Storage blob operations - Commands for uploading, downloading, and managing blob in your Azure Storage accounts.");
        storage.AddSubGroup(blobs);

        // Create Batch subgroup under blobs
        var batch = new CommandGroup("batch", "Storage batch operations - Commands for performing batch operations on multiple storage blobs efficiently.");
        blobs.AddSubGroup(batch);

        // Create a containers subgroup under blobs
        var blobContainer = new CommandGroup("container", "Storage blob container operations - Commands for managing blob container in your Azure Storage accounts.");
        blobs.AddSubGroup(blobContainer);

        // Create Data Lake subgroup under storage
        var dataLake = new CommandGroup("datalake", "Data Lake Storage operations - Commands for managing Azure Data Lake Storage Gen2 file systems and paths.");
        storage.AddSubGroup(dataLake);

        // Create file-system subgroup under datalake
        var fileSystem = new CommandGroup("file-system", "Data Lake file system operations - Commands for managing file systems and paths in Azure Data Lake Storage Gen2.");
        dataLake.AddSubGroup(fileSystem);

        // Create directory subgroup under datalake
        var directory = new CommandGroup("directory", "Data Lake directory operations - Commands for managing directories in Azure Data Lake Storage Gen2.");
        dataLake.AddSubGroup(directory);

        // Register Storage commands
        storageAccount.AddCommand("list", new AccountListCommand(
            loggerFactory.CreateLogger<AccountListCommand>()));
        tables.AddCommand("list", new TableListCommand(
            loggerFactory.CreateLogger<TableListCommand>()));

        blobs.AddCommand("list", new BlobListCommand(loggerFactory.CreateLogger<BlobListCommand>()));

        batch.AddCommand("set-tier", new BatchSetTierCommand(
            loggerFactory.CreateLogger<BatchSetTierCommand>()));

        blobContainer.AddCommand("list", new ContainerListCommand(
            loggerFactory.CreateLogger<ContainerListCommand>()));
        blobContainer.AddCommand("details", new ContainerDetailsCommand(
            loggerFactory.CreateLogger<ContainerDetailsCommand>()));

        fileSystem.AddCommand("list-paths", new FileSystemListPathsCommand(
            loggerFactory.CreateLogger<FileSystemListPathsCommand>()));

        directory.AddCommand("create", new DirectoryCreateCommand(
            loggerFactory.CreateLogger<DirectoryCreateCommand>()));
    }
}
