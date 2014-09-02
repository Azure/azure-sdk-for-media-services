using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using SDK.Client.Samples.LoadBalancing;

namespace Microsoft.WindowsAzure.MediaServices.Client.AccountLoadBalancing
{
    /// <summary>
    /// Implements load balancing strategy for picking storage account with least available space based on storage analytics data
    /// </summary>
    public class LeastCapacityOrDefaultAccountStrategy 
    {

        private readonly Lazy<CloudStorageAccount> _selectedAccount;
        private static Dictionary<CloudStorageAccount, AccountSelectionStrategyStatus> _selectionResults;

        public LeastCapacityOrDefaultAccountStrategy(IEnumerable<CloudStorageAccount> storageAccounts)
        {
            if (storageAccounts == null)
            {
                throw new ArgumentNullException("storageAccounts");
            }
            _selectedAccount = new Lazy<CloudStorageAccount>(() => SelectAccountWithLeastOccupiedSpace(storageAccounts));
        }

        /// <summary>
        /// Selects the account for input asset.
        /// </summary>
        /// <param name="selectionResults"></param>
        /// <returns></returns>
        public CloudStorageAccount SelectAccountForInputAsset(out Dictionary<CloudStorageAccount, AccountSelectionStrategyStatus> selectionResults)
        {
            CloudStorageAccount returned = _selectedAccount.Value;
            selectionResults = _selectionResults;
            return returned;
        }

        /// <summary>
        /// Selects the account for out put asset.
        /// </summary>
        /// <returns></returns>
        public CloudStorageAccount SelectAccountForOutPutAsset(out Dictionary<CloudStorageAccount, AccountSelectionStrategyStatus> selectionResults)
        {
            CloudStorageAccount returned = _selectedAccount.Value;
            selectionResults = _selectionResults;
            return returned;
        }

        public static CloudStorageAccount SelectAccountWithLeastOccupiedSpace(IEnumerable<CloudStorageAccount> accounts)
        {
            //Filter
            Dictionary<CloudStorageAccount,long> accountsWithMetrics = new Dictionary<CloudStorageAccount, long>();
            _selectionResults =new Dictionary<CloudStorageAccount, AccountSelectionStrategyStatus>();
            foreach (CloudStorageAccount cloudStorageAccount in accounts)
            {
                bool enabledAnalytics = false;
                bool metricsTableExists = cloudStorageAccount.MetricsTableExists();

               

                //Selecting available capacity metrics for last 2 days 
                IEnumerable<Tuple<DateTime, long>> capacities = cloudStorageAccount.TryGetBlobUserDataCapacityMetric(DateTime.Now.AddDays(-2), out enabledAnalytics);

                
                if (capacities != null)
                {
                    var capacitiesList = capacities.ToList();
                    if (capacitiesList.Count > 0)
                    {
                        accountsWithMetrics.Add(cloudStorageAccount, capacitiesList.OrderBy(c => c.Item1).Last().Item2);
                    }
                }
                else
                {
                    if (metricsTableExists)
                    {
                        _selectionResults.Add(cloudStorageAccount, AccountSelectionStrategyStatus.AnalyticsDataIsOutofDate);
                    }
                    else
                    {
                        _selectionResults.Add(cloudStorageAccount, AccountSelectionStrategyStatus.NoAnalyticsData);
                    }
                }
            }

            var winner = accountsWithMetrics.OrderBy(c => c.Value).FirstOrDefault().Key;

            _selectionResults.Add(winner,AccountSelectionStrategyStatus.Selected);

            foreach (var looser in accountsWithMetrics.OrderBy(c => c.Value).Skip(1))
            {
                _selectionResults.Add(looser.Key, AccountSelectionStrategyStatus.NotSelected);
            }
            
            return winner;

        }
    }
}