using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;

/// <summary>
/// Returns a set of <see cref="CloudStorageAccount" /> associated with Azure Media Services using app.config as storage for connections strings
/// </summary>
public static class StorageAccountConfigHelper
{

    /// <summary>
    /// Gets the storage account mapping from config
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public static Dictionary<CloudStorageAccount, IStorageAccount> GetStorageAccountMappingFromConfig(CloudMediaContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }
        var storageAccounts = new Dictionary<CloudStorageAccount, IStorageAccount>();
        var registeredAccounts = context.StorageAccounts.ToList();

        foreach (var registeredAccount in registeredAccounts)
        {
            var connectionString =ConfigurationManager.AppSettings["Storage_" + registeredAccount.Name];
                
            //If user don't provide all connection strings to storage accounts we are skipping this storage account. 
            //We also skipping if connection string is not correct
            if (!String.IsNullOrEmpty(connectionString))
            {
                CloudStorageAccount storageAccount = null;
                CloudStorageAccount.TryParse(connectionString, out storageAccount);
                if (storageAccount != null)
                {
                    storageAccounts.Add(storageAccount, registeredAccount);
                }
                {
                    Debug.WriteLine("Unable to parse connection string for storage account {0}", registeredAccount.Name);
                }
            }
            else
            {
                Debug.WriteLine("Storage account {0} has missing setting with connection string in App.config", registeredAccount.Name);
            }

        }
        return storageAccounts;
    }
}