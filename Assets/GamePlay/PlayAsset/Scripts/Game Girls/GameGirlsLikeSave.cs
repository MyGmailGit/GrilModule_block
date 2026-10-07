using System.Collections.Generic;

namespace Watermelon
{
    [System.Serializable]
    public class GameGirlsLikeSave : ISaveObject
    {
        public List<string> LikedFileIds = new List<string>();
        public bool ShowLikedOnly;

        public void Flush()
        {

        }
    }
}
