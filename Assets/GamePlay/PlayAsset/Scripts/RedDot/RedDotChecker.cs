using System.Collections;
using System.Collections.Generic;
using Data;
using Game.RedDot;
using GameLogic;
using UnityEngine;
namespace Game.RedDot
{
    public class RedDotChecker : Singleton<RedDotChecker>
    {
        protected override void OnInit()
        {
            base.OnInit();
            // CheckRedDots();
        }

        // public void CheckRedDots()
        // {
        //     CheckDailyRedDot();
        //     CheckTreasureRedDot();
        // }

        public void CheckDailyRedDot()
        {
            if (Watermelon.SevenDaySignInController.CurrentState.CanClaimToday && !Watermelon.SevenDaySignInController.CurrentState.HasClaimedToday)
            {
                RedDotSystem.Instance.SetRedCount(Game.RedDot.RedDotDefine.Node.Daily, 1);
            }
            else
            {
                RedDotSystem.Instance.ClearNode(Game.RedDot.RedDotDefine.Node.Daily);
            }
        }

        public void CheckTreasureRedDot()
        {
            var save = Watermelon.SaveController.GetSaveObject<GameUI.UITreasure.TreasureSave>("TreasureSave");
            if (save.currentTreasureIndex < TreasureDataList.Data.TreasureDataListData.Count)
            {
                RedDotSystem.Instance.SetRedCount(Game.RedDot.RedDotDefine.Node.Treasure, 1);
            }
            else
            {
                RedDotSystem.Instance.ClearNode(Game.RedDot.RedDotDefine.Node.Treasure);
            }
        }
    }
}