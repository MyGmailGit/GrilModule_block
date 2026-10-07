using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon;

public class SpecialSerilMgr
{
    public (string mainId, int[] fileRange)? GetList()
    {
        return AB_Ctrl.Instance.specialList;
    }

    private SpecialSave _saveData = null;
    private SpecialSave saveData
    {
        get
        {
            if (_saveData == null)
            {
                _saveData = SaveController.GetSaveObject<SpecialSave>("SpecialSaveData");
            }
            return _saveData;
        }
    }

    public class LastTimeGetIds
    {
        /// <summary>
        /// 最近的一次解锁的Mainid
        /// </summary>
        public string mainId_last = null;
        /// <summary>
        /// 最近一次解锁的fileId
        /// </summary>
        public int fileId_last = -1;
    }
    public readonly List<LastTimeGetIds> lastTimeGetIds = new List<LastTimeGetIds>();
    public void ClearLastTimeGet()
    {
        lastTimeGetIds.Clear();
    }

    /// <summary>
    /// 随机获取一个没有使用的fileid，如果没有了就返回null
    /// </summary>
    /// <returns></returns>
    public (string mainId, int fileId)? GetOneFileToUse()
    {
        var list = GetList();
        if (list == null || !list.HasValue) return null;

        var (mainId, fileRange) = list.Value;
        int start = fileRange[0];
        int end = fileRange[1];

        // 获取所有已使用的fileId
        HashSet<int> usedFileIds = new HashSet<int>();
        if (saveData.specialDataIt.ContainsKey(mainId))
        {
            foreach (var item in saveData.specialDataIt[mainId])
            {
                usedFileIds.Add(item.fileId);
            }
        }

        // 找出所有未使用的fileId
        List<int> availableFileIds = new List<int>();
        for (int i = start; i <= end; i++)
        {
            if (!usedFileIds.Contains(i))
            {
                availableFileIds.Add(i);
            }
        }

        if (availableFileIds.Count == 0) return null;

        // 随机选择一个
        int randomIndex = UnityEngine.Random.Range(0, availableFileIds.Count);
        return (mainId, availableFileIds[randomIndex]);
    }
    /// <summary>
    /// 设置一个fileid 已经被使用了，但是没有played，没有levelid，和解锁的unlockTime
    /// </summary>
    /// <param name="mainid"></param>
    /// <param name="fileid"></param>
    public void SetOneFileUse(string mainid, int fileid)
    {
        if (!saveData.specialDataIt.ContainsKey(mainid))
        {
            saveData.specialDataIt[mainid] = new List<SpecialIdxSave>();
        }

        // 检查是否已经存在
        var existing = saveData.specialDataIt[mainid].FirstOrDefault(x => x.fileId == fileid);
        if (existing != null) return;

        // 创建新的条目
        var newItem = new SpecialIdxSave
        {
            mainId = mainid,
            fileId = fileid,
            unlockTime = saveData.maxGetNum + 1,
            isPlayed = false,
            isLike = false,
            levelId = -1
        };

        saveData.specialDataIt[mainid].Add(newItem);
        saveData.maxGetNum++;
        saveData.Flush();

        lastTimeGetIds.Add(new LastTimeGetIds()
        {
            mainId_last = mainid,
            fileId_last = fileid,
        });

    }
    /// <summary>
    /// 设置levelid
    /// </summary>
    /// <param name="mainid"></param>
    /// <param name="fileid"></param>
    /// <param name="levelid"></param>
    public void SetOneFileLevel(string mainid, int fileid, int levelid)
    {
        if (!saveData.specialDataIt.ContainsKey(mainid)) return;

        var item = saveData.specialDataIt[mainid].FirstOrDefault(x => x.fileId == fileid);
        if (item == null) return;

        item.levelId = levelid;
        saveData.Flush();
    }
    public int GetOneFileLevel(string mainid, int fileid)
    {
        if (!saveData.specialDataIt.ContainsKey(mainid)) return -1;

        var item = saveData.specialDataIt[mainid].FirstOrDefault(x => x.fileId == fileid);
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
        if (!saveData.specialDataIt.ContainsKey(mainid)) return;

        var item = saveData.specialDataIt[mainid].FirstOrDefault(x => x.fileId == fileid);
        if (item == null) return;

        item.isPlayed = true;
        saveData.Flush();
    }

    [System.Serializable]
    public class SpecialIdxSaveOut
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
        public bool isLike;
        [SerializeField]
        public int levelId = -1;
    }

    /// <summary>
    /// 获取所有已经解锁之后的file，按照unlockTime倒序排列
    /// </summary>
    /// <returns></returns>
    public List<SpecialIdxSaveOut> GetTotalUseFiles()
    {
        List<SpecialIdxSaveOut> result = new List<SpecialIdxSaveOut>();

        foreach (var kvp in saveData.specialDataIt)
        {
            foreach (var item in kvp.Value)
            {
                result.Add(new SpecialIdxSaveOut
                {
                    mainId = item.mainId,
                    fileId = item.fileId,
                    unlockTime = item.unlockTime,
                    isPlayed = item.isPlayed,
                    isLike = item.isLike,
                    levelId = item.levelId
                });
            }
        }

        // 按照unlockTime倒序排列
        return result.OrderByDescending(x => x.unlockTime).ToList();
    }

    /// <summary>
    /// 生成测试数据
    /// </summary>
    public void GenerateTestData()
    {
#if UNITY_EDITOR
        // 清除现有数据
        saveData.specialDataIt.Clear();
        saveData.maxGetNum = 0;

        // 直接在函数内部填写测试数据
        var testDataList = new List<(string mainId, int fileId, bool isPlayed, int levelId)>
        {
            ("6200", 1, false, -1),     // 已解锁，未玩
            ("6200", 2, true, 101),     // 已解锁，已玩，关卡101
            ("6200", 3, false, -1),     // 已解锁，未玩
            ("6200", 5, true, 102),     // 已解锁，已玩，关卡102
            ("6200", 8, false, -1),     // 已解锁，未玩
            ("6200", 10, true, 103),    // 已解锁，已玩，关卡103
        };

        int currentUnlockTime = 1;

        foreach (var (mainId, fileId, isPlayed, levelId) in testDataList)
        {
            if (!saveData.specialDataIt.ContainsKey(mainId))
            {
                saveData.specialDataIt[mainId] = new List<SpecialIdxSave>();
            }

            var newItem = new SpecialIdxSave
            {
                mainId = mainId,
                fileId = fileId,
                unlockTime = currentUnlockTime++,
                isPlayed = isPlayed,
                isLike = false,
                levelId = levelId
            };

            saveData.specialDataIt[mainId].Add(newItem);
        }

        saveData.maxGetNum = currentUnlockTime - 1;
        saveData.Flush();

        Debug.Log($"生成测试数据完成，共 {testDataList.Count} 条数据");

        SaveController.Save(true);
#endif
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



    #region  SaveData 

    [System.Serializable]
    public class SpecialSave : ISaveObject
    {
        [SerializeField]
        public Dictionary<string, List<SpecialIdxSave>> specialDataIt = new();
        [SerializeField]
        public int maxGetNum = 0;

        public void Flush()
        {
        }
    }

    [System.Serializable]
    public class SpecialIdxSave
    {
        public string mainId;
        public int fileId;
        public int unlockTime;//解锁时间，实际就是解锁的时候的最大的索引，排序
        public bool isPlayed;//只有玩了一次才能看到视频
        public bool isLike;
        public int levelId = -1;
    }
    #endregion
}
