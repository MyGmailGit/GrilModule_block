using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    [RegisterReward(typeof(LivesRewardView))]
    public sealed class LivesMaxLivesReward : Reward
    {
        private const int PREVIEW_SORTING_ORDER = 0;

        [SerializeField] int maxLivesAmount = 5;
        public int MaxLivesAmount => maxLivesAmount;

        public LivesMaxLivesReward() { }
        public LivesMaxLivesReward(int maxLivesAmount)
        {
            this.maxLivesAmount = maxLivesAmount;
        }

        public override void ApplyReward(int multi = 1)
        {
            LivesSystem.OverrideMaxLivesCount(maxLivesAmount);
        }

        public override List<IRewardPreview> GetRewardPreviews(int multi = 1)
        {
            int multiValue = maxLivesAmount * multi;

            return new List<IRewardPreview>()
            {
                new RewardPreview(LivesSystem.Data.RewardPreviewSprite, multiValue.ToString(), PREVIEW_SORTING_ORDER, LivesSystem.Data.RewardMaxLivesPreviewPrefab)
            };
        }
    }
}
