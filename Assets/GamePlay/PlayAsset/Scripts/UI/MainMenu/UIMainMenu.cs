using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using UnityEngine.UI;
using Watermelon.IAPStore;
using GameUI;
using Game.RedDot;
using Game.Video;
using System.Collections.Generic;
using VideoSystem;
using DG.Tweening;
using System.Threading.Tasks;

namespace Watermelon
{
    public partial class UIMainMenu : UIPage
    {
        private const string VIDEO_LOG_TAG = "[UIMainMenuVideo]";
        private const bool ENABLE_VIDEO_LOGS = true;
        public readonly float BUTTONS_RIGHT_OFFSET_X = 300F;

        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaRectTransform;
        [BoxGroup("References")]
        [SerializeField] RectTransform tapToPlayRect;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanel;
        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple diamondPanel;


        [BoxGroup("Side Buttons", "Side Buttons")]
        [SerializeField] UIMainMenuButton noAdsButton;
        [BoxGroup("Side Buttons")]
        [SerializeField] UIMainMenuButton vipButton;
        [BoxGroup("Side Buttons")]
        [SerializeField] UIMainMenuButton dailyButton;
        [BoxGroup("Side Buttons")]
        [SerializeField] UIMainMenuButton treasureButton;
        [BoxGroup("Side Buttons")]
        [SerializeField] UIMainMenuButton supremeButton;
        // [BoxGroup("Side Buttons")]
        // [SerializeField] Button storeButton;
        // [BoxGroup("Side Buttons")]
        // [SerializeField] Button storeButtonOrganic;
        // [BoxGroup("Side Buttons")]
        // [SerializeField] Button photoButton;

        [BoxGroup("Static Map", "Static Map")]
        [SerializeField] StaticMapPanel staticMapPanel;

        // Video related UI references
        [BoxGroup("Video", "Video")]
        [SerializeField] private RawImage videoDisplayTarget;
        [SerializeField] private Image fallbackBackgroundImage;
        [SerializeField] private VideoPlayCtrl videoPlayCtrl;

        [SerializeField] private MainMenuItem[] menuButtons;
        // [SerializeField] private GameObject[] menuPageObj;
        [SerializeField] private List<IMainPage> menuPage;

        [SerializeField] private TodayTaskProgress todayTaskProgress;

        [SerializeField] private GameObject menuPageCanvasPanel;
        [SerializeField] private GameObject menuItemCanvasPanel;


        private Color videoDisplayTargetDisplayColor = new Color(1, 1, 1, 0);

        private UIScaleAnimation coinsLabelScalable;
        private UIScaleAnimation diamondLabelScalable;
        private Coroutine signInRefreshCoroutine;
        private Coroutine firstLaunchAutoStartCoroutine;

        public static bool is_AB_VideoIsB_Local { get; private set; } = false;

        // Video configuration
        // private VideoSourceConfig currentVideoConfig;
        // private const string MENU_VIDEO_UNITY_FOLDER_PATH = "Assets/StreamingAssets/MenuBackgroundVideos";
        // private const string DEFAULT_REMOTE_VIDEO_BASE_URL = ServerUtil.DEFAULT_REMOTE_VIDEO_BASE_URL;

        private void OnEnable()
        {
            AdsManager.ForcedAdDisabled += ForceAdPurchased;
            GameGirlsHomeVideoController.OnHomeVideoChanged += OnHomeVideoChanged;
            IAPManager.PurchaseCompleted += OnPurchaseComplete;
        }

        private void OnDisable()
        {
            AdsManager.ForcedAdDisabled -= ForceAdPurchased;
            GameGirlsHomeVideoController.OnHomeVideoChanged -= OnHomeVideoChanged;
            IAPManager.PurchaseCompleted -= OnPurchaseComplete;
        }

        public override void Init()
        {
            // Initialize existing UI components
            coinsLabelScalable = new UIScaleAnimation(coinsPanel);
            coinsPanel.Init();
            diamondLabelScalable = new UIScaleAnimation(diamondPanel);
            diamondPanel.Init();

            staticMapPanel.Init();

            InitializeButtons();
            InitMenuButtons();

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            // Initialize video system
            // InitializeVideoConfig();
            SetupVideoDisplay();
        }

        #region Button Initialization

        private void InitializeButtons()
        {
            noAdsButton.Init(BUTTONS_RIGHT_OFFSET_X);
            vipButton.Init(BUTTONS_RIGHT_OFFSET_X);
            dailyButton.Init(-BUTTONS_RIGHT_OFFSET_X);
            treasureButton.Init(-BUTTONS_RIGHT_OFFSET_X);
            supremeButton.Init(-BUTTONS_RIGHT_OFFSET_X);

            noAdsButton.Button.onClick.AddListener(NoAdButton);
            vipButton.Button.onClick.AddListener(() => UIController.ShowPage<UIStoreSubscribe>());
            // storeButton.onClick.AddListener(StoreButton);
            // storeButtonOrganic.onClick.AddListener(StoreButton);
            dailyButton.Button.onClick.AddListener(OnSignInEntryButtonClicked);
            treasureButton.Button.onClick.AddListener(() => { UIController.ShowPage<UITreasure>(); });
            supremeButton.Button.onClick.AddListener(() => { UIController.ShowPage<UISupremeDealPopUp>(); });

            // if (photoButton == null)
            // photoButton = transform.Find("Levels Map/PhotoButton")?.GetComponent<Button>();

            // if (photoButton != null)
            // photoButton.onClick.AddListener(OnPhotoButtonClicked);

            coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);
            diamondPanel.AddButton.onClick.AddListener(AddDiamondButton);

            CheckOrganic();
        }

        #endregion

        #region 菜单栏控制

        /// <summary>
        /// 1 金币， 2 钻石
        /// </summary>
        /// <param name="isToCoin"></param>
        public void ShowMenuStore(int isToCoin = 0)
        {
            (menuPage[0] as UIStore).MoveToCoinOrDiamondPanel(isToCoin);

            OnMenuItemTouch(0);
        }

        private void InitMenuButtons()
        {
            // foreach (var it in menuPageObj)
            // {
            //     menuPage.Add(it.GetComponent<IMainPage>());
            // }

            foreach (var it in menuButtons)
            {
                // it.SetSelected(false);
                it.SetCallback(OnMenuItemTouch);
            }
            // menuButtons[2].SetSelected(true);

            foreach (var it in menuPage)
            {
                if (it != null)
                {
                    it.Init();
                    it.Hide();
                }
            }
            // menuPage[2].Show();
        }

        private void OnMenuItemTouch(MainMenuItem.MainMenuType obj)
        {
            int itemIdx = (int)obj;

            foreach (var it in menuButtons)
            {
                it.SetSelected(false);
            }
            menuButtons[itemIdx].SetSelected(true);

            foreach (var it in menuPage)
            {
                if (it != null)
                {
                    it.Hide();
                }
            }
            if (obj != MainMenuItem.MainMenuType.MainPage)
                menuPage[itemIdx].Show();
        }


        public RectTransform GetMenuItem(int idx)
        {
            if (idx >= 0 && idx < menuButtons.Length)
            {
                return menuButtons[idx].transform as RectTransform;
            }
            return null;
        }


        #endregion

        #region Video System Integration

        // private void InitializeVideoConfig()
        // {
        //     // currentVideoConfig = new VideoSourceConfig
        //     // {
        //     //     RemoteBaseUrl = DEFAULT_REMOTE_VIDEO_BASE_URL,
        //     //     LocalFolderPath = MENU_VIDEO_UNITY_FOLDER_PATH,
        //     //     FileExtension = ".mp4",
        //     //     DownloadTimeout = 5.0f,
        //     //     LoopVideo = true
        //     // };
        // }

        private void SetupVideoDisplay()
        {
            // Find or create video display target if not assigned
            if (videoDisplayTarget == null)
            {
                Transform backgroundTransform = transform.Find("Background");
                if (backgroundTransform != null)
                {
                    fallbackBackgroundImage = backgroundTransform.GetComponent<Image>();

                    Transform videoSurfaceTransform = backgroundTransform.Find("Video Surface");
                    if (videoSurfaceTransform == null)
                    {
                        GameObject videoSurfaceObject = new GameObject("Video Surface", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                        videoSurfaceObject.layer = backgroundTransform.gameObject.layer;
                        videoSurfaceObject.transform.SetParent(backgroundTransform, false);

                        RectTransform videoSurfaceRect = videoSurfaceObject.GetComponent<RectTransform>();
                        videoSurfaceRect.anchorMin = Vector2.zero;
                        videoSurfaceRect.anchorMax = Vector2.one;
                        videoSurfaceRect.offsetMin = Vector2.zero;
                        videoSurfaceRect.offsetMax = Vector2.zero;
                        videoSurfaceRect.localScale = Vector3.one;

                        videoSurfaceTransform = videoSurfaceObject.transform;

                        videoPlayCtrl = videoSurfaceObject.AddComponent<VideoPlayCtrl>();
                    }

                    videoDisplayTarget = videoSurfaceTransform.GetComponent<RawImage>();
                    if (videoDisplayTarget != null)
                    {
                        videoDisplayTarget.raycastTarget = false;
                        videoDisplayTarget.enabled = false;
                    }
                }
            }

            // Set display target for VideoPlayCtrl
            if (videoDisplayTarget != null)
            {
                videoPlayCtrl.SetDisplayTarget(videoDisplayTarget);
            }

            // Subscribe to video events
            // if (VideoPlayCtrl.Instance != null)
            {
                videoPlayCtrl.OnVideoStarted += OnVideoStarted;
                videoPlayCtrl.OnVideoError += OnVideoError;
            }
        }

        private void StartBackgroundVideo()
        {
            // if (VideoPlayCtrl.Instance == null || currentVideoConfig == null)
            //     return;

            // // Update video ID based on current game state
            // currentVideoConfig.VideoId = 
            string level = GetCurrentVideoId();

            // Simple one-line call to play video - all logic handled internally
            if (level != null)
                videoPlayCtrl.PlayLevelVideo(level, videoDisplayTarget);
        }

        private string GetCurrentVideoId()
        {
            if (GameGirlsHomeVideoController.HasCustomSelection)
                // if (int.TryParse(GameGirlsHomeVideoController.SelectedLevelId, out var levelid))
                return GameGirlsHomeVideoController.SelectedLevelId;


            // 获取上一张图片信息（任何时间都可以获取，不受 currentProgress 状态影响）
            var previousInfo = VideoSerilNumberManager.Instance.GetPreviousImage();
            if (previousInfo.HasValue)
            {
                Debug.Log($"上一张图片: 主编号={previousInfo.Value.mainId}, 文件编号={previousInfo.Value.fileId}");
                string fullId = VideoSerilNumberManager.FormatMainIdFileId(previousInfo.Value.mainId, previousInfo.Value.fileId);
                return fullId;
            }
            else
            {
                // 获取正在进行的视频，其实也就只有第一关会出现，其他时候根本就不可能有
                var curpro = VideoSerilNumberManager.Instance.GetCurrentFullName();
                if (curpro != null)
                {
                    string fullId = VideoSerilNumberManager.FormatMainIdFileId(curpro.Value.mainId, curpro.Value.fileId);
                    return fullId;
                }
                else
                {
                    return null;
                }
            }


            // ActiveSession session = ActiveSession.Current;

            // int currentIndex = session.Save.DisplayLevelIndex;

            // return currentIndex;

            // LevelSave levelSave = SaveController.GetSaveObject<LevelSave>();
            // int homeLevelId = 1;

            // if (levelSave != null)
            // {
            //     homeLevelId = Mathf.Max(1, levelSave.MaxReachedLevelIndex);
            // }

            // return $"100{homeLevelId:00}";
        }

        private void UpdateVideoVisibility(bool isVisible)
        {
            if (fallbackBackgroundImage != null)
            {
                // fallbackBackgroundImage.enabled = !isVisible;
            }

            if (videoDisplayTarget != null)
            {
                videoDisplayTarget.enabled = isVisible;
            }

            VideoLog($"Video background visible: {isVisible}");
        }

        private void OnVideoStarted(string level)
        {
            UpdateVideoVisibility(true);
            VideoLog("Video playback started");

            videoDisplayTarget.color = videoDisplayTargetDisplayColor;
            videoDisplayTarget.DOFade(1, 0.2f).OnComplete(delegate
            {
            });
        }

        private void OnVideoError(string level, string error)
        {
            VideoLogWarning($"Video error: {error}");
            UpdateVideoVisibility(false);
        }

        private void OnHomeVideoChanged(string fileId)
        {
            if (!IsPageDisplayed) return;
            StartBackgroundVideo();
        }

        #endregion

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            HideAdButton(true);

            coinsLabelScalable.Show();
            diamondLabelScalable.Show();

            ShowAdButton();
            ShowSupremeDealButton();

            dailyButton.Show();
            treasureButton.Show();

            vipButton.Show();

            if (ShouldAutoStartFirstLevel())
            {
                if (firstLaunchAutoStartCoroutine != null)
                {
                    StopCoroutine(firstLaunchAutoStartCoroutine);
                }

                firstLaunchAutoStartCoroutine = StartCoroutine(AutoStartFirstLevelCoroutine());

                UIController.OnPageOpened(this);
                return;
            }

            StartBackgroundVideo();

            UIGetSpecialPic.Show();

            StartSignInStateRefresh();

            UIController.OnPageOpened(this);

            RedDotChecker.Instance.CheckTreasureRedDot();

            MusicSource.ActiveMusicSource.Activate(1);

            CheckToShowMonthlyTask();

            CheckOrganic();
        }
        /// <summary>
        /// 检查月task 是否已经启动
        /// </summary>
        private void CheckToShowMonthlyTask()
        {
            if (MonthlyCtrl.Instance.IsStartMonthlyTask())
            {
                todayTaskProgress.gameObject.SetActive(true);
                todayTaskProgress.InitTodayTaskProgress();
            }
            else
            {
                todayTaskProgress.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 自然量就不显示相册
        /// </summary>
        private void CheckOrganic()
        {

#if TEST_MODE
            // 如果设置强制走视频 ，就要修改哪些按钮不可见
            if (DevPanelEnabler.IsDevForceToVideo || FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#else
            if (FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#endif
            {
                is_AB_VideoIsB_Local = true;

                todayTaskProgress.gameObject.SetActive(true);
                menuPageCanvasPanel.gameObject.SetActive(true);
                menuItemCanvasPanel.gameObject.SetActive(true);
                // photoButton.gameObject.SetActive(true);
                // storeButton.gameObject.SetActive(true);
                // GallaryButton.Button.gameObject.SetActive(true);
                // storeButtonOrganic.gameObject.SetActive(false);
                // if (ServerRemoteMgr.Instance.Remote_GetIsPlayWZMoney())
                // {
                //     moneyPanel.gameObject.SetActive(true);
                // }
                // else
                // {
                //     // 隐藏moneypanel
                //     moneyPanel.gameObject.SetActive(false);
                //     SetPanelMoveUp90(coinsPanel.transform);
                //     SetPanelMoveUp90(livesIndicator.transform);
                // }
            }
            else
            {
                is_AB_VideoIsB_Local = false;

                todayTaskProgress.gameObject.SetActive(false);
                menuPageCanvasPanel.gameObject.SetActive(false);
                menuItemCanvasPanel.gameObject.SetActive(false);
                // photoButton.gameObject.SetActive(false);
                // storeButton.gameObject.SetActive(false);
                // GallaryButton.Button.gameObject.SetActive(false);

                // storeButtonOrganic.gameObject.SetActive(true);

                // // 隐藏moneypanel
                // moneyPanel.gameObject.SetActive(false);
                // SetPanelMoveUp90(coinsPanel.transform);
                // SetPanelMoveUp90(livesIndicator.transform);
            }

            // #else
            //             if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
            //             {

            //                 photoButton.gameObject.SetActive(false);
            //                 storeButton.gameObject.SetActive(false);
            //                 storeButtonOrganic.gameObject.SetActive(true);

            //                 GallaryButton.Button.gameObject.SetActive(false);

            //                 // 隐藏moneypanel
            //                 moneyPanel.gameObject.SetActive(false);
            //                 SetPanelMoveUp90(coinsPanel.transform);
            //                 SetPanelMoveUp90(livesIndicator.transform);
            //             }
            //             else
            //             {
            //                 photoButton.gameObject.SetActive(true);
            //                 storeButton.gameObject.SetActive(true);
            //                 storeButtonOrganic.gameObject.SetActive(false);
            //                 GallaryButton.Button.gameObject.SetActive(true);

            //                 if (ServerRemoteMgr.Instance.Remote_GetIsPlayWZMoney())
            //                 {
            //                     moneyPanel.gameObject.SetActive(true);
            //                 }
            //                 else
            //                 {
            //                     // 隐藏moneypanel
            //                     moneyPanel.gameObject.SetActive(false);

            //                     SetPanelMoveUp90(coinsPanel.transform);
            //                     SetPanelMoveUp90(livesIndicator.transform);
            //                 }
            //             }
            // #endif


            // #if TEST_MODE
            //             // 如果设置强制走视频 ，就要修改哪些按钮不可见
            //             if (DevPanelEnabler.IsDevForceToVideo)
            //             {
            //                 photoButton.gameObject.SetActive(true);
            //                 storeButton.gameObject.SetActive(true);
            //                 storeButtonOrganic.gameObject.SetActive(false);
            //             }
            //             else
            //             {
            //                 photoButton.gameObject.SetActive(false);
            //                 storeButton.gameObject.SetActive(false);
            //                 storeButtonOrganic.gameObject.SetActive(true);
            //             }
            // #else
            //             string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic); //"organic");
            //             Debug.Log($"[ReleaseMode] adjust={enterType}, force={FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA()}");
            //             // if (enterType == AdjustAnalyticsModule.AdjustOrganic || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())//"organic")
            //             if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
            //                 || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())
            //             {
            //                 photoButton.gameObject.SetActive(false);
            //                 storeButton.gameObject.SetActive(false);
            //                 storeButtonOrganic.gameObject.SetActive(true);
            //             }
            //             else
            //             {
            //                 photoButton.gameObject.SetActive(true);
            //                 storeButton.gameObject.SetActive(true);
            //                 storeButtonOrganic.gameObject.SetActive(false);
            //             }
            // #endif
        }

        public override void PlayHideAnimation()
        {
            // if (VideoPlayCtrl.Instance != null)
            {
                videoPlayCtrl.Pause();
            }

            if (signInRefreshCoroutine != null)
            {
                StopCoroutine(signInRefreshCoroutine);
                signInRefreshCoroutine = null;
            }

            UIController.OnPageClosed(this);
        }

        #endregion

        #region Side Buttons Logic

        private void ShowAdButton(bool immediately = false)
        {
            if (AdsManager.IsForcedAdEnabled())
            {
                noAdsButton.Show(immediately);
            }
            else
            {
                noAdsButton.Hide(immediately: true);
            }
        }

        private void HideAdButton(bool immediately = false)
        {
            if (AdsManager.IsForcedAdEnabled())
            {
                noAdsButton.Hide(immediately);
            }
        }

        private void ForceAdPurchased()
        {
            noAdsButton.Hide(true);
        }

        private void ShowSupremeDealButton()
        {
            var save = SaveController.GetSaveObject<IAPItem.Save>($"iap_{ProductKeyType.StarterPack}");

            var product = IAPManager.GetProductData(ProductKeyType.StarterPack);

            if (IAPManager.IsPurchased(ProductKeyType.StarterPack) || product.ProductType == ProductType.NonConsumable && save.IsPurchased)
            {
                if (product.ProductType == ProductType.NonConsumable)
                {
                    supremeButton.Hide(true);
                    return;
                }
            }

            if (IsStarterPackRewardDisabled())
            {
                supremeButton.Hide(true);
                return;
            }

            supremeButton.Show();
        }

        private bool IsStarterPackRewardDisabled()
        {
            var starterPackItem = IAPManager.GetIAPItem(ProductKeyType.StarterPack);
            if (starterPackItem == null || starterPackItem.RewardsSet == null)
                return false;

            var rewards = starterPackItem.RewardsSet.Rewards;
            if (rewards.IsNullOrEmpty())
                return false;

            foreach (Reward reward in rewards)
            {
                if (reward != null && reward.CheckDisableState())
                    return true;
            }

            return false;
        }

        #endregion

        #region Button Handlers

        public void NoAdButton()
        {
            UIController.ShowPage<UINoAdsPopUp>();

            // UIController.ShowPage<UICharacterChoose>();
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void StoreButton()
        {
            // UIController.ShowPage<UIStore>();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void AddCoinsButton()
        {
            // UIController.ShowPage<UIStore>();
            if (is_AB_VideoIsB_Local)
            {
                ShowMenuStore(1);
            }
            else
            {
                UIController.ShowPage<UIStoreCoinDiamond>();
            }

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
        public void AddDiamondButton()
        {
            // UIController.ShowPage<UIStore>();
            if (is_AB_VideoIsB_Local)
            {
                ShowMenuStore(2);
            }
            else
            {
                UIController.ShowPage<UIStoreCoinDiamond>();
            }

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnSkinsStoreButtonClicked()
        {
            // UIController.ShowPage<UISkinStore>();
            // AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnSignInEntryButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            SevenDaySignInController.MarkAutoPopupShown();
            UIController.ShowPage<UISevenDaySignIn>();
        }

        private void OnPhotoButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            UIController.ShowPage<UIGameGirlsView>();
        }

        #endregion
        #region 自动弹出顺序控制

        // 做一个展示队列


        #endregion

        #region Sign In System

        private void StartSignInStateRefresh()
        {
            if (signInRefreshCoroutine != null)
            {
                StopCoroutine(signInRefreshCoroutine);
            }

            signInRefreshCoroutine = StartCoroutine(RefreshSignInStateCoroutine());
        }

        private IEnumerator RefreshSignInStateCoroutine()
        {
            yield return SevenDaySignInController.RefreshStateCoroutine(OnSignInStateRefreshed);
            signInRefreshCoroutine = null;
        }

        private void OnSignInStateRefreshed(SevenDaySignInState state)
        {
            if (state == null)
                return;

            if (SevenDaySignInController.ConsumeInitialAutoPopupSkip())
                return;

            if (SevenDaySignInController.ShouldAutoShowPopup() && !UIController.IsPopupOpened)
            {
                SevenDaySignInController.MarkAutoPopupShown();
                // UIController.ShowPage<UISevenDaySignIn>();
                UIPopupQueueManager.Instance.EnqueuePopup<UISevenDaySignIn>();
            }

            RedDotChecker.Instance.CheckDailyRedDot();
        }

        #endregion

        #region First Launch Logic

        private bool ShouldAutoStartFirstLevel()
        {
            LevelSave levelSave = SaveController.GetSaveObject<LevelSave>();
            if (levelSave == null)
                return false;

            return levelSave.MaxReachedLevelIndex == 0 &&
                   levelSave.DisplayLevelIndex == 0 &&
                   levelSave.RealLevelIndex == 0 &&
                   levelSave.CompletedLevelIndex == -1 &&
                   levelSave.LastPlayerLevelIndex == -1 &&
                   levelSave.FirstStart;
        }

        private IEnumerator AutoStartFirstLevelCoroutine()
        {
            firstLaunchAutoStartCoroutine = null;
            yield return null;
            MenuController.OnPlayButtonClicked();
        }

        #endregion

        #region Purchase Handling

        private void OnPurchaseComplete(ProductKeyType key)
        {
            ShowSupremeDealButton();
            HideAdButton(true);
            ShowAdButton();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (signInRefreshCoroutine != null)
            {
                StopCoroutine(signInRefreshCoroutine);
                signInRefreshCoroutine = null;
            }

            if (firstLaunchAutoStartCoroutine != null)
            {
                StopCoroutine(firstLaunchAutoStartCoroutine);
                firstLaunchAutoStartCoroutine = null;
            }

            // Unsubscribe from video events
            // if (VideoPlayCtrl.Instance != null)
            {
                videoPlayCtrl.SetDisplayTarget(null);
                videoPlayCtrl.OnVideoStarted -= OnVideoStarted;
                videoPlayCtrl.OnVideoError -= OnVideoError;
            }
        }

        #endregion

        #region Logging

        private void VideoLog(string message)
        {
            if (!ENABLE_VIDEO_LOGS) return;
            Debug.Log($"{VIDEO_LOG_TAG} {message}");
        }

        private void VideoLogWarning(string message)
        {
            if (!ENABLE_VIDEO_LOGS) return;
            Debug.LogWarning($"{VIDEO_LOG_TAG} {message}");
        }

        #endregion
    }
}
