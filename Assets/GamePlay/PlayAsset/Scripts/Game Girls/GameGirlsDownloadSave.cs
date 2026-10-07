using System.Collections.Generic;

namespace Watermelon
{
    [System.Serializable]
    public class GameGirlsDownloadSave : ISaveObject
    {
        public List<string> DownloadedFileIds = new List<string>();

        public void Flush()
        {

        }
    }
}
