using System;
using System.Collections;
using UnityEngine;

namespace Watermelon
{
    [StaticUnload]
    public static class SevenDaySignInController
    {
        public const int DAYS_COUNT = 7;

        private const string SAVE_ID = "Seven Day Sign In";
        public const string VIDEO_REWARD_SAVE_ID = "Seven Day Sign In Video Rewards";
        // Network time is now provided by NetTimeMgr / GameTimeMgr. No direct network requests here.
        private const double MISSED_DAY_RESET_THRESHOLD = 1.01d;
        // private static readonly int[] COIN_REWARD_AMOUNTS = { 50, 50, 100, 100, 100, 100 };

        private static SevenDaySignInSave save;
        private static SimpleIntSave videoRewardSave;
        private static bool isRefreshing;
        private static bool hasNetworkTime;
        private static long currentNetworkUnixTime;
        private static DateTime currentNetworkDate;
        private static SevenDaySignInState currentState;

        public static SevenDaySignInState CurrentState => currentState ?? BuildState(true);

        public static IEnumerator RefreshStateCoroutine(Action<SevenDaySignInState> onCompleted = null)
        {
            EnsureSave();

            if (isRefreshing)
            {
                while (isRefreshing)
                {
                    yield return null;
                }

                onCompleted?.Invoke(CurrentState);
                yield break;
            }

            isRefreshing = true;
            currentState = BuildState(true);

            // Use NetTimeMgr if available; otherwise fall back to GameTimeMgr. Do NOT request network time here.
            bool timeAvailable = false;
            DateTime netDate = DateTime.MinValue;
            long netUnixSeconds = 0;

            // Prefer NetTimeMgr (has millisecond precision internally)
            try
            {

                if (GameBase.NetTimeMgr.Instance.HasGetNetTime)
                {
                    timeAvailable = true;
                    DateTime cur = GameBase.NetTimeMgr.Instance.CurTime;
                    netDate = cur.Date;
                    netUnixSeconds = new DateTimeOffset(cur.ToUniversalTime()).ToUnixTimeSeconds();
                }
            }
            catch
            {
                // ignore if NetTimeMgr type/instance not available
            }

            // No fallback: only use NetTimeMgr as source of network time.

            if (timeAvailable)
            {
                hasNetworkTime = true;
                currentNetworkUnixTime = netUnixSeconds;
                currentNetworkDate = netDate;

                ApplyDailyRefresh(currentNetworkDate);

                save.LastSyncedUnixTime = currentNetworkUnixTime;

                // Do not update any other time managers here; NetTimeMgr is authoritative.
                SaveController.MarkAsSaveIsRequired();
            }

            currentState = BuildState(false);
            isRefreshing = false;

            onCompleted?.Invoke(currentState);
        }

        public static bool TryClaimToday(out SevenDaySignInState state, out SevenDaySignInClaimResult claimResult)
        {
            EnsureSave();

            claimResult = null;

            if (!hasNetworkTime)
            {
                state = BuildState(false);
                return false;
            }

            ApplyDailyRefresh(currentNetworkDate);

            if (HasClaimedToday(currentNetworkDate))
            {
                state = BuildState(false);
                return false;
            }

            int claimedDayNumber = save.ClaimedDaysInCycle + 1;
            if (save.ClaimedDaysInCycle >= DAYS_COUNT)
            {
                save.ClaimedDaysInCycle = 0;
                claimedDayNumber = 1;
            }
            int nowClaimeIdx = save.ClaimedDaysInCycle;
            save.ClaimedDaysInCycle++;
            save.LastClaimDateBinary = currentNetworkDate.ToBinary();
            save.AutoPopupShownForCurrentDay = true;
            save.LastSyncedUnixTime = currentNetworkUnixTime;

            claimResult = GrantRewardForDay(claimedDayNumber, nowClaimeIdx);

            SaveController.MarkAsSaveIsRequired();

            currentState = BuildState(false);
            state = currentState;

            return true;
        }

        public static int GetVideoRewardCount()
        {
            EnsureSave();
            return videoRewardSave.Value;
        }

        public static bool ShouldAutoShowPopup()
        {
            SevenDaySignInState state = CurrentState;
            return state.HasValidNetworkTime && state.CanClaimToday && state.ShouldAutoShowToday;
        }

        public static bool ConsumeInitialAutoPopupSkip()
        {
            EnsureSave();

            if (save.InitialAutoPopupSkipped)
                return false;

            bool isBrandNewPlayer = save.ClaimedDaysInCycle == 0 &&
                                    save.LastClaimDateBinary == 0 &&
                                    save.LastProcessedDateBinary == 0;

            save.InitialAutoPopupSkipped = true;
            SaveController.MarkAsSaveIsRequired();

            currentState = BuildState(false);

            return isBrandNewPlayer;
        }

        public static void MarkAutoPopupShown()
        {
            EnsureSave();

            save.AutoPopupShownForCurrentDay = true;
            SaveController.MarkAsSaveIsRequired();

            currentState = BuildState(false);
        }

        private static void EnsureSave()
        {
            if (save == null)
            {
                save = SaveController.GetSaveObject<SevenDaySignInSave>(SAVE_ID);
            }

            if (videoRewardSave == null)
            {
                videoRewardSave = SaveController.GetSaveObject<SimpleIntSave>(VIDEO_REWARD_SAVE_ID);
            }
        }

        private static SevenDaySignInClaimResult GrantRewardForDay(int dayNumber, int idx)
        {
            // Day 7 should follow the same reward configuration as other days, instead of
            // being treated as a special video reward.
            return new SevenDaySignInClaimResult
            {
                ClaimedDayNumber = dayNumber,
                IsVideoReward = false,
                DayIdx = idx,
            };
        }

        // public static int GetCoinsRewardAmount(int dayNumber)
        // {
        //     int index = dayNumber - 1;
        //     if (index < 0 || index >= COIN_REWARD_AMOUNTS.Length)
        //         return 0;

        //     return COIN_REWARD_AMOUNTS[index];
        // }

        private static void ApplyDailyRefresh(DateTime currentDate)
        {
            DateTime processedDate = GetStoredDate(save.LastProcessedDateBinary);
            if (processedDate == currentDate)
                return;

            bool hasProcessedDay = processedDate != DateTime.MinValue;

            DateTime lastClaimDate = GetStoredDate(save.LastClaimDateBinary);
            if (lastClaimDate != DateTime.MinValue)
            {
                TimeSpan dayGap = currentDate - lastClaimDate;
                if (dayGap.TotalDays > MISSED_DAY_RESET_THRESHOLD)
                {
                    save.ClaimedDaysInCycle = 0;
                }
            }

            if (save.ClaimedDaysInCycle >= DAYS_COUNT)
            {
                save.ClaimedDaysInCycle = 0;
            }

            save.LastProcessedDateBinary = currentDate.ToBinary();
            if (hasProcessedDay)
            {
                save.AutoPopupShownForCurrentDay = false;
            }
        }

        private static bool HasClaimedToday(DateTime currentDate)
        {
            DateTime lastClaimDate = GetStoredDate(save.LastClaimDateBinary);
            return lastClaimDate == currentDate;
        }

        private static SevenDaySignInState BuildState(bool isLoading)
        {
            EnsureSave();

            DateTime activeDate = hasNetworkTime ? currentNetworkDate : GetStoredDate(save.LastProcessedDateBinary);
            bool hasActiveDate = activeDate != DateTime.MinValue;
            bool hasClaimedToday = hasActiveDate && HasClaimedToday(activeDate);
            int claimedDays = Mathf.Clamp(save.ClaimedDaysInCycle, 0, DAYS_COUNT);
            bool cycleCompletedToday = hasClaimedToday && claimedDays >= DAYS_COUNT;
            bool canClaimToday = hasNetworkTime && hasActiveDate && !hasClaimedToday && claimedDays < DAYS_COUNT;

            int claimableDayNumber = -1;
            if (canClaimToday)
            {
                claimableDayNumber = claimedDays + 1;
            }

            string statusMessage;
            if (isLoading)
            {
                statusMessage = "Syncing network time...";
            }
            else if (!hasNetworkTime)
            {
                statusMessage = "Network required to claim today's sign-in.";
            }
            else if (cycleCompletedToday)
            {
                statusMessage = "Day 7 claimed. A new cycle will start tomorrow.";
            }
            else if (hasClaimedToday)
            {
                statusMessage = "Today's sign-in has already been claimed.";
            }
            else
            {
                statusMessage = $"Day {claimableDayNumber} is ready to claim.";
            }

            return new SevenDaySignInState
            {
                IsLoading = isLoading,
                HasValidNetworkTime = hasNetworkTime,
                CanClaimToday = canClaimToday,
                HasClaimedToday = hasClaimedToday,
                ShouldAutoShowToday = !save.AutoPopupShownForCurrentDay,
                ClaimedDaysCount = claimedDays,
                ClaimableDayNumber = claimableDayNumber,
                CycleCompletedToday = cycleCompletedToday,
                CurrentDate = activeDate,
                StatusMessage = statusMessage,
            };
        }

        private static DateTime GetStoredDate(long dateBinary)
        {
            if (dateBinary == 0)
                return DateTime.MinValue;

            return DateTime.FromBinary(dateBinary).Date;
        }

        // Network requests for time are handled by NetTimeMgr; no direct network coroutine here anymore.

        private static void UnloadStatic()
        {
            save = null;
            videoRewardSave = null;
            isRefreshing = false;
            hasNetworkTime = false;
            currentNetworkUnixTime = 0;
            currentNetworkDate = DateTime.MinValue;
            currentState = null;
        }
    }

    public class SevenDaySignInState
    {
        public bool IsLoading;
        public bool HasValidNetworkTime;
        public bool CanClaimToday;
        public bool HasClaimedToday;
        public bool ShouldAutoShowToday;
        public bool CycleCompletedToday;
        public int ClaimedDaysCount;
        public int ClaimableDayNumber;
        public DateTime CurrentDate;
        public string StatusMessage;
    }

    public class SevenDaySignInClaimResult
    {
        public int ClaimedDayNumber;
        public bool IsVideoReward;
        // public int CoinsAmount;
        public int TotalVideoRewardCount;
        public string RewardMessage;
        public int DayIdx;
    }
}
