using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;
using Watermelon;

namespace VideoSystem
{
    // 	6000106001~6000106016
    // 	6000154001~6000154016
    // 	6000204001~6000204016
    // 	6000242001~6000242016
    // 	6000246001~6000246016
    // 	6000249001~6000249016
    // 	6000273001~6000273016
    // 	6330303001~6330303016
    // 	6330333001~6330333016
    // 	6330336001~6330336016
    // 	6330342001~6330342016
    // 	6330352001~6330352016
    // 	6330370001~6330370016
    // 	6330375001~6330375016
    // 	6330379001~6330379016
    // 	6330395001~6330395016
    // 	6330445001~6330445016
    // 	6330464001~6330464016
    // 	6330466001~6330466016
    // 	6330482001~6330482016
    // 	6330484001~6330484016
    // 	6330503001~6330503016
    // 	6330517001~6330517016

    // 其中数据分为主编号和文件编号
    // 比如：6000106001中60001060是主编号，01-16是文件编号

    //     其中文件编号01-16分成4份，，每一份一个阶段

    // 两个功能模块
    // 1.如果当前没有正在进行中的阶段，就返回4个随机主编号下没有使用的阶段(这个阶段是13-16，09-12，05-08，01-04)的使用顺序。如果已经随机出了这4个但是并没有选择就记录这个4个阶段，后面无论调用多少次都返回这4个，直到选择了一个阶段为止
    //    1)记录选择当前正在进行的阶段

    // 2.如果有正在进行的阶段，就返回当前阶段按照顺序的没有使用的图片编号


    // 开始或阶段完成时	GetCandidates()	获取4个候选（第一次固定，之后随机）
    // 用户选择后	SelectCandidate(index)	记录选择的阶段
    // 播放阶段时	GetRemainingImagesInCurrentStage()	获取剩余未播放的图片列表
    // 播放阶段时	GetNextImageInCurrentStage()	获取下一张图片编号
    // 用户看完一张	UseImageInCurrentStage(fileNumber)	标记该图片已完成
    // 看完一张并获取下一张	CompleteCurrentImageAndGetNext(fileNumber)	组合方法，标记并返回下一张
    // 任何时候	GetCurrentProgress()	获取当前进行中的阶段信息
    // 任何时候	GetCurrentStageProgress()	获取当前阶段进度(已用/总数)


    // 自定义的数据类，用于持久化存储
    [System.Serializable]
    public class VideoSerilIdData : ISaveObject
    {
        [SerializeField]
        // 记录每个主编号的阶段使用情况
        public List<MainGroupData> mainGroups = new List<MainGroupData>();

        [SerializeField]
        // 当前候选（4个主编号+阶段索引）
        public List<CandidateData> currentCandidates = new List<CandidateData>();

        [SerializeField]
        // 当前进行中的阶段
        public ProgressData _currentProgress = null;
        public ProgressData currentProgress
        {
            get
            {
                return _currentProgress;
            }
            set
            {
                _currentProgress = value;
            }
        }

        // 新增：独立存储上一张图片信息
        [SerializeField]
        public PreviousImageData previousImage = new PreviousImageData();



        [SerializeField]
        // 是否是第一次获取数据（用于判断是否使用固定主编号）
        public bool isFirstTime = true;

        public void Flush()
        {
        }

        // 辅助方法：根据mainId获取MainGroupData
        public MainGroupData GetMainGroup(string mainId)
        {
            return mainGroups.FirstOrDefault(m => m.mainId == mainId);
        }
    }

    [System.Serializable]
    public class MainGroupData
    {
        public string mainId;
        public List<int> usedStages = new List<int>(); // 已完成的阶段索引 0~3
        public List<StageFileData> stageFiles = new List<StageFileData>();
        public int maxFileNum;

        // 辅助方法：获取指定阶段的文件使用数据
        public StageFileData GetStageFile(int stageIndex)
        {
            return stageFiles.FirstOrDefault(s => s.stageIndex == stageIndex);
        }
    }

    [System.Serializable]
    public class StageFileData
    {
        public int stageIndex; // 0~3
        public int stageStart; // 阶段起始文件编号
        public int stageEnd;   // 阶段结束文件编号

        public List<int> usedFiles = new List<int>(); // 已使用的文件绝对编号（如：1,2,3...16）

        // 辅助方法：获取阶段范围
        public int[] GetStageRange()
        {
            return new int[] { stageStart, stageEnd };
        }

        // 辅助方法：检查文件是否在当前阶段范围内
        public bool IsFileInRange(int fileNumber)
        {
            return fileNumber >= stageStart && fileNumber <= stageEnd;
        }
    }

    [System.Serializable]
    public class CandidateData
    {
        public string mainId;
        public int stageIndex;
    }

    [System.Serializable]
    public class ProgressData
    {
        public string mainId;
        public int stageIndex;
        public int currentFileIndex = -1; // 当前正在使用的图片绝对编号（1-16），-1表示还未开始
    }

    [System.Serializable]
    public class PreviousImageData
    {
        public string mainId = "";
        public int fileIndex = -1; // -1 表示没有上一张

        public bool HasPreviousImage()
        {
            return !string.IsNullOrEmpty(mainId) && fileIndex != -1;
        }

        public void Clear()
        {
            mainId = "";
            fileIndex = -1;
        }

        public void Set(string mainId, int fileIndex)
        {
            this.mainId = mainId;
            this.fileIndex = fileIndex;
        }
    }

    public class VideoSerilNumberManager : Singleton<VideoSerilNumberManager>
    {
        // 所有主编号列表
        private List<string> allMainIds { get { return AB_Ctrl.Instance.allMainIds; } }
        // = new List<string>
        // {
        //     "60001060", "60001540", "60002040", "60002420", "60002460", "60002490", "60002730",
        //     "63303030", "63303330", "63303360", "63303420", "63303520", "63303700", "63303750",
        //     "63303790", "63303950", "63304450", "63304640", "63304660", "63304820", "63304840",
        //     "63305030", "63305170"
        // };

        // 固定的前4个主编号（用于第一次加载）
        private List<string> fixedFirstMainIds { get { return AB_Ctrl.Instance.fixedFirstMainIds; } }
        // = new List<string>
        // {
        //     "60001060", "60001540", "60002040", "60002420"
        // };

        // 阶段顺序（需求：13-16, 09-12, 05-08, 01-04）
        private List<int[]> stageFileRanges { get { return AB_Ctrl.Instance.stageFileRanges; } }
        // = new List<int[]>
        // {
        //     new int[] { 13, 16 }, // 阶段 0
        //     new int[] { 9, 12 },  // 阶段 1
        //     new int[] { 5, 8 },   // 阶段 2
        //     new int[] { 1, 4 }    // 阶段 3
        // };

        private VideoSerilIdData saveData;

        public SurpriseSerilMgr surpriseData = new();
        public SpecialSerilMgr specialData = new();

        public static string FormatMainIdFileId(string mainid, int fileId)
        {
            return $"{mainid}{fileId:D2}";
        }
        public static string FormatSurpriseMainIdFileId(string mainid, int fileId)
        {
            return $"{mainid}{fileId:D3}";
        }


        protected override void OnInit()
        {
            base.OnInit();
            LoadData();
        }

        public override void Release()
        {
            SaveData();
        }

        // 加载持久化数据
        private void LoadData()
        {
            // 使用 SaveController 加载数据
            saveData = SaveController.GetSaveObject<VideoSerilIdData>("VideoIdData");

            if (saveData == null)
            {
                // 没有存档，初始化新数据
                InitNewData();
                return;
            }

            // 数据已加载，保留所有已使用过的数据和进行中的阶段
        }

        // 初始化新数据（采用懒加载模式）
        private void InitNewData()
        {
            saveData = new VideoSerilIdData();
            saveData.mainGroups = new List<MainGroupData>();
            saveData.currentCandidates = new List<CandidateData>();
            saveData.currentProgress = null;
            saveData.isFirstTime = true; // 标记为第一次

            SaveData();
        }

        // 保存数据到持久化
        private void SaveData()
        {
            saveData.Flush();
        }

        // 辅助方法：确保指定的mainId数据存在，如果不存在则创建
        private MainGroupData EnsureMainGroupExists(string mainId)
        {
            var mainGroup = saveData.GetMainGroup(mainId);
            if (mainGroup != null)
            {
                return mainGroup;
            }
            int maxFileNumTemp = stageFileRanges[0][1];
            // 创建新的MainGroupData
            var newGroup = new MainGroupData
            {
                mainId = mainId,
                usedStages = new List<int>(),
                stageFiles = new List<StageFileData>(),
                maxFileNum = maxFileNumTemp,
            };

            for (int i = 0; i < stageFileRanges.Count; i++)
            {
                var range = stageFileRanges[i]; // 从当前数据源获取范围
                newGroup.stageFiles.Add(new StageFileData
                {
                    stageIndex = i,
                    stageStart = range[0],  // 保存起始编号
                    stageEnd = range[1],    // 保存结束编号
                    usedFiles = new List<int>()
                });
            }

            saveData.mainGroups.Add(newGroup);
            return newGroup;
        }

        // 修改 GetCandidates 方法，添加 forceRefresh 参数
        /// <summary>
        /// 获取候选列表
        /// </summary>
        /// <param name="forceRefresh">是否强制刷新（忽略已记录的候选）</param>
        /// <returns>返回4个候选</returns>
        public List<(string mainId, int stageIndex, int[] fileRange)> GetCandidates(bool forceRefresh = false)
        {
            return GetCandidatesInternal(forceRefresh, allowWhileInProgress: false);
        }

        /// <summary>
        /// 预加载专用：即使当前已有进行中的阶段，也可以提前生成候选列表
        /// </summary>
        /// <param name="forceRefresh">是否强制刷新（忽略已记录的候选）</param>
        /// <returns>返回4个候选</returns>
        public List<(string mainId, int stageIndex, int[] fileRange)> GetCandidatesForPreload(bool forceRefresh = false)
        {
            return GetCandidatesInternal(forceRefresh, allowWhileInProgress: true);
        }

        private List<(string mainId, int stageIndex, int[] fileRange)> GetCandidatesInternal(bool forceRefresh, bool allowWhileInProgress)
        {
            if (!allowWhileInProgress && saveData.currentProgress != null)
            {
                Debug.LogWarning("已有进行中的阶段，请勿调用 GetCandidates");
                return null;
            }

            // 如果不是强制刷新，且已有候选且未选择，直接返回之前的候选
            if (!forceRefresh && saveData.currentCandidates != null && saveData.currentCandidates.Count == 4)
            {
                return ConvertCandidatesToResult();
            }

            // 如果是强制刷新，清除当前候选
            if (forceRefresh)
            {
                Debug.Log("强制刷新候选列表");
                saveData.currentCandidates = null;
                if (!allowWhileInProgress)
                {
                    saveData.currentProgress = null;
                }
            }

            // 如果是第一次获取数据，使用固定的4个主编号和阶段4（阶段索引0）
            if (saveData.isFirstTime)
            {
                Debug.Log("第一次获取数据，使用固定的4个主编号和阶段4");

                saveData.currentCandidates = new List<CandidateData>();

                foreach (var mainId in fixedFirstMainIds)
                {
                    var mainGroup = EnsureMainGroupExists(mainId);
                    if (mainGroup != null)
                    {
                        // 使用阶段0（文件编号13-16）
                        saveData.currentCandidates.Add(new CandidateData
                        {
                            mainId = mainId,
                            stageIndex = 0 // 阶段0对应13-16
                        });
                    }
                    else
                    {
                        Debug.LogError($"找不到固定的主编号: {mainId}");
                    }
                }

                // 标记已经不是第一次了
                saveData.isFirstTime = false;

                // 保存到持久化
                SaveData();

                return ConvertCandidatesToResult();
            }

            // 随机选 4 个不同主编号（只选还有未完成阶段的）
            var availableMainIds = allMainIds.Where(id =>
            {
                var mainGroup = saveData.GetMainGroup(id);
                return mainGroup == null || mainGroup.usedStages.Count < 4;
            }).ToList();

            if (availableMainIds.Count < 4)
            {
                Debug.LogError("没有足够的未完成主编号");
                return null;
            }

            var random = new System.Random();
            var selectedMainIds = availableMainIds.OrderBy(x => random.Next()).Take(4).ToList();

            saveData.currentCandidates = new List<CandidateData>();

            foreach (var mainId in selectedMainIds)
            {
                var mainGroup = EnsureMainGroupExists(mainId);
                // 找到该主编号未使用的阶段（按阶段0,1,2,3先后顺序）
                for (int stageIdx = 0; stageIdx < 4; stageIdx++)
                {
                    if (!mainGroup.usedStages.Contains(stageIdx))
                    {
                        saveData.currentCandidates.Add(new CandidateData
                        {
                            mainId = mainId,
                            stageIndex = stageIdx
                        });
                        break;
                    }
                }
            }

            // 如果某个主编号所有阶段已用（理论上前面取availableMainIds已过滤，但保险）
            if (saveData.currentCandidates.Count < 4)
            {
                Debug.LogError("候选不足");
                saveData.currentCandidates = null;
                return null;
            }

            // 保存到持久化
            SaveData();

            return ConvertCandidatesToResult();
        }

        private List<(string mainId, int stageIndex, int[] fileRange)> ConvertCandidatesToResult()
        {
            // var result = new List<(string, int, int[])>();
            // foreach (var candidate in saveData.currentCandidates)
            // {
            //     var mainGroup = saveData.GetMainGroup(candidate.mainId);

            //     result.Add((candidate.mainId, candidate.stageIndex, stageFileRanges[candidate.stageIndex]));
            // }
            // return result;

            var result = new List<(string, int, int[])>();

            foreach (var candidate in saveData.currentCandidates)
            {
                // 从 MainGroupData 中获取该阶段的固定范围
                var mainGroup = saveData.GetMainGroup(candidate.mainId);
                if (mainGroup != null)
                {
                    var stageData = mainGroup.GetStageFile(candidate.stageIndex);
                    if (stageData != null)
                    {
                        // 使用保存在 StageFileData 中的范围
                        result.Add((candidate.mainId, candidate.stageIndex, new int[] { stageData.stageStart, stageData.stageEnd }));
                        continue;
                    }
                }

                // 降级方案：如果找不到数据，使用当前数据源
                Debug.LogWarning($"找不到主编号 {candidate.mainId} 阶段 {candidate.stageIndex} 的数据，使用默认范围");
                if (candidate.stageIndex < stageFileRanges.Count)
                {
                    result.Add((candidate.mainId, candidate.stageIndex, stageFileRanges[candidate.stageIndex]));
                }
                else
                {
                    Debug.LogError($"阶段索引 {candidate.stageIndex} 超出范围");
                }
            }

            return result;
        }
        public void SelectCandidate(int candidateIndex)
        {
            if (saveData.currentCandidates == null || candidateIndex < 0 || candidateIndex >= saveData.currentCandidates.Count)
            {
                Debug.LogError("无效的候选索引");
                return;
            }

            var selected = saveData.currentCandidates[candidateIndex];

            // 确保主编号数据存在
            var mainGroup = EnsureMainGroupExists(selected.mainId);
            if (mainGroup == null)
            {
                Debug.LogError($"找不到主编号数据: {selected.mainId}");
                return;
            }

            var stageData = mainGroup.GetStageFile(selected.stageIndex);
            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {selected.mainId}, {selected.stageIndex}");
                return;
            }

            // 清理 usedFiles 中无效的记录（防止脏数据）
            var validUsedFiles = new List<int>();
            foreach (var fileId in stageData.usedFiles)
            {
                if (fileId >= stageData.stageStart && fileId <= stageData.stageEnd)
                {
                    validUsedFiles.Add(fileId);
                }
                else
                {
                    Debug.LogWarning($"发现无效的文件记录: {fileId}，已清理");
                }
            }
            stageData.usedFiles = validUsedFiles;
            stageData.usedFiles.Sort();

            // 获取该阶段的第一张未使用图片
            int firstUnusedFile = -1;
            for (int fileNum = stageData.stageStart; fileNum <= stageData.stageEnd; fileNum++)
            {
                if (!stageData.usedFiles.Contains(fileNum))
                {
                    firstUnusedFile = fileNum;
                    break;
                }
            }

            if (firstUnusedFile == -1)
            {
                Debug.LogError($"阶段 {selected.mainId} 阶段 {selected.stageIndex} 已无未使用图片");
                return;
            }

            saveData.currentProgress = new ProgressData
            {
                mainId = selected.mainId,
                stageIndex = selected.stageIndex,
                currentFileIndex = firstUnusedFile, // 设置为第一张未使用的图片
            };

            // 清除候选（因为已经选择了）
            saveData.currentCandidates = null;

            // 保存到持久化
            SaveData();

            Debug.Log($"选择了候选: {selected.mainId} 阶段 {selected.stageIndex}，当前图片: {firstUnusedFile}");
        }

        /// <summary>
        /// 直接开始使用指定主编号的指定阶段
        /// </summary>
        /// <param name="mainId">主编号</param>
        /// <param name="stageIndex">阶段索引 (0-3)</param>
        /// <returns>是否成功开始</returns>
        public bool StartStageDirectly(string mainId, int stageIndex)
        {
            // 1. 检查是否有进行中的阶段
            if (saveData.currentProgress != null)
            {
                Debug.LogWarning($"已有进行中的阶段: {saveData.currentProgress.mainId} 阶段 {saveData.currentProgress.stageIndex}，请先完成当前阶段");
                return false;
            }

            // 2. 验证参数
            if (string.IsNullOrEmpty(mainId))
            {
                Debug.LogError("mainId 不能为空");
                return false;
            }

            if (stageIndex < 0 || stageIndex >= 4)
            {
                Debug.LogError($"阶段索引 {stageIndex} 无效，必须在 0-3 之间");
                return false;
            }

            // 3. 确保主编号数据存在
            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup == null)
            {
                Debug.LogError($"找不到主编号数据: {mainId}");
                return false;
            }

            // 4. 检查该阶段是否已完成
            if (mainGroup.usedStages.Contains(stageIndex))
            {
                Debug.LogError($"主编号 {mainId} 的阶段 {stageIndex} 已经完成");
                return false;
            }

            // 5. 获取阶段数据
            var stageData = mainGroup.GetStageFile(stageIndex);
            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {mainId}, {stageIndex}");
                return false;
            }

            // 6. 清理无效的文件记录（防止脏数据）
            var validUsedFiles = new List<int>();
            foreach (var fileId in stageData.usedFiles)
            {
                if (fileId >= stageData.stageStart && fileId <= stageData.stageEnd)
                {
                    validUsedFiles.Add(fileId);
                }
                else
                {
                    Debug.LogWarning($"发现无效的文件记录: {fileId}，已清理");
                }
            }
            stageData.usedFiles = validUsedFiles;
            stageData.usedFiles.Sort();

            // 7. 检查该阶段是否还有未使用的图片
            int firstUnusedFile = -1;
            for (int fileNum = stageData.stageStart; fileNum <= stageData.stageEnd; fileNum++)
            {
                if (!stageData.usedFiles.Contains(fileNum))
                {
                    firstUnusedFile = fileNum;
                    break;
                }
            }

            if (firstUnusedFile == -1)
            {
                Debug.LogError($"阶段 {mainId} 阶段 {stageIndex} 已无未使用图片，应该标记为已完成但未标记");
                // 自动标记为已完成
                if (!mainGroup.usedStages.Contains(stageIndex))
                {
                    mainGroup.usedStages.Add(stageIndex);
                    mainGroup.usedStages.Sort();
                    SaveData();
                }
                return false;
            }

            // 8. 清除候选列表（如果有）
            if (saveData.currentCandidates != null)
            {
                saveData.currentCandidates = null;
            }

            // 9. 设置当前进度
            saveData.currentProgress = new ProgressData
            {
                mainId = mainId,
                stageIndex = stageIndex,
                currentFileIndex = firstUnusedFile, // 设置为第一张未使用的图片
            };

            // 10. 保存到持久化
            SaveData();

            Debug.Log($"直接开始阶段: {mainId} 阶段 {stageIndex}，当前图片: {firstUnusedFile}");
            return true;
        }

        /// <summary>
        /// 模块2：有进行中阶段时，获取剩余未用图片编号（按文件编号从小到大）
        /// </summary>
        /// <returns></returns>
        public List<int> GetRemainingImagesInCurrentStage()
        {
            if (saveData.currentProgress == null)
            {
                Debug.LogWarning("没有进行中的阶段");
                return null;
            }

            var mainId = saveData.currentProgress.mainId;
            var stageIdx = saveData.currentProgress.stageIndex;

            // 获取该阶段的已使用文件（确保mainGroup存在）
            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup == null)
            {
                Debug.LogError($"找不到主编号数据: {mainId}");
                return null;
            }

            var stageData = mainGroup.GetStageFile(stageIdx);
            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {mainId}, {stageIdx}");
                return null;
            }

            var usedFiles = stageData.usedFiles; // 现在存储的是绝对编号
            // var range = stageFileRanges[stageIdx];
            int start = stageData.stageStart;
            int end = stageData.stageEnd;

            List<int> remaining = new List<int>();
            for (int fileNum = start; fileNum <= end; fileNum++)
            {
                if (!usedFiles.Contains(fileNum)) // 直接比较绝对编号
                    remaining.Add(fileNum);
            }
            return remaining;
        }

        /// <summary>
        /// 设置上一张图片（由 UseImageInCurrentStage 调用）
        /// </summary>
        private void SetPreviousImage(string mainId, int fileIndex)
        {
            if (saveData.previousImage == null)
            {
                saveData.previousImage = new PreviousImageData();
            }
            saveData.previousImage.Set(mainId, fileIndex);
            SaveData();
        }
        /// <summary>
        /// 获取上一张图片的完整信息
        /// </summary>
        /// <returns>返回 (mainId, fileId)，如果没有则返回 null</returns>
        public (string mainId, int fileId)? GetPreviousImage()
        {
            if (saveData.previousImage == null || !saveData.previousImage.HasPreviousImage())
            {
                return null;
            }
            return (saveData.previousImage.mainId, saveData.previousImage.fileIndex);
        }

        /// <summary>
        /// 获取上一张图片的完整编号字符串
        /// </summary>
        public string GetPreviousImageFullId()
        {
            var info = GetPreviousImage();
            if (!info.HasValue)
            {
                return "";
            }
            return FormatMainIdFileId(info.Value.mainId, info.Value.fileId);
        }

        /// <summary>
        /// 清除上一张图片记录
        /// </summary>
        public void ClearPreviousImage()
        {
            if (saveData.previousImage != null)
            {
                saveData.previousImage.Clear();
                SaveData();
            }
        }

        /// <summary>
        /// 使用当前阶段的某一张图片（传入实际文件编号）
        /// </summary>
        /// <param name="fileNumber"></param>
        public void UseImageInCurrentStage()//int fileNumber)
        {
            if (saveData.currentProgress == null)
            {
                Debug.LogError("无进行中阶段");
                return;
            }

            var mainId = saveData.currentProgress.mainId;
            var stageIdx = saveData.currentProgress.stageIndex;
            int fileNumber = saveData.currentProgress.currentFileIndex;

            // var range = stageFileRanges[stageIdx];
            // if (fileNumber < range[0] || fileNumber > range[1])
            // {
            //     Debug.LogError($"文件编号 {fileNumber} 不在当前阶段范围 {range[0]}-{range[1]}");
            //     return;
            // }

            // 获取该阶段的已使用文件列表（确保mainGroup存在）
            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup == null)
            {
                Debug.LogError($"找不到主编号数据: {mainId}");
                return;
            }

            var stageData = mainGroup.GetStageFile(stageIdx);
            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {mainId}, {stageIdx}");
                return;
            }

            var usedFiles = stageData.usedFiles;

            if (!usedFiles.Contains(fileNumber))
            {
                // 在标记当前图片为已完成之前，先记录为上一张
                if (saveData.currentProgress.currentFileIndex != -1)
                {
                    // 保存当前图片到独立存储
                    SetPreviousImage(saveData.currentProgress.mainId, saveData.currentProgress.currentFileIndex);
                }

                usedFiles.Add(fileNumber);
                usedFiles.Sort();

                // 记录当前正在使用的图片（使用绝对编号）
                saveData.currentProgress.currentFileIndex = fileNumber;
                // 保存进度
                SaveData();

                // 如果该阶段 4 张图都已用，标记阶段完成并清除当前进度
                if (usedFiles.Count == 4)
                {
                    CompleteCurrentStage();
                }
            }
            else
            {
                Debug.LogWarning($"图片 {fileNumber} 已经使用过了");
            }
        }

        private void CompleteCurrentStage()
        {
            if (saveData.currentProgress == null) return;

            var mainId = saveData.currentProgress.mainId;
            var stageIdx = saveData.currentProgress.stageIndex;

            // 标记阶段为已完成（确保mainGroup存在）
            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup != null && !mainGroup.usedStages.Contains(stageIdx))
            {
                mainGroup.usedStages.Add(stageIdx);
                mainGroup.usedStages.Sort();
            }

            // 清除当前进度
            saveData.currentProgress = null;

            // 保存数据
            SaveData();
        }

        // 在 VideoSerilNumberManager 类中添加以下方法

        /// <summary>
        /// 获取当前阶段的下一张未使用的图片编号
        /// </summary>
        /// <returns>返回下一张图片编号，如果没有则返回-1</returns>
        public (string mainId, int fileId)? GetNextImageInCurrentStage()
        {
            if (saveData.currentProgress == null)
            {
                Debug.LogWarning("没有进行中的阶段");
                return null;
            }

            var mainId = saveData.currentProgress.mainId;
            var stageIdx = saveData.currentProgress.stageIndex;

            // 获取该阶段的已使用文件（确保mainGroup存在）
            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup == null)
            {
                Debug.LogError($"找不到主编号数据: {mainId}");
                return null;
            }

            var stageData = mainGroup.GetStageFile(stageIdx);
            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {mainId}, {stageIdx}");
                return null;
            }

            var usedFiles = stageData.usedFiles; // 现在存储的是绝对编号
            // var range = stageFileRanges[stageIdx];
            int start = stageData.stageStart;//range[0];
            int end = stageData.stageEnd;//range[1];

            // 按顺序查找第一个未使用的图片
            for (int fileNum = start; fileNum <= end; fileNum++)
            {
                if (!usedFiles.Contains(fileNum)) // 直接比较绝对编号
                {
                    return (mainId, fileNum);
                }
            }

            // 如果所有图片都已使用，返回-1
            Debug.Log("当前阶段所有图片已使用完毕");
            return null;
        }

        /// <summary>
        /// 完成当前图片并自动获取下一张图片
        /// </summary>
        /// <param name="currentFileNumber">当前完成的图片编号</param>
        /// <returns>返回下一张图片编号，如果没有则返回-1</returns>
        public int CompleteCurrentImageAndGetNext()
        {
            // 先标记当前图片为已完成
            UseImageInCurrentStage();//currentFileNumber);

            // 检查是否还有进行中的阶段（如果当前阶段已完成，UseImageInCurrentStage会清空currentProgress）
            if (saveData.currentProgress == null)
            {
                Debug.Log("当前阶段已完成，没有下一张图片");
                return -1;
            }

            // 获取下一张图片
            // return GetNextImageInCurrentStage();

            // 获取下一张图片
            var nextImage = GetNextImageInCurrentStage();

            // 如果有下一张，自动设置为当前图片
            if (nextImage != null)
            {
                SetCurrentImageInCurrentStage(nextImage.Value.fileId);
            }

            return nextImage.Value.fileId;

        }
        /// <summary>
        /// 获取当前进行中的完整图
        /// </summary>
        /// <returns></returns>
        public (string mainId, int stageIndex, int fileId)? GetCurrentFullName()
        {
            if (saveData.currentProgress == null) return null;

            var progress = saveData.currentProgress;

            // 如果还没有开始播放任何图片，返回null
            if (progress.currentFileIndex < 0)
            {
                Debug.Log("当前阶段还未开始播放任何图片");
                return null;
            }

            // var range = stageFileRanges[progress.stageIndex];
            // int fileId = range[0] + progress.currentFileIndex;

            // return (progress.mainId, progress.stageIndex, fileId);
            return (progress.mainId, progress.stageIndex, progress.currentFileIndex);
        }
        /// <summary>
        /// 获取阶段开始和结束
        /// </summary>
        /// <returns></returns>
        public (int startIdx, int endIdx)? GetCurrentStageStartEnd()
        {
            if (saveData.currentProgress == null) return null;

            var mainGroup = EnsureMainGroupExists(saveData.currentProgress.mainId);
            var stageData = mainGroup?.GetStageFile(saveData.currentProgress.stageIndex);

            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {saveData.currentProgress.mainId}, {saveData.currentProgress.stageIndex}");
                return null;
            }
            return (stageData.stageStart, stageData.stageEnd);
        }
        /// <summary>
        /// 设置当前阶段正在播放的图片（用于外部控制播放进度）
        /// </summary>
        /// <param name="fileNumber">实际文件编号</param>
        public void SetCurrentImageInCurrentStage(int fileNumber)
        {
            if (saveData.currentProgress == null)
            {
                Debug.LogError("无进行中阶段");
                return;
            }

            var progress = saveData.currentProgress;
            var mainGroup = EnsureMainGroupExists(progress.mainId);
            var stageData = mainGroup?.GetStageFile(progress.stageIndex);

            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {progress.mainId}, {progress.stageIndex}");
                return;
            }

            // var range = stageFileRanges[saveData.currentProgress.stageIndex];
            if (fileNumber < stageData.stageStart || fileNumber > stageData.stageEnd)
            {
                Debug.LogError($"文件编号 {fileNumber} 不在当前阶段范围 {stageData.stageStart}-{stageData.stageEnd}");
                return;
            }

            // 【新增】如果设置不同的图片，记录当前图片为上一张
            if (progress.currentFileIndex != -1 && progress.currentFileIndex != fileNumber)
            {
                SetPreviousImage(progress.mainId, progress.currentFileIndex);
            }

            // int relativeIndex = fileNumber - range[0];
            // saveData.currentProgress.currentFileIndex = relativeIndex;
            saveData.currentProgress.currentFileIndex = fileNumber;
            SaveData();
        }

        /// <summary>
        /// 获取当前阶段正在播放的图片编号
        /// </summary>
        public int GetCurrentImageInCurrentStage()
        {
            if (saveData.currentProgress == null)
            {
                Debug.LogError("无进行中阶段");
                return -1;
            }

            if (saveData.currentProgress.currentFileIndex < 0)
            {
                return -1;
            }
            return saveData.currentProgress.currentFileIndex;
            // var range = stageFileRanges[saveData.currentProgress.stageIndex];
            // return range[0] + saveData.currentProgress.currentFileIndex;
        }

        /// <summary>
        /// 获取当前阶段的进度信息（已使用/总数）
        /// </summary>
        /// <returns>返回(已使用数量, 总数量)</returns>
        public (int usedCount, int totalCount) GetCurrentStageProgress()
        {
            if (saveData.currentProgress == null)
            {
                Debug.LogWarning("没有进行中的阶段");
                return (0, 0);
            }

            var mainId = saveData.currentProgress.mainId;
            var stageIdx = saveData.currentProgress.stageIndex;

            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup == null)
            {
                Debug.LogError($"找不到主编号数据: {mainId}");
                return (0, 0);
            }

            var stageData = mainGroup.GetStageFile(stageIdx);
            if (stageData == null)
            {
                Debug.LogError($"找不到阶段数据: {mainId}, {stageIdx}");
                return (0, 0);
            }

            return (stageData.usedFiles.Count, 4);
        }

        // 外部可选：获取当前进行阶段信息
        public (string mainId, int stageIndex)? GetCurrentProgress()
        {
            if (saveData.currentProgress == null) return null;
            return (saveData.currentProgress.mainId, saveData.currentProgress.stageIndex);
        }

        // 获取某个主编号某个阶段的已使用图片数量
        public int GetUsedImageCountInStage(string mainId, int stageIndex)
        {
            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup == null) return 0;

            var stageData = mainGroup.GetStageFile(stageIndex);
            return stageData?.usedFiles.Count ?? 0;
        }

        // 检查某个主编号是否已完成所有阶段
        public bool IsMainIdCompleted(string mainId)
        {
            if (string.IsNullOrEmpty(mainId)) return true;

            var mainGroup = EnsureMainGroupExists(mainId);
            if (mainGroup == null) return false;
            return mainGroup.usedStages.Count >= 4;
        }

        // 获取某个mainId下所有未使用的阶段及其图片
        public Dictionary<int, List<int>> GetAllUnusedStagesAndImages(string mainId)
        {
            var result = new Dictionary<int, List<int>>();
            var mainGroup = saveData.GetMainGroup(mainId);
            if (mainGroup == null) return result;

            int curentStageIdx = -1;
            var curProgress = saveData.currentProgress;
            if (curProgress != null && curProgress.mainId == mainId)
            {
                curentStageIdx = curProgress.stageIndex;
            }

            // 这里用4是因为如果没取到是直接跳过的
            for (int stageIdx = 0; stageIdx < 4; stageIdx++)
            {
                if (!mainGroup.usedStages.Contains(stageIdx) && curentStageIdx != stageIdx)
                {
                    var stageData = mainGroup.GetStageFile(stageIdx);
                    if (stageData == null) continue;

                    var remainingImages = new List<int>();
                    for (int fileNum = stageData.stageStart; fileNum <= stageData.stageEnd; fileNum++)
                    {
                        if (!stageData.usedFiles.Contains(fileNum))
                        {
                            remainingImages.Add(fileNum);
                        }
                    }

                    result[stageIdx] = remainingImages;
                }
            }

            return result;
        }

        public class CurrentFinishPicData
        {
            public string mainId;
            public int fileId;
            public bool isSpecial;
            public int startIdx;
            public int endIdx;
            public bool currentStageIsFinish;
        }


        #region 获取所有主编号的完成信息
        /// <summary>
        /// 阶段完成信息
        /// </summary>
        [System.Serializable]
        public class StageCompletionInfo
        {
            public int stageIndex;
            public List<int> completedFileIds = new List<int>();
        }

        /// <summary>
        /// 主编号完成信息
        /// </summary>
        [System.Serializable]
        public class MainIdCompletionInfo
        {
            public string mainId;
            public int totalCount;
            public int completedCount;
            public List<StageCompletionInfo> stages = new List<StageCompletionInfo>();
        }
        /// <summary>
        /// 获取所有主编号的完成信息
        /// </summary>
        public List<MainIdCompletionInfo> GetAllMainIdCompletionInfo()
        {
            var result = new List<MainIdCompletionInfo>();

            if (saveData == null || saveData.mainGroups == null)
            {
                Debug.LogWarning("没有数据或数据为空");
                return result;
            }

            // 获取当前进行中的图片信息（如果有）
            string currentMainId = null;
            int currentFileIndex = -1;
            if (saveData.currentProgress != null)
            {
                currentMainId = saveData.currentProgress.mainId;
                currentFileIndex = saveData.currentProgress.currentFileIndex;
            }

            foreach (var mainGroup in saveData.mainGroups)
            {
                var info = new MainIdCompletionInfo
                {
                    mainId = mainGroup.mainId,
                    totalCount = 0,
                    completedCount = 0,
                    stages = new List<StageCompletionInfo>()
                };

                foreach (var stageData in mainGroup.stageFiles)
                {
                    // 计算该阶段的总文件数
                    int stageTotal = stageData.stageEnd - stageData.stageStart + 1;
                    info.totalCount += stageTotal;

                    // 获取该阶段已完成的文件列表（排除进行中的图片）
                    var completedFiles = new List<int>();
                    foreach (var fileId in stageData.usedFiles)
                    {
                        // 如果是当前进行中的图片，不计入完成列表
                        if (currentMainId == mainGroup.mainId && currentFileIndex == fileId)
                        {
                            continue;
                        }
                        completedFiles.Add(fileId);
                    }

                    // 如果有完成的文件，添加到阶段信息中
                    if (completedFiles.Count > 0)
                    {
                        info.completedCount += completedFiles.Count;

                        var stageInfo = new StageCompletionInfo
                        {
                            stageIndex = stageData.stageIndex,
                            completedFileIds = completedFiles
                        };
                        stageInfo.completedFileIds.Sort();
                        info.stages.Add(stageInfo);
                    }
                }

                // 只有有完成图片的主编号才添加到结果中
                if (info.completedCount > 0)
                {
                    // 按阶段索引排序
                    info.stages = info.stages.OrderBy(s => s.stageIndex).ToList();
                    result.Add(info);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取指定主编号的完成信息
        /// </summary>
        public MainIdCompletionInfo GetMainIdCompletionInfo(string mainId)
        {
            if (string.IsNullOrEmpty(mainId))
            {
                Debug.LogWarning("mainId不能为空");
                return null;
            }

            var mainGroup = saveData?.GetMainGroup(mainId);
            if (mainGroup == null)
            {
                Debug.LogWarning($"未找到主编号数据: {mainId}");
                return null;
            }

            // 获取当前进行中的图片信息（如果有）
            string currentMainId = null;
            int currentFileIndex = -1;
            if (saveData.currentProgress != null && saveData.currentProgress.mainId == mainId)
            {
                currentMainId = saveData.currentProgress.mainId;
                currentFileIndex = saveData.currentProgress.currentFileIndex;
            }

            var info = new MainIdCompletionInfo
            {
                mainId = mainId,
                totalCount = 0,
                completedCount = 0,
                stages = new List<StageCompletionInfo>()
            };

            foreach (var stageData in mainGroup.stageFiles)
            {
                int stageTotal = stageData.stageEnd - stageData.stageStart + 1;
                info.totalCount += stageTotal;

                // 获取该阶段已完成的文件列表（排除进行中的图片）
                var completedFiles = new List<int>();
                foreach (var fileId in stageData.usedFiles)
                {
                    // 如果是当前进行中的图片，不计入完成列表
                    if (currentMainId == mainId && currentFileIndex == fileId)
                    {
                        continue;
                    }
                    completedFiles.Add(fileId);
                }

                if (completedFiles.Count > 0)
                {
                    info.completedCount += completedFiles.Count;

                    var stageInfo = new StageCompletionInfo
                    {
                        stageIndex = stageData.stageIndex,
                        completedFileIds = completedFiles
                    };
                    stageInfo.completedFileIds.Sort();
                    info.stages.Add(stageInfo);
                }
            }

            info.stages = info.stages.OrderBy(s => s.stageIndex).ToList();
            return info;
        }

        /// <summary>
        /// 获取指定主编号下已完成的所有图片
        /// </summary>
        /// <param name="mainId">主编号</param>
        /// <returns>返回该主编号下已完成的所有fileId列表</returns>
        public List<int> GetCompletedImagesByMainId(string mainId)
        {
            if (string.IsNullOrEmpty(mainId))
            {
                Debug.LogWarning("mainId不能为空");
                return new List<int>();
            }

            var mainGroup = saveData?.GetMainGroup(mainId);
            if (mainGroup == null)
            {
                Debug.LogWarning($"未找到主编号数据: {mainId}");
                return new List<int>();
            }

            var completedFileIds = new List<int>();

            // 遍历所有阶段，直接收集已使用的文件
            foreach (var stageData in mainGroup.stageFiles)
            {
                foreach (var fileId in stageData.usedFiles)
                {
                    completedFileIds.Add(fileId);
                }
            }

            completedFileIds.Sort();
            return completedFileIds;
        }

        #endregion
        // 重置所有数据（测试用）
        public void ResetAllData()
        {
            InitNewData();
            SaveData();
        }

        // 检查是否是第一次（用于外部判断）
        public bool IsFirstTime()
        {
            return saveData.isFirstTime;
        }


        /// <summary>
        /// 测试函数：直接在函数内部配置测试数据
        /// </summary>
        public void TestCompleteFiles()
        {
#if UNITY_EDITOR
            // ===== 在这里配置测试数据 =====
            var testData = new List<(string mainId, List<int> fileIds)>
            {
                ("60001060", new List<int> { 13, 14, 15,16 }),
                ("60001060", new List<int> { 9, 10, 11 }),
                ("60001540", new List<int> { 9, 10, 11 }),
                ("60002040", new List<int> { 5, 6, 7, 8 }),
                // 可以继续添加更多...
                // ("60002420", new List<int> { 1, 2, 3, 4 }),
            };
            // ==============================

            if (testData == null || testData.Count == 0)
            {
                Debug.LogWarning("测试数据为空");
                return;
            }

            Debug.Log($"========== 开始测试 ==========");

            foreach (var (mainId, fileIds) in testData)
            {
                if (string.IsNullOrEmpty(mainId) || fileIds == null || fileIds.Count == 0)
                    continue;

                Debug.Log($"处理主编号: {mainId}, 文件: {string.Join(", ", fileIds)}");

                var mainGroup = EnsureMainGroupExists(mainId);
                if (mainGroup == null)
                {
                    Debug.LogError($"无法创建主编号数据: {mainId}");
                    continue;
                }

                foreach (var fileId in fileIds)
                {
                    // 查找文件所属的阶段
                    int? stageIndex = null;
                    foreach (var stageDatadd in mainGroup.stageFiles)
                    {
                        if (fileId >= stageDatadd.stageStart && fileId <= stageDatadd.stageEnd)
                        {
                            stageIndex = stageDatadd.stageIndex;
                            break;
                        }
                    }

                    if (!stageIndex.HasValue)
                    {
                        Debug.LogError($"找不到文件 {fileId} 所属的阶段");
                        continue;
                    }

                    var stageData = mainGroup.GetStageFile(stageIndex.Value);
                    if (stageData == null)
                        continue;

                    if (!stageData.usedFiles.Contains(fileId))
                    {
                        stageData.usedFiles.Add(fileId);
                        stageData.usedFiles.Sort();
                        Debug.Log($"  完成文件: {mainId}{fileId:D2}");
                    }

                    // 检查阶段是否全部完成
                    if (stageData.usedFiles.Count == 4 && !mainGroup.usedStages.Contains(stageIndex.Value))
                    {
                        mainGroup.usedStages.Add(stageIndex.Value);
                        mainGroup.usedStages.Sort();
                        Debug.Log($"  阶段 {stageIndex.Value + 1} 全部完成！");
                    }
                }
            }

            SaveData();
            Debug.Log("========== 测试完成 ==========");
#endif
        }




    }

}