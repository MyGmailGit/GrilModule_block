namespace Watermelon
{
    [System.Serializable]
    public class GameGirlsHomeVideoSave : ISaveObject
    {
        public bool HasCustomSelection;
        public string SelectedLevelId;

        public void Flush()
        {

        }
    }
}
