using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;
using Coffee.UIExtensions;


namespace Watermelon
{
    public class UIComplete : UIPage
    {
        private const float MULTIPLIER_SPIN_SPEED = 0.85f;
        private const float MULTIPLIER_MIN_TWO = 0.258f;
        private const float MULTIPLIER_MAX_TWO = 0.742f;
        private const float MULTIPLIER_MIN_THREE = 0.434f;
        private const float MULTIPLIER_MAX_THREE = 0.566f;

        [BoxGroup("References", "References")]
        [SerializeField] UIFadeAnimation backgroundFade;
        [BoxGroup("References")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanelUI;

        [Space]
        [BoxGroup("Content", "Content")]
        [SerializeField] TextMeshProUGUI rewardAmountText;
        [BoxGroup("Content")]
        [SerializeField] Image rewardCurrencyIconImage;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button nextLevelButton;
        [BoxGroup("Buttons")]
        [SerializeField] Button extraRewardButton;

        [SerializeField] UIParticle finishParticle;

        private UIScaleAnimation coinsPanelScalable;

        private CanvasGroup nextLevelCanvasGroup;
        private CanvasGroup extraRewardCanvasGroup;

        private CurrencyAmount currentReward;

        private TextMeshProUGUI extraRewardButtonText;
        private RectTransform multiplierWheelRect;
        private RectTransform multiplierArrowRect;

        private Coroutine multiplierSpinCoroutine;
        private bool multiplierSpinForward = true;
        private float multiplierNormalizedPosition;
        private int currentMultiplier = 2;
        private const int CurrentMultiplierFixed = 2;

        static bool isBackToMainMenu = false;
        public static void ShowPage(bool isToMainMenu)
        {
            isBackToMainMenu = isToMainMenu;
            UIController.ShowPage<UIComplete>();
        }

        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
            extraRewardButton.onClick.AddListener(OnExtraRewardButtonClicked);

            nextLevelCanvasGroup = nextLevelButton.GetComponent<CanvasGroup>();
            extraRewardCanvasGroup = extraRewardButton.GetComponent<CanvasGroup>();

            coinsPanelScalable = new UIScaleAnimation(coinsPanelUI);

            extraRewardButtonText = extraRewardButton.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            multiplierWheelRect = transform.Find("ImageWhellBG") as RectTransform;
            multiplierArrowRect = transform.Find("ImageWhellBG/Arrow") as RectTransform;

            UpdateMultiplierVisuals();
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            bool issub = IAPManager.IsSubscribed();

            finishParticle.Play();

            StopMultiplierSpin();

            // AudioClip popupClip = AudioController.AudioClips.completePopup != null
            //     ? AudioController.AudioClips.completePopup
            //     : AudioController.AudioClips.win;

            AudioController.PlaySound(AudioController.AudioClips.completePopup);

            AudioController.PlaySound(AudioController.AudioClips.spark01, 0.5f);

            extraRewardCanvasGroup.alpha = 0;
            extraRewardCanvasGroup.gameObject.SetActive(false);
            extraRewardCanvasGroup.interactable = false;

            nextLevelCanvasGroup.alpha = 0;
            nextLevelCanvasGroup.gameObject.SetActive(false);
            nextLevelCanvasGroup.interactable = false;

            currentReward = GameData.Data.DefaultReward;

            coinsPanelUI.Init(currentReward.CurrencyType);
            rewardCurrencyIconImage.sprite = currentReward.Currency.Icon;
            rewardAmountText.text = currentReward.FormattedPrice;

            coinsPanelScalable.Hide(immediately: true);

            backgroundFade.Show(duration: 0.3f);

            coinsPanelScalable.Show();

            DOVirtual.Float(0, currentReward.Amount, 0.6f, (value) =>
            {
                rewardAmountText.text = value.ToString("F0");
            }).OnComplete(() =>
            {
                AdsSettings adsSettings = Monetization.AdsSettings;
                if (adsSettings.RewardedVideoType != AdProvider.Disable && Monetization.IsActive)
                {
                    ResetMultiplierSpin();

                    if (issub)
                    {
                        extraRewardCanvasGroup.gameObject.SetActive(false);
                    }
                    else
                    {
                        extraRewardCanvasGroup.gameObject.SetActive(true);
                        extraRewardCanvasGroup.interactable = true;
                        extraRewardCanvasGroup.DOFade(1.0f, 0.3f);
                    }

                    StartMultiplierSpin();
                }

                nextLevelCanvasGroup.gameObject.SetActive(true);
                nextLevelCanvasGroup.DOFade(1.0f, 0.3f).SetDelay(0.3f).OnComplete(() =>
                {
                    nextLevelCanvasGroup.interactable = true;
                });

                FloatingCloud.SpawnCurrency(currentReward.CurrencyType.ToString(), rewardAmountText.rectTransform, coinsPanelUI.Image.rectTransform, 10, "", () =>
                {
                    CurrencyController.Add(currentReward.CurrencyType, currentReward.Amount);
                    if (issub)
                        AddExtraReward();
                });
            });

            UIController.OnPageOpened(this);

        }

        public override void PlayHideAnimation()
        {
            StopMultiplierSpin();
            UIController.OnPageClosed(this);
        }
        #endregion

        #region Buttons
        private void OnExtraRewardButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            StopMultiplierSpin();
            extraRewardCanvasGroup.interactable = false;

            AdsManager.ShowRewardBasedVideo((reward) =>
            {
                if (reward)
                {
                    // AddExtraReward(OnNextLevelButtonClicked);
                    // nextLevelCanvasGroup.interactable = false;
                    // nextLevelButton.interactable = false;
                    int tempReward = currentReward.Amount;
                    int additionalReward = tempReward * Mathf.Max(1, currentMultiplier - 1);//currentMultiplier - 1);
                    int finalReward = tempReward + additionalReward;

                    DOVirtual.Float(tempReward, finalReward, 0.6f, (value) =>
                    {
                        rewardAmountText.text = value.ToString("F0");
                    }).OnComplete(() =>
                    {
                        FloatingCloud.SpawnCurrency(currentReward.CurrencyType.ToString(), rewardAmountText.rectTransform, coinsPanelUI.Image.rectTransform, 10, "", () =>
                        {
                            CurrencyController.Add(currentReward.CurrencyType, additionalReward);

                            nextLevelCanvasGroup.interactable = false;
                            nextLevelButton.interactable = false;

                            OnNextLevelButtonClicked();
                        });
                    });

                    extraRewardCanvasGroup.gameObject.SetActive(false);
                }
                else
                {
                    extraRewardCanvasGroup.interactable = true;
                    StartMultiplierSpin();
                }
            }, AnalyticsStr.reward_level_finish_coindouble);
        }

        private void OnNextLevelButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            FirebaseRemote.FirebaseWrapper.CheckNoticfacation();

            Overlay.Show(0.3f, () =>
            {
                // ActiveSession activeSession = ActiveSession.Current;

                if (isBackToMainMenu)//activeSession.IsPlaySpecialLevel())
                {
                    // LivesSystem.UnlockLife(true);

                    SaveController.Save(true);

                    GameController.LoadMenu();
                }
                else
                {
                    GameController.Unload(() =>
                    {
                        // LivesSystem.LockLife();

                        SceneManager.LoadScene(GameConsts.SCENE_GAME);
                    });
                }
            });
        }

        private void AddExtraReward(Action callNext = null)
        {
            int tempReward = currentReward.Amount;
            int additionalReward = tempReward * Mathf.Max(1, currentMultiplier - 1);
            int finalReward = tempReward + additionalReward;

            DOVirtual.Float(tempReward, finalReward, 0.6f, (value) =>
            {
                rewardAmountText.text = value.ToString("F0");
            }).OnComplete(() =>
            {
                FloatingCloud.SpawnCurrency(currentReward.CurrencyType.ToString(), rewardAmountText.rectTransform, coinsPanelUI.Image.rectTransform, 10, "", () =>
                {
                    CurrencyController.Add(currentReward.CurrencyType, additionalReward);
                    callNext?.Invoke();
                    // OnNextLevelButtonClicked();
                });
            });
        }

        #endregion

        private void StartMultiplierSpin()
        {
            if (multiplierArrowRect == null)
                return;

            StopMultiplierSpin();
            multiplierSpinCoroutine = StartCoroutine(SpinMultiplierArrowCoroutine());
        }

        private void StopMultiplierSpin()
        {
            if (multiplierSpinCoroutine != null)
            {
                StopCoroutine(multiplierSpinCoroutine);
                multiplierSpinCoroutine = null;
            }
        }

        private void ResetMultiplierSpin()
        {
            multiplierSpinForward = true;
            multiplierNormalizedPosition = 0.0f;
            ApplyMultiplierPosition(multiplierNormalizedPosition);
        }

        private System.Collections.IEnumerator SpinMultiplierArrowCoroutine()
        {
            while (true)
            {
                float delta = Time.unscaledDeltaTime * MULTIPLIER_SPIN_SPEED;

                if (multiplierSpinForward)
                {
                    multiplierNormalizedPosition += delta;
                    if (multiplierNormalizedPosition >= 1.0f)
                    {
                        multiplierNormalizedPosition = 1.0f;
                        multiplierSpinForward = false;
                    }
                }
                else
                {
                    multiplierNormalizedPosition -= delta;
                    if (multiplierNormalizedPosition <= 0.0f)
                    {
                        multiplierNormalizedPosition = 0.0f;
                        multiplierSpinForward = true;
                    }
                }

                ApplyMultiplierPosition(multiplierNormalizedPosition);

                yield return null;
            }
        }

        private void ApplyMultiplierPosition(float normalizedPosition)
        {
            currentMultiplier = GetMultiplierForPosition(normalizedPosition);

            if (multiplierArrowRect != null)
            {
                float range = GetMultiplierArrowRange();
                Vector2 anchoredPosition = multiplierArrowRect.anchoredPosition;
                anchoredPosition.x = Mathf.Lerp(-range, range, normalizedPosition);
                multiplierArrowRect.anchoredPosition = anchoredPosition;
            }

            UpdateMultiplierVisuals();
        }

        private float GetMultiplierArrowRange()
        {
            if (multiplierWheelRect == null || multiplierArrowRect == null)
                return 160f;

            return Mathf.Max(0.0f, (multiplierWheelRect.rect.width - multiplierArrowRect.rect.width) * 0.5f - 18.0f);
        }

        private int GetMultiplierForPosition(float normalizedPosition)
        {
            if (normalizedPosition < MULTIPLIER_MIN_TWO || normalizedPosition > MULTIPLIER_MAX_TWO)
                return 2;

            if (normalizedPosition < MULTIPLIER_MIN_THREE || normalizedPosition > MULTIPLIER_MAX_THREE)
                return 3;

            return 4;
        }

        private void UpdateMultiplierVisuals()
        {
            if (extraRewardButtonText != null)
                extraRewardButtonText.text = $"Claim X{currentMultiplier}";//$"CLAIM X{currentMultiplier}";
        }
    }
}
