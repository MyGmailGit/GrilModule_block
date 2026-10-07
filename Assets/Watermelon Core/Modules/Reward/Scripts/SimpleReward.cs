using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public sealed class SimpleReward
    {
        [SerializeReference] Reward reward;
        public Reward Reward => reward;

        public void ApplyReward(int multi = 1)
        {
            reward?.ApplyReward(multi);
        }

        public IRewardPreview GetPreview()
        {
            if (reward == null) return null;

            List<IRewardPreview> previews = reward.GetRewardPreviews();
            if (previews.IsNullOrEmpty()) return null;

            return previews[0];
        }
    }
}
