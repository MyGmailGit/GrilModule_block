namespace Watermelon
{
    public class AnalyticsAdRevenueData : IAnalyticsEventData
    {
        public string countryCode;
        public double revenue;
        public string networkName;
        public string adUnitId;
        public string adFormat;
        public string mediation;
        public int networkfirmid;
        public string placement;
        // public bool isvpn;
        public string adsource;

        public override string ToString()
        {
            return $"AnalyticsAdRevenueData(countryCode={countryCode ?? string.Empty}, revenue={revenue}, networkName={networkName ?? string.Empty}, adUnitId={adUnitId ?? string.Empty}, adFormat={adFormat ?? string.Empty}, mediation={mediation ?? string.Empty}, networkfirmid={networkfirmid}, placement={placement ?? string.Empty}, adsource={adsource ?? string.Empty})";
        }
    }
}
