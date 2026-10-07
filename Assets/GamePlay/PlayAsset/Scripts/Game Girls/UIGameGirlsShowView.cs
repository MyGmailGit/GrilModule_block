using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Game.Video;
using Unity.VisualScripting;
using DG.Tweening;

namespace Watermelon
{
    public class UIGameGirlsShowView : UIPage, IPopupWindow//, IPausePopup
    {
        private const string VIDEO_CACHE_FOLDER_NAME = ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME;//"MenuBackgroundVideoCache";
        // private const string VIDEO_FILE_EXTENSION = ServerUtil.MENU_VIDEO_FILE_EXTENSION;//".mp4";
        private const string LOCAL_VIDEO_UNITY_FOLDER_PATH = ServerUtil.MENU_VIDEO_UNITY_FOLDER_PATH;

        private const int COIN_DOWNLOAD_COST = 500;

        [SerializeField] private RectTransform contentPrefab;
        [SerializeField] private RectTransform contentRoot;

        private static PendingSelection pendingSelection;
        private RectTransform contentInstance;
        private CanvasGroup contentInstanceCanvasGroup;
        private Button closeButton;
        private Button downloadButton;
        private Button downloadAdButton;
        private Button coinDownloadButton;
        private Button useButton;
        private Button vipDownloadButton;
        private Toggle likeToggle;
        private GameObject useSelectedStateObject;
        private GameObject logoHintObject;
        private GameObject tipsHintObject;
        private Image primaryPreviewImage;
        private Image secondaryPreviewImage;
        private RawImage primaryVideoImage;
        private RawImage secondaryVideoImage;
        private UnityEngine.UI.Text textDiamondNum;
        private VideoPlayCtrl videoPlayCtrl;
        private Coroutine playVideoCoroutine;
        private string currentVideoPath;
        private bool suppressLikeToggleCallback;
        private bool isDownloadInProgress;

        private Color defaultdisplay = Color.white;

        public bool IsOpened => canvas.enabled;

        private static Action closeCallback = null;

        public static void ShowForItem(string levelId, string fileId, Action callback = null)
        {
            closeCallback = callback;

            pendingSelection = new PendingSelection(levelId, fileId);
            UIController.ShowPage<UIGameGirlsShowView>();
        }

        public override void Init()
        {
            EnsureContentInstance();
            ResolveReferences();
            InitializeVideoPlayer();
            BindButtons();
            PrepareStaticLayout();
        }

        public override void PlayShowAnimation()
        {
            if (contentRoot != null)
            {
                contentRoot.anchoredPosition = Vector2.zero;
                contentRoot.localScale = Vector3.one;
            }

            ApplyPendingSelection();

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            StopVideoPlayback();
            primaryVideoImage.GetOrAddComponent<RawImgLoading>().Release();
            UIController.OnPageClosed(this);

            closeCallback?.Invoke();
        }

        private void OnDestroy()
        {
            StopVideoPlayback();

            if (videoPlayCtrl != null)
                videoPlayCtrl.Dispose();
        }

        private void EnsureContentInstance()
        {
            if (contentInstance != null || contentPrefab == null)
                return;

            Transform parent = contentRoot != null ? contentRoot : transform;
            contentInstance = Instantiate(contentPrefab, parent, false);
            contentInstance.name = contentPrefab.name;

            contentInstance.anchorMin = Vector2.zero;
            contentInstance.anchorMax = Vector2.one;
            contentInstance.offsetMin = Vector2.zero;
            contentInstance.offsetMax = Vector2.zero;
            contentInstance.anchoredPosition = Vector2.zero;
            contentInstance.localScale = Vector3.one;

            contentInstanceCanvasGroup = contentInstance.GetComponent<CanvasGroup>();
            contentInstanceCanvasGroup.alpha = 0;
        }

        private void ResolveReferences()
        {
            Transform root = contentInstance != null ? contentInstance : transform;

            if (closeButton == null)
                closeButton = FindDeepComponent<Button>(root, "ButtonHideGirls#Button");

            if (downloadButton == null)
                downloadButton = FindDeepComponent<Button>(root, "ButtonDown#Button");

            if (downloadAdButton == null)
                downloadAdButton = FindDeepComponent<Button>(root, "ButtonDownAD#Button");

            if (coinDownloadButton == null)
                coinDownloadButton = FindDeepComponent<Button>(root, "ButtonDownDiamond#Button");

            if (vipDownloadButton == null)
                vipDownloadButton = FindDeepComponent<Button>(root, "VipButtonDown#Button");

            if (textDiamondNum == null)
                textDiamondNum = FindDeepComponent<UnityEngine.UI.Text>(root, "TextDiamondNum#Text");

            textDiamondNum.text = COIN_DOWNLOAD_COST.ToString();

            if (useButton == null)
                useButton = FindDeepComponent<Button>(root, "ButtonUse#Button");

            if (likeToggle == null)
                likeToggle = FindDeepComponent<Toggle>(root, "ToggleLike#Toggle");

            if (useSelectedStateObject == null)
            {
                Transform selectedTransform = FindChildRecursive(useButton != null ? useButton.transform : root, "ImageSelect#Button");
                if (selectedTransform != null)
                    useSelectedStateObject = selectedTransform.gameObject;
            }

            if (logoHintObject == null)
            {
                Transform logoTransform = FindChildRecursive(root, "ImageLogo#GameObject");
                if (logoTransform != null)
                    logoHintObject = logoTransform.gameObject;
            }

            if (tipsHintObject == null)
            {
                Transform tipsTransform = FindChildRecursive(root, "TextTips#GameObject");
                if (tipsTransform != null)
                    tipsHintObject = tipsTransform.gameObject;
            }

            RectTransform primaryVideoRoot = FindChildRecursive(root, "ImageGirls1") as RectTransform;
            RectTransform secondaryVideoRoot = FindChildRecursive(root, "ImageGirls2") as RectTransform;

            if (primaryPreviewImage == null)
                primaryPreviewImage = FindDeepComponent<Image>(root, "ImageGirls1#Image");

            if (secondaryPreviewImage == null)
                secondaryPreviewImage = FindDeepComponent<Image>(root, "ImageGirls2#Image");

            if (primaryVideoRoot == null && primaryPreviewImage != null)
                primaryVideoRoot = primaryPreviewImage.transform.parent as RectTransform;

            if (secondaryVideoRoot == null && secondaryPreviewImage != null)
                secondaryVideoRoot = secondaryPreviewImage.transform.parent as RectTransform;

            if (primaryVideoRoot != null && primaryVideoImage == null)
                primaryVideoImage = GetOrCreateVideoSurface(primaryVideoRoot, "Primary Video Surface");

            defaultdisplay.a = 0;
            primaryVideoImage.color = defaultdisplay;

            if (secondaryVideoRoot != null && secondaryVideoImage == null)
                secondaryVideoImage = GetOrCreateVideoSurface(secondaryVideoRoot, "Secondary Video Surface");
        }

        private void BindButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            if (downloadButton != null)
                downloadButton.onClick.RemoveAllListeners();

            if (downloadAdButton != null)
                downloadAdButton.onClick.RemoveAllListeners();

            if (coinDownloadButton != null)
                coinDownloadButton.onClick.RemoveAllListeners();

            if (useButton != null)
                useButton.onClick.RemoveAllListeners();

            if (vipDownloadButton != null)
                vipDownloadButton.onClick.RemoveAllListeners();

            if (downloadButton != null)
                downloadButton.onClick.RemoveListener(OnDownloadButtonClicked);

            if (useButton != null)
                useButton.onClick.RemoveListener(OnUseButtonClicked);

            if (likeToggle != null)
            {
                likeToggle.onValueChanged.RemoveListener(OnLikeToggleValueChanged);
                likeToggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
            }

            if (downloadButton != null)
                downloadButton.onClick.AddListener(OnDownloadButtonClicked);

            downloadAdButton?.onClick.AddListener(OnDownloadButtonClicked);
            coinDownloadButton?.onClick.AddListener(OnCoinButtonClicked);

            vipDownloadButton?.onClick.AddListener(OnVipDownloadButtonClicked);

            if (useButton != null)
                useButton.onClick.AddListener(OnUseButtonClicked);
        }

        private void PrepareStaticLayout()
        {
            var ideoRewardSave = SaveController.GetSaveObject<SimpleIntSave>(SevenDaySignInController.VIDEO_REWARD_SAVE_ID);

            if (IAPManager.IsSubscribed())// || (pendingSelection != null && pendingSelection.LevelId <= ideoRewardSave.Value))
            // IAPManager.IsSubscribed(ProductKeyType.SubscriptionMonthly) ||
            // IAPManager.IsSubscribed(ProductKeyType.SubscriptionYearly))
            {
                SetNodeActive("ButtonDownAD#Button", false);
                SetNodeActive("ImageADIcon#GameObject", false);
                SetNodeActive("ButtonDownDiamond#Button", false);
                SetNodeActive("VipButtonDown#Button", true);
            }
            else
            {
                SetNodeActive("ButtonDownAD#Button", true);
                SetNodeActive("ImageADIcon#GameObject", true);
                SetNodeActive("ButtonDownDiamond#Button", true);
                SetNodeActive("ButtonDown#Button", false);
                SetNodeActive("VipButtonDown#Button", false);
            }
            SetNodeActive("ButtonUse#Button", true);
            SetNodeActive("ImageSelect#Button", false);
            SetNodeActive("ToggleUnLuck#Toggle", false);
            SetNodeActive("ToggleLike#Toggle", true);
            SetNodeActive("ButtonLeft#Button", false);
            SetNodeActive("ButtonRight#Button", false);
        }

        private void ApplyPendingSelection()
        {
            if (pendingSelection == null)
                return;

            HideStaticPreviewImages();
            ApplyLikeToggleState();
            ApplyDownloadHintState();
            ApplyUseButtonState();

            // string videoPath = pendingSelection.GetVideoPath();
            StartVideoPlayback();//videoPath);
        }

        private void HideStaticPreviewImages()
        {
            if (primaryPreviewImage != null)
                primaryPreviewImage.enabled = false;

            if (secondaryPreviewImage != null)
                secondaryPreviewImage.enabled = false;
        }

        private void SetNodeActive(string nodeName, bool isActive)
        {
            Transform node = FindChildRecursive(contentInstance != null ? contentInstance : transform, nodeName);
            if (node != null)
                node.gameObject.SetActive(isActive);
        }

        private void InitializeVideoPlayer()
        {
            if (videoPlayCtrl != null)
                return;

            GameObject videoControllerObject = new GameObject("Game Girls Video Controller", typeof(VideoPlayCtrl));
            videoControllerObject.hideFlags = HideFlags.DontSave;
            videoControllerObject.transform.SetParent(transform, false);

            videoPlayCtrl = videoControllerObject.GetComponent<VideoPlayCtrl>();
            if (videoPlayCtrl != null)
            {
                videoPlayCtrl.OnVideoStarted += OnViewStarted;
                // Set primary video surface as display target
                if (primaryVideoImage != null)
                    videoPlayCtrl.SetDisplayTarget(primaryVideoImage);
            }
        }

        private RawImage GetOrCreateVideoSurface(RectTransform parent, string surfaceName)
        {
            if (parent == null)
                return null;

            Transform existing = parent.Find(surfaceName);
            RectTransform surfaceTransform = existing as RectTransform;
            if (surfaceTransform == null)
            {
                GameObject surfaceObject = new GameObject(surfaceName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                surfaceObject.layer = parent.gameObject.layer;
                surfaceObject.transform.SetParent(parent, false);
                surfaceTransform = surfaceObject.GetComponent<RectTransform>();
            }

            surfaceTransform.anchorMin = Vector2.zero;
            surfaceTransform.anchorMax = Vector2.one;
            surfaceTransform.offsetMin = Vector2.zero;
            surfaceTransform.offsetMax = Vector2.zero;
            surfaceTransform.localScale = Vector3.one;
            surfaceTransform.localRotation = Quaternion.identity;
            surfaceTransform.SetAsLastSibling();

            RawImage rawImage = surfaceTransform.GetComponent<RawImage>();
            rawImage.raycastTarget = false;
            rawImage.color = Color.white;
            rawImage.enabled = false;

            return rawImage;
        }

        private void StartVideoPlayback()//string videoPath)
        {
            StopVideoPlayback();
            // currentVideoPath = videoPath;

            // if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
            // {
            //     SetVideoSurfacesVisible(false);
            //     return;
            // }

            playVideoCoroutine = StartCoroutine(PlayVideoCoroutine());
        }

        private void StopVideoPlayback()
        {
            currentVideoPath = null;

            if (playVideoCoroutine != null)
            {
                StopCoroutine(playVideoCoroutine);
                playVideoCoroutine = null;
            }

            if (videoPlayCtrl != null)
                videoPlayCtrl.Stop();

            SetVideoSurfacesVisible(false);
        }

        private IEnumerator PlayVideoCoroutine()//string videoPath)
        {
            if (videoPlayCtrl == null)
                yield break;

            contentInstanceCanvasGroup.alpha = 0;
            // For UIGameGirlsShowView, we need to extract levelId from videoPath or use a default
            // Since we don't have levelId, we'll use 1 as default (VideoPlayCtrl can handle this)
            // int levelId = GetLevelIdFromPath(videoPath);
            primaryVideoImage.color = defaultdisplay;
            // Set both video surfaces as targets
            videoPlayCtrl.SetDisplayTarget(primaryVideoImage);

            primaryVideoImage.GetOrAddComponent<RawImgLoading>().LoadImage(pendingSelection.LevelId, () =>
            {
                // primaryVideoImage.enabled = true;
                SetVideoSurfacesVisible(true);
                primaryVideoImage.DOFade(1, 0.1f);
            });


            videoPlayCtrl.PlayLevelVideo(pendingSelection.LevelId, primaryVideoImage);

            // Wait a bit for the video to start
            // yield return new WaitForSecondsRealtime(0.5f);

            contentInstanceCanvasGroup.DOFade(1.0f, 0.3f);//, 0, true, UpdateMethod.Update);

            // Check if video is prepared and bind secondary surface
            if (videoPlayCtrl.IsPrepared && primaryVideoImage != null && primaryVideoImage.texture != null)
            {
                SetVideoSurfacesVisible(true);

                // Bind texture to secondary surface if it exists
                if (secondaryVideoImage != null)
                    secondaryVideoImage.texture = primaryVideoImage.texture;
            }
            else
            {
                // SetVideoSurfacesVisible(false);
            }

            playVideoCoroutine = null;
        }

        private void OnViewStarted(string obj)
        {
            primaryVideoImage.DOFade(1, 0.2f).OnComplete(delegate
            {
                // videoPlayCtrl.OnVideoStarted -= OnViewStarted;
            });
        }



        // private int GetLevelIdFromPath(string videoPath)
        // {
        //     // Extract levelId from file name (e.g., "10001.mp4" -> 1, "10002.mp4" -> 2)
        //     try
        //     {
        //         string fileName = Path.GetFileNameWithoutExtension(videoPath);
        //         if (fileName.Length >= 3 && int.TryParse(fileName.Substring(3), out int levelId))
        //             return Mathf.Max(1, levelId);
        //     }
        //     catch { }

        //     return 1; // Default to level 1
        // }



        private void SetVideoSurfacesVisible(bool isVisible)
        {
            if (primaryVideoImage != null)
                primaryVideoImage.enabled = isVisible;

            if (secondaryVideoImage != null)
                secondaryVideoImage.enabled = isVisible;
        }

        private void OnCloseButtonClicked()
        {
            if (IsPageDisplayed)
                UIController.HidePage(this);
        }

        private void OnLikeToggleValueChanged(bool isOn)
        {
            if (suppressLikeToggleCallback || pendingSelection == null)
                return;

            GameGirlsLikeController.SetLiked(pendingSelection.FileId, isOn);
        }

        private void OnDownloadButtonClicked()
        {
            if (pendingSelection == null || isDownloadInProgress)
                return;

            isDownloadInProgress = true;
            SetDownloadButtonInteractable(false);

            AdsManager.ShowRewardBasedVideo((success) =>
            {
                if (!success)
                {
                    isDownloadInProgress = false;
                    SetDownloadButtonInteractable(true);
                    return;
                }

                // string videoPath = pendingSelection.GetVideoPath();
                string videoPath = PendingSelection.GetVideoPath(pendingSelection.FileId);
                bool saved = GameGirlsVideoSaveUtility.TrySaveVideoToDevice(videoPath, pendingSelection.LevelId, pendingSelection.FileId, out string message);

                if (saved)
                {
                    GameGirlsDownloadController.MarkAsDownloaded(pendingSelection.FileId);
                    ApplyDownloadHintState();
                }

                SystemMessage.ShowMessage(message);

                isDownloadInProgress = false;
                SetDownloadButtonInteractable(true);
            }, AnalyticsStr.reward_game_girls_download);
        }

        private void OnVipDownloadButtonClicked()
        {
            if (pendingSelection == null || isDownloadInProgress)
                return;

            isDownloadInProgress = true;
            // SetDownloadButtonInteractable(false);
            vipDownloadButton.interactable = false;


            // string videoPath = pendingSelection.GetVideoPath();
            string videoPath = PendingSelection.GetVideoPath(pendingSelection.FileId);
            bool saved = GameGirlsVideoSaveUtility.TrySaveVideoToDevice(videoPath, pendingSelection.LevelId, pendingSelection.FileId, out string message);

            if (saved)
            {
                GameGirlsDownloadController.MarkAsDownloaded(pendingSelection.FileId);
                ApplyDownloadHintState();
            }

            SystemMessage.ShowMessage(message);

            isDownloadInProgress = false;
            // SetDownloadButtonInteractable(true);
            vipDownloadButton.interactable = true;
        }

        private void OnCoinButtonClicked()
        {
            if (CurrencyController.HasAmount(CurrencyType.Coins, COIN_DOWNLOAD_COST))
            {
                CurrencyController.Substract(CurrencyType.Coins, COIN_DOWNLOAD_COST, "download");

                string videoPath = PendingSelection.GetVideoPath(pendingSelection.FileId);
                bool saved = GameGirlsVideoSaveUtility.TrySaveVideoToDevice(videoPath, pendingSelection.LevelId, pendingSelection.FileId, out string message);

                if (saved)
                {
                    GameGirlsDownloadController.MarkAsDownloaded(pendingSelection.FileId);
                    ApplyDownloadHintState();
                }

                SystemMessage.ShowMessage(message);

                isDownloadInProgress = false;
                SetDownloadButtonInteractable(true);
            }
            else
            {
                // UIController.ShowPage<IAPStore.UIStore>();
            }

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnUseButtonClicked()
        {
            if (pendingSelection == null)
                return;

            if (GameGirlsHomeVideoController.SetSelectedFileId(pendingSelection.LevelId.ToString()))
            {
                ApplyUseButtonState();
                SystemMessage.ShowMessage("Video set successfully.");
            }
        }

        private void ApplyLikeToggleState()
        {
            if (likeToggle == null || pendingSelection == null)
                return;

            suppressLikeToggleCallback = true;
            likeToggle.SetIsOnWithoutNotify(GameGirlsLikeController.IsLiked(pendingSelection.FileId));
            suppressLikeToggleCallback = false;
        }

        private void ApplyDownloadHintState()
        {
            if (pendingSelection == null)
                return;

            bool shouldShowHint = GameGirlsDownloadController.ShouldShowDownloadHint(pendingSelection.FileId);

            if (logoHintObject != null)
                logoHintObject.SetActive(shouldShowHint);

            if (tipsHintObject != null)
                tipsHintObject.SetActive(shouldShowHint);
        }

        private void SetDownloadButtonInteractable(bool isInteractable)
        {
            if (downloadButton != null)
                downloadButton.interactable = isInteractable;

            if (downloadAdButton != null)
                downloadAdButton.interactable = isInteractable;
        }

        private void ApplyUseButtonState()
        {
            bool isSelected = pendingSelection != null && GameGirlsHomeVideoController.IsSelected(pendingSelection.LevelId.ToString());

            if (useSelectedStateObject != null)
                useSelectedStateObject.SetActive(isSelected);

            if (useButton != null)
                useButton.interactable = !isSelected;
        }

        private T FindDeepComponent<T>(Transform root, string objectName) where T : Component
        {
            Transform child = FindChildRecursive(root, objectName);
            if (child == null)
                return null;

            return child.GetComponent<T>();
        }

        private Transform FindChildRecursive(Transform parent, string objectName)
        {
            if (parent == null)
                return null;

            if (parent.name == objectName)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindChildRecursive(parent.GetChild(i), objectName);
                if (result != null)
                    return result;
            }

            return null;
        }

        public sealed class PendingSelection
        {
            private readonly string levelId;
            public string LevelId { get { return levelId; } }
            private readonly string fileId;

            public PendingSelection(string levelId, string fileId)
            {
                this.levelId = levelId;
                this.fileId = fileId;
            }

            public string FileId => string.IsNullOrEmpty(fileId)
                ? ServerUtil.GetVideoFileName(levelId)//$"100{Mathf.Max(1, levelId):00}"
                : fileId;

            public static string GetVideoPath(string FileId)
            {
                string resolvedFileId = FileId;

                string cachedPath = Path.Combine(Application.persistentDataPath, VIDEO_CACHE_FOLDER_NAME, resolvedFileId);
                if (File.Exists(cachedPath))
                    return cachedPath;

                string unityRelativePath = Path.Combine(LOCAL_VIDEO_UNITY_FOLDER_PATH, resolvedFileId);
                const string streamingAssetsPrefix = "Assets/StreamingAssets";

                if (unityRelativePath.StartsWith(streamingAssetsPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    string relativePath = unityRelativePath.Substring(streamingAssetsPrefix.Length).TrimStart('/', '\\');
                    string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, relativePath);
                    if (File.Exists(streamingAssetsPath))
                        return streamingAssetsPath;
                }

                return null;
            }
        }

        private string ConvertToVideoUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return $"file://{path}";
        }
    }
}
