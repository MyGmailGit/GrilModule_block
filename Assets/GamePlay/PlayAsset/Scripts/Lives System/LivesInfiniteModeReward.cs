using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    [RegisterReward(typeof(LivesInfiniteModeRewardView))]
    public sealed class LivesInfiniteModeReward : Reward
    {
        private const int PREVIEW_SORTING_ORDER = 0;

        [SerializeField] float durationInMinutes = 60;
        public float DurationInMinutes => durationInMinutes;

        public LivesInfiniteModeReward() { }
        public LivesInfiniteModeReward(float durationInMinutes)
        {
            this.durationInMinutes = durationInMinutes;
        }

        public override void ApplyReward(int multi = 1)
        {
            LivesSystem.EnableInfiniteMode(durationInMinutes * 60 * multi);
        }

        public override List<IRewardPreview> GetRewardPreviews(int multi = 1)
        {
            float multiData = durationInMinutes * multi;

            string durationFormat = "{mm} min";
            if (multiData > 60)
                durationFormat = "{hh} hr";

            return new List<IRewardPreview>()
            {
                new RewardPreview(LivesSystem.Data.RewardPreviewSprite, TimeUtils.GetFormatedTime(multiData, durationFormat), PREVIEW_SORTING_ORDER, LivesSystem.Data.RewardInfiniteModePreviewPrefab)
            };
        }
    }
}
