namespace SDK.Client.Samples.LoadBalancing
{
    public enum AccountSelectionStrategyStatus
    {
        //Table $MetricsCapacityBlob doen't exists or empty. Storage account analytics need to be enabled. Please note that once analytics is enabled
        // data population happens after certain period of time (1 day time frame window)
        NoAnalyticsData,
        //$MetricsCapacityBlob table exists, but records are not up to date. Please make sure that storage account analytics is enabled
        AnalyticsDataIsOutofDate,
        //Storage account participated in selection algorythm but has not been selected
        NotSelected,
        //Storage account participated in selection algorythm and has been selected
        Selected
    }
}