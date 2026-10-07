using System;
using System.Collections;
using System.Collections.Generic;
using GameBase;
using GameLogic;
using UnityEngine;
using Watermelon;

// 我需要做一个monthly task 管理功能，主要是记录玩家连胜次数，
// 1. 总连胜，每30天重置数据,只需要跨天检测，如果失败，就需要减去当前关卡的胜利，第一个胜利不算，以第二个胜利算一连胜关卡，然后累计连胜的总关卡，计算任务列表完成数量，记录奖励领取的最大胜利次数
// 2. 5心连胜，连胜获取心，心用来做当日任务 每天重置，连胜失败需要重置，第一个胜利不算，以第二个胜利算一连胜，然后获取一颗心，二连胜完成关卡获取2颗心，最高5连胜，完成关卡获取5颗心，统计每天获取的心数量。需要记录当天任务是否已经领取奖励
// 3. 数据存储类MonthlySave。 var saveData = SaveController.GetSaveObject<MonthlySave>("MonthlySaveData");
// 4. DateTime currentLocalTime =  NetTimeMgr.Instance.CurTime;// 获取当前时间。 NetTimeMgr.Instance.CurTime.onEnterNextDay += (DateTime time)=>{}//跨天监听
public class MonthlyCtrl : Singleton<MonthlyCtrl>
{
    public class MonthlySave : ISaveObject
    {
        #region 总连胜用来完成任务列表
        /// <summary>
        /// 完成没失败的次数
        /// </summary>
        [SerializeField] public int monthFinishCount = 0;
        [SerializeField] public long unixTimeMonthlyStart = 0;
        /// <summary>
        /// 当前连胜数（用于计算连续通关）
        /// </summary>
        [SerializeField] public int currentWinStreak = 0;
        /// <summary>
        /// 记录奖励领取的最大胜利次数
        /// </summary>
        [SerializeField] public int maxRewardClaimedCount_free = 0;
        [SerializeField] public int maxRewardClaimedCount_Prem = 0;
        #endregion

        #region 5心连胜
        /// <summary>
        /// 连胜 1-5 的心
        /// </summary>
        [SerializeField] public int winHeartFullCount = 0;
        /// <summary>
        /// 今天完成多少心
        /// </summary>
        [SerializeField] public int hartGetCount = 0;
        /// <summary>
        /// 今日任务是否已领取奖励
        /// </summary>
        [SerializeField] public bool todayRewardClaimed = false;
        /// <summary>
        /// 上次心跳日期（用于跨天检测）
        /// </summary>
        [SerializeField] public string lastHeartDate = "";
        [SerializeField] public int nowPlayingHeartTaskIndex = -1;
        #endregion

        #region 是否购买了
        [SerializeField] public bool IAP_BuyPremium = false;
        #endregion

        public void Flush()
        {
        }
        /// <summary>
        /// 重置月度数据
        /// </summary>
        public void ResetMonthlyData()
        {
            monthFinishCount = 0;
            unixTimeMonthlyStart = 0;
            currentWinStreak = 0;
            maxRewardClaimedCount_free = 0;
            maxRewardClaimedCount_Prem = 0;
        }

        /// <summary>
        /// 重置每日心数据
        /// </summary>
        public void ResetDailyHeartData()
        {
            winHeartFullCount = 0;
            hartGetCount = 0;
            todayRewardClaimed = false;
        }
    }
    /****************************************************************************************/

    public enum FiveWinHeart
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
    }



#if UNITY_EDITOR
    public const int StartMonthlyLevel = 1;
#else
    public const int StartMonthlyLevel = 3;
#endif

    private MonthlySave saveData;
    private bool isInit = false;

    public bool GetIAP_BuyPremium()
    {
        return saveData.IAP_BuyPremium;
    }
    public void SetIAP_BuyPremium(bool isBuy)
    {
        saveData.IAP_BuyPremium = isBuy;
    }

    protected override void OnInit()
    {
        base.OnInit();
        isInit = false;

        // 获取存档数据
        saveData = SaveController.GetSaveObject<MonthlySave>("MonthlySaveData");
        if (IsStartMonthlyTask())
        {
            InitMonthly();
        }
    }

    private void InitMonthly()
    {
        if (isInit) return;
        isInit = true;
        // 检查月度重置
        CheckMonthlyReset();

        // 检查每日重置
        CheckDailyReset();

        // 监听跨天事件
        NetTimeMgr.Instance.onEnterNextDay += OnEnterNextDay;
    }

    /// <summary>
    /// 跨天处理
    /// </summary>
    private void OnEnterNextDay(DateTime time)
    {
        CheckMonthlyReset();
        CheckDailyReset();
    }

    /// <summary>
    /// 检查月度重置（30天）
    /// </summary>
    private void CheckMonthlyReset()
    {
        DateTime currentTime = NetTimeMgr.Instance.CurTime;

        if (saveData.unixTimeMonthlyStart == 0)
        {
            // 首次初始化
            saveData.unixTimeMonthlyStart = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            return;
        }

        DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(saveData.unixTimeMonthlyStart).DateTime;
        TimeSpan elapsed = currentTime - startTime;

        if (elapsed.TotalDays >= 30)
        {
            // 30天已过，重置月度数据
            saveData.ResetMonthlyData();
            saveData.unixTimeMonthlyStart = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            // 触发月度重置事件（如果需要）
            OnMonthlyReset();
        }
    }

    /// <summary>
    /// 检查每日重置
    /// </summary>
    private void CheckDailyReset()
    {
        DateTime currentTime = NetTimeMgr.Instance.CurTime;
        string currentDate = currentTime.ToString("yyyy-MM-dd");

        if (saveData.lastHeartDate != currentDate)
        {
            // 新的一天，重置心的数据
            saveData.ResetDailyHeartData();
            ResetDailyHeartIndex();
            saveData.lastHeartDate = currentDate;
        }
    }

    private void ResetDailyHeartIndex()
    {
        int nowIdx = saveData.nowPlayingHeartTaskIndex;
        nowIdx++;

        var dailyTaskList = MonthlyDataList.Data.FiveHeartDailyTask;
        saveData.nowPlayingHeartTaskIndex = nowIdx % dailyTaskList.Count;
    }
    /// <summary>
    /// 关卡改变检测是否可以开启monthly task
    /// </summary>
    public void ToCheckShouldInitMonthly()
    {
        if (IsStartMonthlyTask())
        {
            InitMonthly();
        }
    }

    public bool IsStartMonthlyTask()
    {
        ActiveSession activeSession = ActiveSession.Current;
        if (activeSession.DisplayLevelIndex >= StartMonthlyLevel)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 处理关卡胜利
    /// </summary>
    /// <param name="levelId">关卡ID</param>
    public void OnLevelWin()//int levelId)
    {
        if (!IsStartMonthlyTask()) return;
        ToCheckShouldInitMonthly();

        DateTime currentTime = NetTimeMgr.Instance.CurTime;

        // 检查是否需要重置
        CheckMonthlyReset();
        CheckDailyReset();

        // 增加连胜计数
        saveData.currentWinStreak++;

        // 5心连胜处理
        UpdateHeartSystem();

        // 总连胜处理（以第二个胜利算一连胜）
        if (saveData.currentWinStreak >= 2)
        {
            // saveData.monthFinishCount++;
            // 修改成每次叠加5心连胜
            int heartCount = Mathf.Min(saveData.winHeartFullCount, 5);
            saveData.monthFinishCount += heartCount;
        }

        OnWinStreakChanged();
    }

    /// <summary>
    /// 处理关卡失败
    /// </summary>
    /// <param name="levelId">关卡ID</param>
    public void OnLevelFail()//int levelId)
    {
        if (!IsStartMonthlyTask()) return;
        DateTime currentTime = NetTimeMgr.Instance.CurTime;

        // 检查是否需要重置
        CheckMonthlyReset();
        CheckDailyReset();

        // 失败时减去当前关卡的胜利（第一个胜利不算）
        if (saveData.currentWinStreak > 0)
        {
            saveData.currentWinStreak--;

            // 如果之前有连胜记录，需要从总连胜中减去
            if (saveData.currentWinStreak >= 1 && saveData.monthFinishCount > 0)
            {
                saveData.monthFinishCount--;
            }
        }

        // 重置连胜（因为失败了）
        saveData.currentWinStreak = 0;

        // 重置心的连胜
        saveData.winHeartFullCount = 0;

        // 触发失败事件
        OnWinStreakBroken();
    }

    /// <summary>
    /// 更新心系统
    /// </summary>
    private void UpdateHeartSystem()
    {
        // 第二个胜利才开始算心
        if (saveData.currentWinStreak >= 2)
        {
            // 计算应该获得的心数（最高5连胜）
            // int heartCount = Mathf.Min(saveData.currentWinStreak - 1, 5);

            // 更新连胜心计数
            saveData.winHeartFullCount += 1;
            int heartCount = Mathf.Min(saveData.winHeartFullCount, 5);
            // 更新今日获得的总心数
            saveData.hartGetCount += heartCount;
        }
    }

    /// <summary>
    /// 领取任务列表奖励
    /// </summary>
    /// <param name="requiredCount">所需完成次数</param>
    /// <returns>是否可以领取</returns>
    public bool ClaimTaskReward(int requiredCount)
    {
        if (saveData.monthFinishCount >= requiredCount &&
            requiredCount > saveData.maxRewardClaimedCount_free)
        {
            saveData.maxRewardClaimedCount_free = requiredCount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 领取任务列表奖励
    /// </summary>
    /// <param name="requiredCount">所需完成次数</param>
    /// <returns>是否可以领取</returns>
    public bool ClaimTaskReward_Prem(int requiredCount)
    {
        if (saveData.monthFinishCount >= requiredCount &&
            requiredCount > saveData.maxRewardClaimedCount_Prem)
        {
            saveData.maxRewardClaimedCount_Prem = requiredCount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查指定奖励是否已领取
    /// </summary>
    /// <param name="requiredCount">所需完成次数</param>
    /// <returns>是否已领取</returns>
    public bool IsTaskRewardClaimed_Free(int requiredCount)
    {
        return saveData.maxRewardClaimedCount_free >= requiredCount;
    }
    /// <summary>
    /// 检查指定奖励是否已领取
    /// </summary>
    /// <param name="requiredCount">所需完成次数</param>
    /// <returns>是否已领取</returns>
    public bool IsTaskRewardClaimed_Prem(int requiredCount)
    {
        return saveData.maxRewardClaimedCount_Prem >= requiredCount;
    }

    /// <summary>
    /// 检查是否可以领取指定奖励
    /// </summary>
    /// <param name="requiredCount">所需完成次数</param>
    /// <returns>是否可以领取</returns>
    public bool CanClaimTaskReward_free(int requiredCount)
    {
        return saveData.monthFinishCount >= requiredCount &&
               requiredCount > saveData.maxRewardClaimedCount_free;
    }
    /// <summary>
    /// 检查是否可以领取指定奖励
    /// </summary>
    /// <param name="requiredCount">所需完成次数</param>
    /// <returns>是否可以领取</returns>
    public bool CanClaimTaskReward_Prem(int requiredCount)
    {
        return saveData.monthFinishCount >= requiredCount &&
               requiredCount > saveData.maxRewardClaimedCount_Prem;
    }
    /// <summary>
    /// 获取任务列表完成数量
    /// </summary>
    public int GetTaskCompleteCount()
    {
        return saveData.monthFinishCount;
    }


    /// <summary>
    /// 领取每日心任务奖励
    /// </summary>
    /// <returns>是否可以领取</returns>
    public bool ClaimDailyHeartReward()
    {
        if (!saveData.todayRewardClaimed && saveData.hartGetCount > 0)
        {
            saveData.todayRewardClaimed = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取当前连胜数
    /// </summary>
    public int GetCurrentWinStreak()
    {
        return saveData.currentWinStreak;
    }

    /// <summary>
    /// 获取今日获得的心数
    /// </summary>
    public int GetTodayHeartCount()
    {
        return saveData.hartGetCount;
    }

    /// <summary>
    /// 获取当前心连胜等级
    /// </summary>
    public int GetCurrentHeartLevel()
    {
        return saveData.winHeartFullCount;
    }

    /// <summary>
    /// 今日奖励是否已领取
    /// </summary>
    public bool IsTodayRewardClaimed()
    {
        return saveData.todayRewardClaimed;
    }

    public int GetCurrentHeartPlayingIndex()
    {
        return saveData.nowPlayingHeartTaskIndex;
    }

    // /// <summary>
    // /// 获取剩余月度天数
    // /// </summary>
    // public int GetRemainingMonthlyDays()
    // {
    //     if (saveData.unixTimeMonthlyStart == 0) return 30;

    //     DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(saveData.unixTimeMonthlyStart).DateTime;
    //     DateTime currentTime = NetTimeMgr.Instance.CurTime;
    //     int daysPassed = (int)(currentTime - startTime).TotalDays;
    //     return Mathf.Max(0, 30 - daysPassed);
    // }

    /// <summary>
    /// 获取月度任务剩余时间结构体
    /// </summary>
    public struct MonthlyRemainingTime
    {
        public int days;
        public int hours;
        public int minutes;

        public override string ToString()
        {
            return $"{days}d{hours}h{minutes}m";
        }
    }

    /// <summary>
    /// 获取月度任务剩余时间（天、时、分）
    /// </summary>
    /// <returns>剩余时间结构体</returns>
    public MonthlyRemainingTime GetMonthlyRemainingTime()
    {
        MonthlyRemainingTime remainingTime = new MonthlyRemainingTime();

        if (saveData.unixTimeMonthlyStart == 0)
        {
            // 尚未开始，返回完整的30天
            remainingTime.days = 30;
            remainingTime.hours = 0;
            remainingTime.minutes = 0;
            return remainingTime;
        }

        DateTime currentTime = NetTimeMgr.Instance.CurTime;
        DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(saveData.unixTimeMonthlyStart).DateTime;

        // 计算30天后的结束时间
        DateTime endTime = startTime.AddDays(30);

        // 计算剩余时间
        TimeSpan remaining = endTime - currentTime;

        if (remaining.TotalSeconds <= 0)
        {
            // 已经过期
            remainingTime.days = 0;
            remainingTime.hours = 0;
            remainingTime.minutes = 0;
            return remainingTime;
        }

        // 提取天、时、分
        remainingTime.days = remaining.Days;
        remainingTime.hours = remaining.Hours;
        remainingTime.minutes = remaining.Minutes;

        return remainingTime;
    }

    // /// <summary>
    // /// 获取月度任务剩余总分钟数
    // /// </summary>
    // /// <returns>剩余总分钟数</returns>
    // public int GetMonthlyRemainingTotalMinutes()
    // {
    //     MonthlyRemainingTime remaining = GetMonthlyRemainingTime();
    //     return remaining.days * 24 * 60 + remaining.hours * 60 + remaining.minutes;
    // }

    // /// <summary>
    // /// 获取月度任务剩余总秒数
    // /// </summary>
    // /// <returns>剩余总秒数</returns>
    // public double GetMonthlyRemainingTotalSeconds()
    // {
    //     if (saveData.unixTimeMonthlyStart == 0)
    //     {
    //         return 30 * 24 * 60 * 60; // 30天的秒数
    //     }

    //     DateTime currentTime = NetTimeMgr.Instance.CurTime;
    //     DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(saveData.unixTimeMonthlyStart).DateTime;
    //     DateTime endTime = startTime.AddDays(30);

    //     TimeSpan remaining = endTime - currentTime;
    //     return Math.Max(0, remaining.TotalSeconds);
    // }

    /// <summary>
    /// 获取格式化的剩余时间字符串
    /// </summary>
    /// <param name="format">格式化模板，例如："剩余{0}天{1}小时{2}分钟"</param>
    /// <returns>格式化后的时间字符串</returns>
    public string GetFormattedRemainingTime(string format = "{0}d{1}h{2}m")
    {
        MonthlyRemainingTime remaining = GetMonthlyRemainingTime();
        return string.Format(format, remaining.days, remaining.hours, remaining.minutes);
    }

    #region 事件回调（可根据需要实现）
    private void OnMonthlyReset()
    {
        // 月度重置时的回调
    }

    private void OnWinStreakChanged()
    {
        // 连胜变化时的回调
    }

    private void OnWinStreakBroken()
    {
        // 连胜中断时的回调
    }
    #endregion
}
