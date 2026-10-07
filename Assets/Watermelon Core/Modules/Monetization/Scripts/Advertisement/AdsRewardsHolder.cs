using System;
using UnityEngine;
using GameBase;
using UnityEngine.UI;

namespace Watermelon
{
    public sealed class AdsRewardsHolder : RewardsHolder
    {
        [Group("Settings"), UniqueID]
        [SerializeField] string rewardID;

        [Group("Settings"), Space]
        [SerializeField] Button adsButton;

        // [Group("Settings")]
        // [SerializeField] int disableAfterPurchase_num = 5;

        [Group("Settings"), Space]
        [SerializeField] string analyticsEvent = "Default";

        private const int DisableAfterPurchase_num = 2;

        private AdsRewardsSave save;

        private class AdsRewardsSave : ISaveObject
        {
            [SerializeField] long adsRewardTimeUnixTime;
            [SerializeField] public long disableAfterPurchase_num = DisableAfterPurchase_num;
            public long AdsRewardTimeUnixTime
            {
                get => adsRewardTimeUnixTime;
                set => this.adsRewardTimeUnixTime = value;
            }

            public void Flush() { }
        }

        public void Refresh()
        {
            var save = GetSave();

            var netMgr = NetTimeMgr.Instance;
            if (netMgr == null || !netMgr.HasGetNetTime)
            {
                gameObject.SetActive(false);
                return;
            }
            else
            {
                DateTime dateTime = new DateTime(save.AdsRewardTimeUnixTime);
                if (save.disableAfterPurchase_num <= 0 && dateTime.Date == netMgr.CurTime.Date)
                {
                    // Disable holder game object
                    gameObject.SetActive(false);
                    return;
                }
                else if (dateTime.Date != netMgr.CurTime.Date)
                {
                    save.disableAfterPurchase_num = DisableAfterPurchase_num;
                }
            }
        }
        private AdsRewardsSave GetSave()
        {
            if (save == null)
            {
                save = SaveController.GetSaveObject<AdsRewardsSave>($"AdsRewardsSave_CurrencyProduct_{rewardID}");
            }
            return save;
        }

        private void Start()
        {
            InitializeComponents();

            // save = SaveController.GetSaveObject<AdsRewardsSave>($"AdsRewardsSave_CurrencyProduct_{rewardID}");
            // var netMgr = NetTimeMgr.Instance;
            // if (netMgr == null || !netMgr.HasGetNetTime)
            // {
            //     gameObject.SetActive(false);
            //     return;
            // }
            // else
            // {
            //     DateTime dateTime = new DateTime(save.AdsRewardTimeUnixTime);
            //     if (disableAfterPurchase && dateTime.Date == netMgr.CurTime.Date)
            //     {
            //         // Disable holder game object
            //         gameObject.SetActive(false);
            //         return;
            //     }
            // }

            adsButton.onClick.AddListener(OnPurchased);
        }

        private void OnPurchased()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            AdsManager.ShowRewardBasedVideo((reward) =>
            {
                if (reward)
                {
                    GetSave().disableAfterPurchase_num--;

                    rewardSet.ApplyReward();

                    GetSave().AdsRewardTimeUnixTime = NetTimeMgr.Instance != null ? NetTimeMgr.Instance.CurTime.Ticks : DateTime.Now.Ticks;

                    if (GetSave().disableAfterPurchase_num <= 0)
                    {
                        // Disable holder game object
                        gameObject.SetActive(false);
                    }

                    SaveController.MarkAsSaveIsRequired();
                }
            }, AnalyticsStr.reward_store_ad_coin);
        }
    }
}
