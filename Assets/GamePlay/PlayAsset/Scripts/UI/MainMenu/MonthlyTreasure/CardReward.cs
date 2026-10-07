using System;
using System.Collections.Generic;
using UnityEngine;
using VideoSystem;

namespace Watermelon
{
    [Serializable]
    [RegisterReward(typeof(CardRewardView))]
    public sealed class CardReward : Reward
    {
        [SerializeField] Sprite previewSprite;
        [SerializeField] int getNum = 1;
        public int GetNum => getNum;

        public CardReward() { }
        public CardReward(int getNum)
        {
            this.getNum = getNum;
        }

        public override void ApplyReward(int multi = 1)
        {
            for (int i = 0; i < this.getNum; ++i)
            {
                var specialOne = VideoSerilNumberManager.Instance.specialData.GetOneFileToUse();
                if (specialOne != null)
                {
                    VideoSerilNumberManager.Instance.specialData.SetOneFileUse(specialOne.Value.mainId, specialOne.Value.fileId);
                }
            }
        }

        public override bool CheckDisableState()
        {
            return false;
        }

        public override List<IRewardPreview> GetRewardPreviews(int multi = 1)
        {
            return new List<IRewardPreview>()
            {
                new RewardPreview(previewSprite, $"Card x{getNum}")
            };
        }
    }
}
