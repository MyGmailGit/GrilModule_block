using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] CanvasGroup backgroundFade;
        [BoxGroup("References")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Content", "Content")]
        [SerializeField] GameObject gameOverPanel;
        [BoxGroup("Content")]
        [SerializeField] UIScaleAnimation levelFailed;

        [BoxGroup("Revive", "Revive")]
        [SerializeField] GameObject revivePanel;
        [BoxGroup("Revive")]
        [SerializeField] TextMeshProUGUI reviveTimerText;
        [BoxGroup("Revive")]
        [SerializeField] Image reviveFillbarImage;
        [BoxGroup("Revive")]
        [SerializeField] Button reviveButton;
        [BoxGroup("Revive")]
        [SerializeField] Button reviveSkipButton;
        [BoxGroup("Revive")]
        [SerializeField] Button coinButton;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button replayButton;

        private bool showRevive;
        private Tween floatTweenCase;

        private int coinNeed = 150;

        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            replayButton.onClick.AddListener(OnReplayButtonClicked);
            reviveButton.onClick.AddListener(OnReviveButtonClicked);
            reviveSkipButton.onClick.AddListener(OnReviveSkipButtonClicked);
            coinButton.onClick.AddListener(OnCoinButtonClicked);

            showRevive = false;
        }

        public override void PlayShowAnimation()
        {
            backgroundFade.alpha = 0.0f;

            if (showRevive)
            {
                backgroundFade.DOFade(0.7f, 0.3f);

                revivePanel.SetActive(true);
                gameOverPanel.SetActive(false);

                int reviveDuration = GameData.Data.ReviveDuration;
                int targetTime = 1;

                reviveTimerText.text = reviveDuration.ToString();

                // floatTweenCase = DOVirtual.Float(0, reviveDuration, reviveDuration, (value) =>
                // {
                //     reviveFillbarImage.fillAmount = 1.0f - floatTweenCase.State;

                //     if (value >= targetTime)
                //     {
                //         targetTime += 1;
                //         reviveDuration -= 1;

                //         reviveTimerText.text = reviveDuration.ToString();
                //     }
                // }).OnComplete(() =>
                // {
                //     ShowLevelFailUI();
                // });
            }
            else
            {
                ShowLevelFailUI();
            }

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        private void ShowLevelFailUI()
        {
#if MODULE_MONETIZATION
            AdsManager.ShowInterstitial((result) =>
            {
                // LivesSystem.TakeLife();

                backgroundFade.DOFade(1f, 0.3f);

                revivePanel.SetActive(false);
                gameOverPanel.SetActive(true);
            }, AnalyticsStr.inter_level_fail);
#else
            LivesSystem.TakeLife();

            backgroundFade.DOFade(1f, 0.3f);

            revivePanel.SetActive(false);
            gameOverPanel.SetActive(true);
#endif
        }

        private void OnDestroy()
        {
            floatTweenCase?.Kill();
        }

        public static void Show(bool showRevive)
        {
            UIGameOver gameOverUI = UIController.GetPage<UIGameOver>();
            gameOverUI.showRevive = showRevive;

            AdsSettings adsSettings = Monetization.AdsSettings;
            if (adsSettings.RewardedVideoType == AdProvider.Disable || !Monetization.IsActive)
                gameOverUI.showRevive = false;

            UIController.ShowPage<UIGameOver>();
        }
        #region Buttons 

        public void OnReplayButtonClicked()
        {
            // GameController.Replay();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnReviveButtonClicked()
        {
            floatTweenCase?.Pause();

            AdsManager.ShowRewardBasedVideo((reward) =>
            {
                if (reward)
                {
                    GameController.Revive(GameData.Data.ReviveExtraSeconds);
                }
                else
                {
                    // floatTweenCase?.Complete();
                    ShowLevelFailUI();
                }
            }, AnalyticsStr.reward_level_fail_addtime);

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
        private void OnCoinButtonClicked()
        {
            floatTweenCase?.Pause();

            if (CurrencyController.HasAmount(CurrencyType.Coins, coinNeed))
            {
                CurrencyController.Substract(CurrencyType.Coins, coinNeed);
                GameController.Revive(GameData.Data.ReviveExtraSeconds);
            }
            else
            {

                // floatTweenCase?.Complete();
                // ShowLevelFailUI();
            }

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnReviveSkipButtonClicked()
        {
            // floatTweenCase?.Complete();
            ShowLevelFailUI();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
        #endregion
    }
}