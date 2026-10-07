using System.Text;

namespace Watermelon
{
    public class AnalyticsIAPEventTrackData : IAnalyticsEventData
    {
        public IAPStatus status;
        public string product_id;
        public string failure_reason;
        public string isoCurrencyCode;
        public float localizedPrice;
        public string token;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"status: {status}");
            sb.AppendLine($"product_id: {(string.IsNullOrEmpty(product_id) ? "N/A" : product_id)}");
            sb.AppendLine($"failure_reason: {(string.IsNullOrEmpty(failure_reason) ? "N/A" : failure_reason)}");
            sb.AppendLine($"isoCurrencyCode: {(string.IsNullOrEmpty(isoCurrencyCode) ? "N/A" : isoCurrencyCode)}");
            sb.AppendLine($"localizedPrice: {localizedPrice}");
            return sb.ToString();
        }
    }
}
