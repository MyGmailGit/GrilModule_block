namespace Watermelon
{
    [System.Serializable]
    public class LevelSave : ISaveObject
    {
        // 记录玩家已解锁  到达的最高关卡索引，零基编号
        public int MaxReachedLevelIndex = 0;

        /// <summary>
        /// 实际加载的内部关卡索引，可能与 DisplayLevelIndex 不同（随机关卡替换时）
        /// </summary>
        public int RealLevelIndex = 0;
        /// <summary>
        /// 当前玩家看到/选择的显示关卡索引，零基编号
        /// </summary>
        public int DisplayLevelIndex = 0;
        /// 标记当前是否正在播放随机替换的关卡
        public bool IsPlayingRandomLevel = false;

        /// 上一次实际播放的关卡索引，用于随机关卡选择时避免重复
        public int LastPlayerLevelIndex = -1;

        /// 最后一次完成的关卡索引，用来判断是否首次完成当前显示关卡
        public int CompletedLevelIndex = -1;

        /// 标记当前关卡是否刚开始，用于区分首发开始状态
        public bool FirstStart = true;


        public int DisplaySpecialLevelIndex = 0;
        public int RealSpecialLevelIndex = -1;



        public void Flush()
        {
            // ISaveObject 接口方法，目前无需额外刷新逻辑
        }
    }
}
