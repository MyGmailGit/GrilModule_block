using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Watermelon
{
    public class UISupremeDealPopUp : UIIAPOffer
    {
        [SerializeField] RectTransform[] bottles;

        private List<Vector3> localPosBase = new List<Vector3>();

        public override void Init()
        {
            base.Init();
            foreach (var it in bottles)
            {
                localPosBase.Add(it.localPosition);
            }
        }
        public override void PlayShowAnimation()
        {
            base.PlayShowAnimation();

            foreach (var it in bottles)
            {
                it.localPosition = Vector3.zero;
            }
            for (int i = 0; i < bottles.Length; ++i)
            {
                bottles[i].DOLocalMove(localPosBase[i], 0.5f).SetDelay(i * 0.15f);
            }
            AudioController.PlaySound(AudioController.AudioClips.Fly);
        }

        public static void ShowAuto()
        {
            var save = SaveController.GetSaveObject<SimpleLongSave>($"SupremeDealPopUp_Time");
            long unixTimestampNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (!IsSameDayUtcFast(save.Value, unixTimestampNow))
            {
                save.Value = unixTimestampNow;
                save.Flush();

                UIPopupQueueManager.Instance.EnqueuePopup<UISupremeDealPopUp>();
            }
            // UIController.ShowPage<UISupremeDealPopUp>();
        }
        public static bool IsSameDayUtcFast(long a, long b)
        {
            // 计算从 Unix Epoch 开始经过的天数（UTC）
            long daysA = a / 86400;      // 86400 = 24*60*60
            long daysB = b / 86400;
            return daysA == daysB;
        }

    }
}