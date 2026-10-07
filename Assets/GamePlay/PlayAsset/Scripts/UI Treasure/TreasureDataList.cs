using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "Treasure Data List", menuName = "Data/Treasure/Treasure Data List")]
    public class TreasureDataList : ScriptableObject
    {
        /// <summary>
        /// 每多少关卡刷新一次
        /// </summary>
        [SerializeField]
        private int levelRefresh;
        public int LevelRefresh => levelRefresh;
        [SerializeField]
        private List<TreasureData> treasureDataList;
        public List<TreasureData> TreasureDataListData => treasureDataList;

        public static TreasureDataList Data { get; private set; }

        public void Init()
        {
            Data = this;
        }
    }
}
