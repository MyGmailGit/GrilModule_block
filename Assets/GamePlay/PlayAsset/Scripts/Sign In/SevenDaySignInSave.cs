namespace Watermelon
{
    [System.Serializable]
    public class SevenDaySignInSave : ISaveObject
    {
        public int ClaimedDaysInCycle;
        public long LastClaimDateBinary;
        public long LastProcessedDateBinary;
        public long LastSyncedUnixTime;
        public bool AutoPopupShownForCurrentDay;
        public bool InitialAutoPopupSkipped;

        public void Flush()
        {

        }
    }
}
