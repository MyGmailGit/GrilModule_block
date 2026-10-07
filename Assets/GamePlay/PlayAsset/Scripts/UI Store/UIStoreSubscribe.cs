using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
namespace Watermelon.IAPStore
{
    public class UIStoreSubscribe : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaTransform;
        [SerializeField] SubscribeSwitchButton weeklyButton;
        [SerializeField] SubscribeSwitchButton monthlyButton;
        [SerializeField] SubscribeSwitchButton yearlyButton;
        [SerializeField] SubscribeButton subscribeButton;

        [SerializeField] Button privacyPolicyButton;
        [SerializeField] Button termsOfUseButton;
        [SerializeField] Button closeButton;


        public static string[] GetDescription(ProductKeyType key)
        {
            switch (key)
            {
                case ProductKeyType.SubscriptionWeekly:
                    return new[] { "Weekly Subscription", "7 Days" };
                case ProductKeyType.SubscriptionMonthly:
                    return new[] { "Monthly Subscription", "30 Days" };
                case ProductKeyType.SubscriptionYearly:
                    return new[] { "Yearly Subscription", "365 Days" };
                default:
                    return null;
            }
        }

        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaTransform);

            weeklyButton.Init(ProductKeyType.SubscriptionWeekly);
            weeklyButton.OnClicked += OnVipSwitchButtonClicked;
            weeklyButton.SetChosen(true);
            OnVipSwitchButtonClicked(weeklyButton, ProductKeyType.SubscriptionWeekly);
            monthlyButton.Init(ProductKeyType.SubscriptionMonthly);
            monthlyButton.OnClicked += OnVipSwitchButtonClicked;
            yearlyButton.Init(ProductKeyType.SubscriptionYearly);
            yearlyButton.OnClicked += OnVipSwitchButtonClicked;


            privacyPolicyButton.onClick.AddListener(() =>
            {
                Application.OpenURL(Monetization.Settings.PrivacyLink_Sub);

                AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            });

            termsOfUseButton.onClick.AddListener(() =>
            {
                Application.OpenURL(Monetization.Settings.TermsOfUseLink_Sub);
                AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            });

            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }



        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
        }

        private void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UIStoreSubscribe>();
        }

        private void OnVipSwitchButtonClicked(SubscribeSwitchButton button, ProductKeyType key)
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            foreach (SubscribeSwitchButton subscribeButton in new[] { weeklyButton, monthlyButton, yearlyButton })
            {
                if (subscribeButton != button)
                {
                    subscribeButton.SetChosen(false);
                }
            }
            subscribeButton.SetProducktKey(key);

        }
    }
}
