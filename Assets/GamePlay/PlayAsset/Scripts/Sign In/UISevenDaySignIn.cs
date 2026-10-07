using System.Collections;
using System.Collections.Generic;
using Game.RedDot;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DG.Tweening;
using VideoSystem;

namespace Watermelon
{
    public class UISevenDaySignIn : UIPage, IPopupWindow//, IPausePopup
    {
        private const string CLAIM_BUTTON_TEXT = "CLAIM";
        private const string CLAIMED_BUTTON_TEXT = "CLAIMED";
        private const string NETWORK_BUTTON_TEXT = "NETWORK REQUIRED";
        private const string SYNCING_BUTTON_TEXT = "SYNCING...";
        private const string NETWORK_WARNING_MESSAGE = "Network required to claim today's sign-in.";
        private const int COIN_CLOUD_ELEMENTS = 10;
        private static readonly string[] DAY_ROOT_NAMES =
        {
            "ButtonDayOen#Transform",
            "ButtonDayTwo#Transform",
            "ButtonDayThree#Transform",
            "ButtonDayFour#Transform",
            "ButtonDayFive#Transform",
            "ButtonDaySix#Transform",
            "ButtonDaySeven#Transform",
        };

        [SerializeField] Image backgroundImage;
        [SerializeField] RectTransform panelRectTransform;
        // [SerializeField] RectTransform contentRectTransform;
        [SerializeField] Button closeButton;

        [SerializeField] List<RewardsSet> rewardsSets = new(8);
        [SerializeField] RewardsSet rewardsSet7;
        [SerializeField] RewardsSet rewardsSet8;

        [SerializeField] DailySignInRewardsHolder dailySignInRewardsHolder;

        private readonly List<DayView> dayViews = new List<DayView>();

        List<RewardsSet> rewardsSetsLocal = new();

        private Button claimButton;
        private Text claimButtonText;
        private float fadeIntensity;
        private Coroutine refreshStateCoroutine;
        private SevenDaySignInState currentState;
        // private Sprite coinRewardSprite;

        public bool IsOpened => canvas.enabled;

        public override void Init()
        {

#if TEST_MODE
            // 如果设置强制走视频 ，就要修改哪些按钮不可见
            if (DevPanelEnabler.IsDevForceToVideo || FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#else
            if (FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#endif
            {
                rewardsSetsLocal.Clear();
                foreach (var it in rewardsSets)
                {
                    rewardsSetsLocal.Add(it);
                }
                rewardsSetsLocal.Add(rewardsSet7);
            }
            else
            {
                rewardsSetsLocal.Clear();
                foreach (var it in rewardsSets)
                {
                    rewardsSetsLocal.Add(it);
                }
                rewardsSetsLocal.Add(rewardsSet8);
            }



            ResolveStaticReferences();

            fadeIntensity = backgroundImage != null ? backgroundImage.color.a : 0.7f;

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);

            if (backgroundImage != null)
                backgroundImage.AddEvent(EventTriggerType.PointerDown, OnBackgroundClicked);

            if (claimButton != null)
                claimButton.onClick.AddListener(OnClaimButtonClicked);

            // LoadRewardSprites();
            CacheDayViews();
            ApplyRewardVisuals();
            ApplyState(SevenDaySignInController.CurrentState);
            dailySignInRewardsHolder.Init(this);
        }

        public override void PlayShowAnimation()
        {
            if (panelRectTransform != null)
            {
                panelRectTransform.anchoredPosition = Vector2.down * 2200.0f;
                panelRectTransform.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutSine);
            }

            if (backgroundImage != null)
            {
                backgroundImage.SetAlpha(0.0f);
                backgroundImage.DOFade(fadeIntensity, 0.3f).OnComplete(() =>
                {
                    UIController.OnPageOpened(this);
                });
            }
            else
            {
                UIController.OnPageOpened(this);
            }

            StartStateRefresh();
        }

        public override void PlayHideAnimation()
        {
            if (refreshStateCoroutine != null)
            {
                StopCoroutine(refreshStateCoroutine);
                refreshStateCoroutine = null;
            }

            if (panelRectTransform != null)
            {
                panelRectTransform.DOAnchorPos(Vector2.down * 2200.0f, 0.3f).SetEase(Ease.InSine);
            }

            if (backgroundImage != null)
            {
                backgroundImage.DOFade(0.0f, 0.3f).OnComplete(() =>
                {
                    UIController.OnPageClosed(this);
                });
            }
            else
            {
                UIController.OnPageClosed(this);
            }
        }

        private void OnDestroy()
        {
            if (refreshStateCoroutine != null)
            {
                StopCoroutine(refreshStateCoroutine);
                refreshStateCoroutine = null;
            }
        }

        private void ResolveStaticReferences()
        {
            if (backgroundImage == null)
                backgroundImage = FindDeepComponent<Image>("Fade") ?? FindFullScreenBackground();

            if (panelRectTransform == null)
                panelRectTransform = FindDeepTransform("ImageBG") as RectTransform;

            // if (contentRectTransform == null)
            //     contentRectTransform = FindDeepTransform("Count#RectTransform") as RectTransform;

            if (closeButton == null)
                closeButton = FindDeepComponent<Button>("ButtonClose#Button");

            claimButton = FindDeepComponent<Button>("ButtonSignIn#Button");
            claimButtonText = FindDeepComponent<Text>("TextSignIn");
        }

        // private void LoadRewardSprites()
        // {
        //     Currency coinsCurrency = CurrencyController.GetCurrency(CurrencyType.Coins);
        //     coinRewardSprite = coinsCurrency != null ? coinsCurrency.Icon : null;
        // }

        private void CacheDayViews()
        {
            dayViews.Clear();

            for (int i = 0; i < DAY_ROOT_NAMES.Length; i++)
            {
                Transform rootTransform = FindDeepTransform(DAY_ROOT_NAMES[i]);
                if (rootTransform == null)
                    continue;

                dayViews.Add(new DayView
                {
                    RootTransform = rootTransform,
                    Toggle = rootTransform.GetComponent<Toggle>(),
                    ObtainMask = FindChildRecursive(rootTransform, "obtainMask"),
                    RewardTexts = FindRewardTexts(rootTransform),
                    DayTitleText = FindTextRecursive(rootTransform, "title"),
                    DayLabelText = FindTextRecursive(rootTransform, "TextDay"),
                    RewardImage = FindRewardImage(rootTransform),
                });
            }
        }

        private void ApplyRewardVisuals()
        {
            for (int i = 0; i < dayViews.Count; i++)
            {
                DayView dayView = dayViews[i];
                string rewardText = "VIDEO";
                Sprite sprite = null;
                float scale = 1f;

                if (i < rewardsSetsLocal.Count && rewardsSetsLocal[i] != null)
                {
                    for (int j = 0; j < rewardsSetsLocal[i].Rewards.Count; j++)
                    {
                        var reward = rewardsSetsLocal[i].Rewards[j];
                        if (reward == null) continue;
                        reward.GetRewardPreviews().ForEach(r =>
                        {
                            if (r != null)
                            {
                                sprite = r.Icon;
                                rewardText = $"{r.Text}";
                            }
                        });

                        if (reward is CurrencyReward)
                        {
                            scale = 1.0f;
                        }
                    }
                }

                if (dayView.RewardTexts != null)
                {
                    for (int j = 0; j < dayView.RewardTexts.Count; j++)
                    {
                        if (dayView.RewardTexts[j] != null)
                            dayView.RewardTexts[j].text = rewardText;
                    }
                }

                if (dayView.DayTitleText != null)
                {
                    dayView.DayTitleText.text = $"Day {i + 1}";
                }

                if (dayView.DayLabelText != null)
                {
                    dayView.DayLabelText.text = $"Day {i + 1}";
                }

                if (dayView.RewardImage != null)
                {
                    if (i < rewardsSetsLocal.Count && rewardsSetsLocal[i] != null)
                    {
                        dayView.RewardImage.sprite = sprite;
                        dayView.RewardImage.transform.localScale = Vector3.one * scale;
                    }
                    dayView.RewardImage.preserveAspect = true;
                }
            }
        }

        private void StartStateRefresh()
        {
            if (refreshStateCoroutine != null)
            {
                StopCoroutine(refreshStateCoroutine);
            }

            refreshStateCoroutine = StartCoroutine(RefreshStateCoroutine());
        }

        private IEnumerator RefreshStateCoroutine()
        {
            ApplyState(new SevenDaySignInState
            {
                IsLoading = true,
                StatusMessage = "Syncing network time...",
            });

            yield return SevenDaySignInController.RefreshStateCoroutine(OnStateRefreshed);

            refreshStateCoroutine = null;
        }

        private void OnStateRefreshed(SevenDaySignInState state)
        {
            ApplyState(state);
        }

        private void ApplyState(SevenDaySignInState state)
        {
            if (state == null)
                return;

            currentState = state;

            for (int i = 0; i < dayViews.Count; i++)
            {
                int dayNumber = i + 1;
                bool isClaimed = dayNumber <= state.ClaimedDaysCount;
                bool isCurrent = state.CanClaimToday && state.ClaimableDayNumber == dayNumber;

                if (dayViews[i].ObtainMask != null)
                    dayViews[i].ObtainMask.gameObject.SetActive(isClaimed);

                if (dayViews[i].Toggle != null)
                    dayViews[i].Toggle.isOn = isCurrent;
            }

            if (claimButton != null)
                claimButton.interactable = state != null && state.CanClaimToday;

            if (claimButtonText != null)
            {
                if (state == null || state.IsLoading)
                {
                    claimButtonText.text = SYNCING_BUTTON_TEXT;
                }
                else if (state.CanClaimToday)
                {
                    claimButtonText.text = CLAIM_BUTTON_TEXT;
                }
                else if (state.HasClaimedToday)
                {
                    claimButtonText.text = CLAIMED_BUTTON_TEXT;
                }
                else
                {
                    claimButtonText.text = NETWORK_BUTTON_TEXT;
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            UIController.HidePage<UISevenDaySignIn>();
        }

        private void OnClaimButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            if (currentState == null || currentState.IsLoading)
            {
                SystemMessage.ShowMessage("Syncing network time...");
                return;
            }

            if (!currentState.HasValidNetworkTime)
            {
                SystemMessage.ShowMessage(NETWORK_WARNING_MESSAGE);
                return;
            }

            if (!currentState.CanClaimToday)
            {
                SystemMessage.ShowMessage(currentState.StatusMessage);
                return;
            }

            if (SevenDaySignInController.TryClaimToday(out SevenDaySignInState updatedState, out SevenDaySignInClaimResult claimResult))
            {
                ApplyState(updatedState);

                if (claimResult != null && !claimResult.IsVideoReward)
                {
                    // RectTransform spawnPoint = CreateTemporaryCoinsSpawnPoint(claimResult.ClaimedDayNumber);

                    if (claimButton != null)
                        claimButton.interactable = false;

                    UIController.HidePage<UISevenDaySignIn>(() =>
                    {
                        PlayCoinsRewardAnimation(claimResult);//, spawnPoint);
                    });
                }
                else
                {
                    SystemMessage.ShowMessage(claimResult != null ? claimResult.RewardMessage : "Sign-in reward claimed.");
                }

                RedDotChecker.Instance.CheckDailyRedDot();
            }
        }

        private void PlayCoinsRewardAnimation(SevenDaySignInClaimResult claimResult)//, RectTransform spawnRectTransform)
        {
            VideoSerilNumberManager.Instance.specialData.ClearLastTimeGet();

            dailySignInRewardsHolder.Reset();
            dailySignInRewardsHolder.SetRewardSet(rewardsSetsLocal[claimResult.DayIdx]);

            dailySignInRewardsHolder.InitializeComponentsEx();
            rewardsSetsLocal[claimResult.DayIdx].ApplyReward();


            //统计是否有卡片获取到，然后
            if (VideoSerilNumberManager.Instance.specialData.lastTimeGetIds.Count > 0)
            {
                List<string> unlockIdsStr = new List<string>();
                foreach (var it in VideoSerilNumberManager.Instance.specialData.lastTimeGetIds)
                {
                    unlockIdsStr.Add(VideoSerilNumberManager.FormatSurpriseMainIdFileId(it.mainId_last, it.fileId_last));
                }

                UIPopDownload.Show(unlockIdsStr, true);

                VideoSerilNumberManager.Instance.specialData.ClearLastTimeGet();
            }
            else if (claimResult.DayIdx == SevenDaySignInController.DAYS_COUNT - 1)// 如果是7天并且没有获取到图片，就把地6天的奖励给他
            {
                rewardsSetsLocal[claimResult.DayIdx - 1].ApplyReward();
            }


            // RectTransform targetRectTransform = GetCoinsPanelRectTransform();

            // if (spawnRectTransform == null || targetRectTransform == null)
            // {
            //     CurrencyController.Add(CurrencyType.Coins, claimResult.CoinsAmount, $"sign_in_day_{claimResult.ClaimedDayNumber}");

            //     if (spawnRectTransform != null)
            //         Destroy(spawnRectTransform.gameObject);

            //     return;
            // }

            // FloatingCloud.SpawnCurrency(CurrencyType.Coins.ToString(), spawnRectTransform, targetRectTransform, COIN_CLOUD_ELEMENTS, "", () =>
            // {
            //     CurrencyController.Add(CurrencyType.Coins, claimResult.CoinsAmount, $"sign_in_day_{claimResult.ClaimedDayNumber}");
            // });

            // Destroy(spawnRectTransform.gameObject);
        }

        private RectTransform GetClaimDayRewardRectTransform(int claimedDayNumber)
        {
            int index = claimedDayNumber - 1;
            if (index >= 0 && index < dayViews.Count)
            {
                if (dayViews[index].RewardImage != null)
                    return dayViews[index].RewardImage.rectTransform;

                if (dayViews[index].RootTransform is RectTransform rootRectTransform)
                    return rootRectTransform;
            }

            return claimButton != null ? claimButton.GetComponent<RectTransform>() : null;
        }

        private RectTransform CreateTemporaryCoinsSpawnPoint(int claimedDayNumber)
        {
            RectTransform mainCanvasRectTransform = UIController.MainCanvas != null ? UIController.MainCanvas.transform as RectTransform : null;
            if (mainCanvasRectTransform == null)
                return null;

            GameObject tempSpawnObject = new GameObject("SignIn Coins Spawn Point", typeof(RectTransform));
            RectTransform tempRectTransform = tempSpawnObject.GetComponent<RectTransform>();
            tempRectTransform.SetParent(mainCanvasRectTransform, false);
            tempRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            tempRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            tempRectTransform.pivot = new Vector2(0.5f, 0.5f);
            tempRectTransform.sizeDelta = new Vector2(100.0f, 100.0f);
            tempRectTransform.localScale = Vector3.one;

            Camera uiCamera = GetUICamera();
            Vector2 screenCenterPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(mainCanvasRectTransform, screenCenterPoint, uiCamera, out Vector3 worldPoint))
            {
                tempRectTransform.position = worldPoint;
            }
            else
            {
                tempRectTransform.anchoredPosition = Vector2.zero;
                tempRectTransform.localPosition = Vector3.zero;
            }

            return tempRectTransform;
        }

        private Camera GetUICamera()
        {
            if (UIController.MainCanvas == null)
                return null;

            if (UIController.MainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return UIController.MainCanvas.worldCamera != null ? UIController.MainCanvas.worldCamera : Camera.main;
        }

        private RectTransform GetCoinsPanelRectTransform()
        {
            CurrencyUIPanelSimple[] currencyPanels = FindObjectsOfType<CurrencyUIPanelSimple>(true);
            for (int i = 0; i < currencyPanels.Length; i++)
            {
                CurrencyUIPanelSimple currencyPanel = currencyPanels[i];
                if (currencyPanel == null)
                    continue;

                if (currencyPanel.transform.IsChildOf(transform))
                    continue;

                if (currencyPanel.Currency != null && currencyPanel.Currency.CurrencyType == CurrencyType.Coins)
                    return currencyPanel.RectTransform;
            }

            return null;
        }

        private void OnBackgroundClicked(PointerEventData data)
        {
            UIController.HidePage<UISevenDaySignIn>();
        }

        private T FindDeepComponent<T>(string objectName) where T : Component
        {
            Transform targetTransform = FindDeepTransform(objectName);
            return targetTransform != null ? targetTransform.GetComponent<T>() : null;
        }

        private Transform FindDeepTransform(string objectName)
        {
            return FindChildRecursive(transform, objectName);
        }

        private static Transform FindChildRecursive(Transform parent, string objectName)
        {
            if (parent == null)
                return null;

            if (parent.name == objectName)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildRecursive(child, objectName);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static Text FindTextRecursive(Transform rootTransform, string objectName)
        {
            Transform textTransform = FindChildRecursive(rootTransform, objectName);
            return textTransform != null ? textTransform.GetComponent<Text>() : null;
        }

        private static List<Text> FindRewardTexts(Transform rootTransform)
        {
            List<Text> rewardTexts = new List<Text>();
            if (rootTransform == null)
                return rewardTexts;

            Text[] texts = rootTransform.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                Text text = texts[i];
                if (text == null)
                    continue;

                string objectName = text.gameObject.name;
                string value = text.text;

                if (objectName.Contains("Reward") || (!string.IsNullOrEmpty(value) && (value.StartsWith("x") || value == "VIDEO")))
                {
                    rewardTexts.Add(text);
                }
            }

            return rewardTexts;
        }

        private static Image FindRewardImage(Transform rootTransform)
        {
            string[] imageNames =
            {
                "RewardImage",
                "ImageRewardIcon",
                "rewardIcon",
                "rewardIcon (1)",
                "ImageIcon",
                "Girl",
            };

            for (int i = 0; i < imageNames.Length; i++)
            {
                Transform imageTransform = FindChildRecursive(rootTransform, imageNames[i]);
                if (imageTransform != null)
                {
                    Image image = imageTransform.GetComponent<Image>();
                    if (image != null)
                        return image;
                }
            }

            return null;
        }

        private Image FindFullScreenBackground()
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                RectTransform rectTransform = images[i].rectTransform;
                if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == Vector2.one)
                    return images[i];
            }

            return null;
        }

        private sealed class DayView
        {
            public Transform RootTransform;
            public Toggle Toggle;
            public Transform ObtainMask;
            public List<Text> RewardTexts;
            public Text DayTitleText;
            public Text DayLabelText;
            public Image RewardImage;
        }
    }
}
