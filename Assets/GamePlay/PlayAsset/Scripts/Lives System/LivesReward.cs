using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    [RegisterReward(typeof(LivesRewardView))]
    public sealed class LivesReward : Reward
    {
        private const int PREVIEW_SORTING_ORDER = 0;

        [SerializeField] int livesAmount = 1;
        public int LivesAmount => livesAmount;

        public LivesReward() { }
        public LivesReward(int livesAmount)
        {
            this.livesAmount = livesAmount;
        }

        public override void ApplyReward(int multi = 1)
        {
            LivesSystem.AddLife(livesAmount, true);
        }

        public override List<IRewardPreview> GetRewardPreviews(int multi = 1)
        {
            return new List<IRewardPreview>()
            {
                new RewardPreview(LivesSystem.Data.RewardPreviewSprite, $"x{livesAmount * multi}", PREVIEW_SORTING_ORDER, LivesSystem.Data.RewardPreviewPrefab)
            };
        }
    }
}
