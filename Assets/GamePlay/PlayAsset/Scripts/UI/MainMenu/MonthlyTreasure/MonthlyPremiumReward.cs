using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    [RegisterReward(typeof(MonthlyPremiumRewardView))]
    public sealed class MonthlyPremiumReward : Reward
    {
        public MonthlyPremiumReward() { }

        public override void ApplyReward(int multi = 1)
        {
            MonthlyCtrl.Instance.SetIAP_BuyPremium(true);
        }

        public override bool CheckDisableState()
        {
            return false;
        }

        public override List<IRewardPreview> GetRewardPreviews(int multi = 1)
        {
            return null;
        }
    }
}
