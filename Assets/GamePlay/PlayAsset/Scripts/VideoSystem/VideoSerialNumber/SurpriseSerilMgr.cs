using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;
using Watermelon;

namespace VideoSystem
{

    #region  Surprise
    public class SurpriseSerilMgr
    {
        public (List<AB_Ctrl.SupriseData>, bool) GetSurpriseDatas()
        {
            return AB_Ctrl.Instance.GetSupriseList();
        }
        private SurpriseSave _saveData = null;
        private SurpriseSave saveData
        {
            get
            {
                if (_saveData == null)
                {
                    _saveData = SaveController.GetSaveObject<SurpriseSave>("SurpriseDataSave");
                }
                return _saveData;
            }
        }

        private List<SurpriseIdxSave> GetOrCreateList(string mainId)
        {
            if (string.IsNullOrEmpty(mainId))
                return new List<SurpriseIdxSave>();

            if (!saveData.surpriseDataIt.TryGetValue(mainId, out var list))
            {
                list = new List<SurpriseIdxSave>();
                saveData.surpriseDataIt[mainId] = list;
            }

            return list;
        }

        #region  out data
        /// <summary>
        /// 包含mainId信息的Surprise数据项，用于跨mainId的列表展示
        /// </summary>
        [System.Serializable]
        public class SurpriseItemOut
        {
            public string mainId;
            public int fileId;
            public int unlockTime;
            public bool isPlayed;
            public bool isLike;
        }

        List<SurpriseItemOut> allItemsOut = new List<SurpriseItemOut>();
        /// <summary>
        /// 获取所有mainId的列表，按isPlayed排前面（未播放的在前），然后按时间从大到小排序，支持正序/倒序
        /// </summary>
        /// <param name="ascending">true为正序（时间从小到大），false为倒序（时间从大到小）</param>
        /// <param name="onlyLiked">是否只获取喜欢的项</param>
        /// <returns>排序后的列表</returns>
        public List<SurpriseItemOut> GetSortedListByPlayedAndTime(bool ascending = false, bool onlyLiked = false)
        {
            allItemsOut.Clear();

            // 遍历所有mainId
            foreach (var kvp in saveData.surpriseDataIt)
            {
                string mainId = kvp.Key;
                if (kvp.Value != null)
                {
                    foreach (var item in kvp.Value)
                    {
                        // 如果只获取喜欢的，跳过不喜欢的
                        if (onlyLiked && !IsLike(mainId, item.fileId))//!item.isLike)
                            continue;

                        allItemsOut.Add(new SurpriseItemOut
                        {
                            mainId = mainId,
                            fileId = item.fileId,
                            unlockTime = item.unlockTime,
                            isPlayed = item.isPlayed,
                            isLike = IsLike(mainId, item.fileId) //item.isLike
                        });
                    }
                }
            }

            if (allItemsOut.Count == 0)
                return null;

            // 先按isPlayed排序（false在前，即未播放的在前），然后按unlockTime排序
            if (ascending)
            {
                // 正序：未播放在前，时间从小到大
                return allItemsOut.OrderBy(x => x.isPlayed ? 1 : 0)
                              .ThenBy(x => x.unlockTime)
                              .ToList();
            }
            else
            {
                // 倒序：未播放在前，时间从大到小
                return allItemsOut.OrderBy(x => x.isPlayed ? 1 : 0)
                              .ThenByDescending(x => x.unlockTime)
                              .ToList();
            }
        }
        #endregion

        public bool IsLike(string mainId, int fileId)
        {
            return GameGirlsLikeController.IsLiked(VideoSerilNumberManager.FormatSurpriseMainIdFileId(mainId, fileId));
        }

        /// <summary>
        /// 通过mainId和fileId设置isLike状态
        /// </summary>
        /// <param name="mainId">主ID</param>
        /// <param name="fileId">文件ID</param>
        /// <param name="isLike">要设置的状态</param>
        /// <returns>是否设置成功</returns>
        public bool SetIsLikeByFileId(string mainId, int fileId, bool isLike)
        {
            // IsLike(mainId, fileId)
            GameGirlsLikeController.SetLiked(VideoSerilNumberManager.FormatSurpriseMainIdFileId(mainId, fileId), isLike);

            // if (string.IsNullOrEmpty(mainId))
            //     return false;

            // var list = GetOrCreateList(mainId);
            // if (list == null || list.Count == 0)
            // {
            //     Debug.LogWarning($"No records found for mainId: {mainId}");
            //     return false;
            // }

            // var target = list.FirstOrDefault(x => x.fileId == fileId);
            // if (target == null)
            // {
            //     Debug.LogWarning($"FileId {fileId} not found in mainId {mainId}");
            //     return false;
            // }

            // target.isLike = isLike;
            // saveData.Flush();
            return true;
        }

        // /// <summary>
        // /// 通过mainId和fileId获取当前是否喜欢
        // /// </summary>
        // /// <param name="mainId">主ID</param>
        // /// <param name="fileId">文件ID</param>
        // /// <returns>
        // /// (bool isLike, bool found) 
        // /// isLike: 是否喜欢，found: 是否找到该数据
        // /// </returns>
        // public (bool isLike, bool found) GetIsLikeByFileId(string mainId, int fileId)
        // {
        //     if (string.IsNullOrEmpty(mainId))
        //         return (false, false);

        //     var list = GetOrCreateList(mainId);
        //     if (list == null || list.Count == 0)
        //     {
        //         Debug.LogWarning($"No records found for mainId: {mainId}");
        //         return (false, false);
        //     }

        //     var target = list.FirstOrDefault(x => x.fileId == fileId);
        //     if (target == null)
        //     {
        //         Debug.LogWarning($"FileId {fileId} not found in mainId {mainId}");
        //         return (false, false);
        //     }

        //     return (target.isLike, true);
        // }

        /// <summary>
        /// 通过mainId和fileId获取当前是否喜欢（简化版）
        /// </summary>
        /// <param name="mainId">主ID</param>
        /// <param name="fileId">文件ID</param>
        /// <param name="defaultValue">当找不到数据时返回的默认值</param>
        /// <returns>是否喜欢</returns>
        // public bool IsLike(string mainId, int fileId, bool defaultValue = false)
        // {
        //     var result = GetIsLikeByFileId(mainId, fileId);
        //     return result.found ? result.isLike : defaultValue;
        // }

        /// <summary>
        /// 获取所有喜欢的项列表，按isPlayed排前面，然后按时间从大到小排序
        /// </summary>
        /// <param name="ascending">true为正序（时间从小到大），false为倒序（时间从大到小）</param>
        /// <returns>喜欢的项列表</returns>
        public List<SurpriseItemOut> GetLikedList(bool ascending = false)
        {
            return GetSortedListByPlayedAndTime(ascending, true);
        }
        /// <summary>
        /// 设置levelid
        /// </summary>
        /// <param name="mainid"></param>
        /// <param name="fileid"></param>
        /// <param name="levelid"></param>
        public void SetOneFileLevel(string mainid, int fileid, int levelid)
        {
            if (!saveData.surpriseDataIt.ContainsKey(mainid)) return;

            var item = saveData.surpriseDataIt[mainid].FirstOrDefault(x => x.fileId == fileid);
            if (item == null) return;

            item.levelId = levelid;
            saveData.Flush();
        }

        public int GetOneFileLevel(string mainid, int fileid)
        {
            if (!saveData.surpriseDataIt.ContainsKey(mainid)) return -1;

            var item = saveData.surpriseDataIt[mainid].FirstOrDefault(x => x.fileId == fileid);
            if (item == null) return -1;

            return item.levelId;
        }

        /// <summary>
        /// 设置谋个文件已经被玩过了
        /// </summary>
        /// <param name="mainid"></param>
        /// <param name="fileid"></param>
        public void SetOneFilePlayed(string mainid, int fileid)
        {
            if (!saveData.surpriseDataIt.ContainsKey(mainid)) return;

            var item = saveData.surpriseDataIt[mainid].FirstOrDefault(x => x.fileId == fileid);
            if (item == null) return;

            item.isPlayed = true;
            saveData.Flush();
        }

        /// <summary>
        /// 获取这个mainid的最近一个需要解开的fileid，另外也可以用来判断是否已经用完了
        /// 返回的是已经获取过的id ,返回游标位置
        /// </summary>
        /// <param name="mainId"></param>
        /// <returns></returns>
        public int GetMainIdIt(string mainId)
        {
            if (string.IsNullOrEmpty(mainId))
                return -1;

            var config = GetSurpriseDatas().Item1.FirstOrDefault(x => x.mainId == mainId);
            if (config == null)
            {
                Debug.LogWarning($"Surprise mainId not found: {mainId}");
                return -1;
            }

            var list = GetOrCreateList(mainId);
            if (list.Count == 0)
            {
                return config.fileRange[1] + 1;
            }

            return list.Min(x => x.fileId);
        }
        /// <summary>
        /// 解锁多张图，并返回当前已获取的文件编号列表
        /// </summary>
        /// <param name="mainId"></param>
        /// <param name="num"></param>
        /// <returns>返回本次解锁的文件编号列表</returns>
        public List<int> SetGetFileId(string mainId, int num)
        {
            var unlockedFileIds = new List<int>();

            if (string.IsNullOrEmpty(mainId))
                return unlockedFileIds;

            if (num <= 0)
            {
                Debug.LogWarning($"Unlock count must be greater than zero: {num}");
                return unlockedFileIds;
            }

            var config = GetSurpriseDatas().Item1.FirstOrDefault(x => x.mainId == mainId);
            if (config == null)
            {
                Debug.LogWarning($"Surprise mainId not found: {mainId}");
                return unlockedFileIds;
            }

            var list = GetOrCreateList(mainId);
            int nextFileId;
            if (list.Count == 0)
            {
                nextFileId = config.fileRange[1];
            }
            else
            {
                var minFileId = list.Min(x => x.fileId);
                nextFileId = minFileId - 1;
            }

            while (num > 0 && nextFileId >= config.fileRange[0])
            {
                if (!list.Any(x => x.fileId == nextFileId))
                {
                    list.Add(new SurpriseIdxSave
                    {
                        mainId = mainId,
                        fileId = nextFileId,
                        unlockTime = saveData.maxGetNum,
                        isPlayed = false,
                        levelId = -1,
                    });

                    unlockedFileIds.Add(nextFileId);
                    saveData.maxGetNum++;
                    num--;
                }
                nextFileId--;
            }

            if (unlockedFileIds.Count == 0)
            {
                Debug.Log($"Surprise mainId already fully unlocked or no files available: {mainId}");
            }
            else
            {
                saveData.Flush();
            }

            return unlockedFileIds;
        }

        public List<SurpriseIdxSave> Get(string mainId)
        {
            if (string.IsNullOrEmpty(mainId))
                return new List<SurpriseIdxSave>();

            return GetOrCreateList(mainId);
        }


        #region 进游戏的时候存储一个id 数据，在游戏完成的时候然后记录

        private string currentPlayingMainId = null;
        private int currentPlayingFileId = -1;
        public void ResetCurrentPlaying()
        {
            currentPlayingMainId = null;
            currentPlayingFileId = -1;
        }
        public void SetCurrentPlaying(string mainId, int fileId)
        {
            ResetCurrentPlaying();

            currentPlayingMainId = mainId;
            currentPlayingFileId = fileId;
        }

        public (string mainId, int fileId) GetCurrentPlayingId()
        {
            return (currentPlayingMainId, currentPlayingFileId);
        }

        public void CompleteCurrentPlaying()
        {
            if (string.IsNullOrEmpty(currentPlayingMainId) || currentPlayingFileId < 0)
            {
                return;
            }

            SetOneFilePlayed(currentPlayingMainId, currentPlayingFileId);

            // Reset after completion
            currentPlayingMainId = null;
            currentPlayingFileId = -1;
        }

        #endregion


        [System.Serializable]
        public class SurpriseSave : ISaveObject
        {
            [SerializeField]
            public Dictionary<string, List<SurpriseIdxSave>> surpriseDataIt = new();
            [SerializeField]
            public int maxGetNum = 0;

            public void Flush()
            {
            }
        }

        [System.Serializable]
        public class SurpriseIdxSave
        {
            [SerializeField]
            public string mainId;
            [SerializeField]
            public int fileId;
            [SerializeField]
            public int unlockTime;//解锁时间，实际就是解锁的时候的最大的索引，排序
            [SerializeField]
            public bool isPlayed;//只有玩了一次才能看到视频

            [SerializeField]
            public int levelId = -1;
        }



        /// <summary>
        /// 测试方法：设置示例数据，包含特定的 mainId 和 fileId 组合，以及全局排序时间
        /// </summary>
        public void SetupTestData()
        {
#if UNITY_EDITOR
            // 清空现有数据
            saveData.surpriseDataIt.Clear();
            saveData.maxGetNum = 0;

            // 定义测试数据：mainId, fileId, isPlayed, isLike, unlockTime（全局排序顺序）
            // unlockTime 值越大表示越晚解锁/生成
            var testData = new List<(string mainId, int fileId, bool isPlayed, bool isLike, int unlockTime)>
            {
                ("650010", 10, true, true, 5),    // 第5个解锁，已播放，已喜欢
                ("650010", 9, true, false, 12),   // 第12个解锁，已播放，未喜欢
                ("650010", 8, false, true, 3),    // 第3个解锁，未播放，已喜欢
                ("650020", 7, false, false, 8),   // 第8个解锁，未播放，未喜欢

                ("650030", 5, true, true, 2),     // 第2个解锁，已播放，已喜欢
                ("650050", 4, true, false, 6),    // 第6个解锁，已播放，未喜欢
                ("650010", 3, false, false, 1),   // 第1个解锁，未播放，未喜欢（最早的）

                ("650080", 20, false, true, 15),  // 第15个解锁，未播放，已喜欢（最晚的）
                ("650070", 19, false, false, 10), // 第10个解锁，未播放，未喜欢
                ("650060", 18, true, true, 7),    // 第7个解锁，已播放，已喜欢
            };

            foreach (var (mainId, fileId, isPlayed, isLike, unlockTime) in testData)
            {
                // 获取或创建该 mainId 的列表
                var list = GetOrCreateList(mainId);

                // 添加数据项，使用指定的 unlockTime
                list.Add(new SurpriseIdxSave
                {
                    mainId = mainId,
                    fileId = fileId,
                    unlockTime = unlockTime,  // 使用指定的全局排序值
                    isPlayed = isPlayed,
                    // isLike = IsLike(mainId, fileId),//isLike
                });
            }

            // 更新 maxGetNum 为最大的解锁时间
            saveData.maxGetNum = testData.Max(x => x.unlockTime);
            saveData.Flush();

            Debug.Log($"测试数据设置完成。共创建 {testData.Count} 条数据，分布在 {saveData.surpriseDataIt.Count} 个 mainId 中。");
            Debug.Log($"最大解锁时间（全局排序序号）: {saveData.maxGetNum}");

            // 打印数据用于验证
            PrintAllData();

            SaveController.Save(true);
        }

        /// <summary>
        /// 辅助方法：打印所有数据用于验证
        /// </summary>
        private void PrintAllData()
        {
            Debug.Log("=== 测试数据验证（按原始顺序） ===");
            var allItems = GetSortedListByPlayedAndTime(false);
            if (allItems == null || allItems.Count == 0)
            {
                Debug.Log("没有数据可打印。");
                return;
            }

            Debug.Log("排序规则：未播放的在前，然后按解锁时间从大到小（最新的在前）");
            int index = 1;
            foreach (var item in allItems)
            {
                Debug.Log($"#{index++} | mainId: {item.mainId}, fileId: {item.fileId}, " +
                          $"是否已播放: {item.isPlayed}, 是否喜欢: {item.isLike}, " +
                          $"解锁时间(全局排序): {item.unlockTime}");
            }
            Debug.Log($"总数据条数: {allItems.Count}");
            Debug.Log($"总 mainId 数量: {saveData.surpriseDataIt.Count}");

            // 额外打印：按正序排列（时间从小到大）
            Debug.Log("\n=== 正序排列（时间从小到大） ===");
            var ascendingItems = GetSortedListByPlayedAndTime(true);
            if (ascendingItems != null)
            {
                int idx = 1;
                foreach (var item in ascendingItems)
                {
                    Debug.Log($"#{idx++} | mainId: {item.mainId}, fileId: {item.fileId}, " +
                              $"是否已播放: {item.isPlayed}, 是否喜欢: {item.isLike}, " +
                              $"解锁时间(全局排序): {item.unlockTime}");
                }
            }
#endif
        }

    }
    #endregion


}