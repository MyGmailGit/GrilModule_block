namespace Watermelon
{
    [System.Serializable]
    public class ProgressRemoteConfigData : RemoteConfigData
    {
        public override string Key => "progress";

        public int skipMenuUntilLevel = 15;
    }
}
