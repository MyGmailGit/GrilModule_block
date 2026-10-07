namespace Watermelon
{
    [System.Serializable]
    public class LevelRemoteConfigData : RemoteConfigData
    {
        public override string Key => "level1";
        public override bool PrettyPrint => false;

        public int duration = -1;

        public string hash;
    }
}

