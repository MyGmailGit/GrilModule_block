using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Rewards Set", menuName = "Data/Rewards/Rewards Set")]
    public class RewardsSet : ScriptableObject
    {
        [Group("System")]
        [SerializeReference] List<Reward> rewards = new List<Reward>();
        public List<Reward> Rewards => rewards;

        [Group("System")]
        [SerializeField] string notes;

        public event SimpleIntCallback RewardRecieved;

        public void ApplyReward(int multi = 1)
        {
            if (!rewards.IsNullOrEmpty())
            {
                for (int i = 0; i < rewards.Count; i++)
                    rewards[i]?.ApplyReward(multi);
            }

            RewardRecieved?.Invoke(multi);
        }

        public List<IRewardPreview> GetPreviews(int multi = 1)
        {
            List<IRewardPreview> preview = new List<IRewardPreview>();
            foreach (Reward reward in rewards)
            {
                List<IRewardPreview> rewardPreview = reward.GetRewardPreviews(multi);
                if (!rewardPreview.IsNullOrEmpty())
                {
                    preview.AddRange(rewardPreview);
                }
            }

            return preview;
        }
    }
}
