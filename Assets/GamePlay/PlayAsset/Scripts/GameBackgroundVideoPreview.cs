using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Game.Video;
using VideoSystem;
using DG.Tweening;

namespace Watermelon
{
    public class GameBackgroundVideoPreview : MonoBehaviour
    {
        private const string VIDEO_CACHE_FOLDER_NAME = ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME;//"MenuBackgroundVideoCache";
        // private const string VIDEO_FILE_EXTENSION = ".mp4";
        private const int BACKGROUND_CANVAS_SORTING_ORDER = -100;

        private Canvas backgroundCanvas;
        private RawImage videoBackgroundImage;
        private Image videoBackgroundImage_Cover;
        private RawImage videoImageCover;
        private VideoPlayCtrl videoPlayCtrl;
        private Coroutine victoryPlaybackCoroutine;
        private bool isPrepared;
        private static GameBackgroundVideoPreview instance;

        public static void EnsureInitialized()
        {
            GameObject backgroundObject = null;
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                if (rootGameObjects[i].name == "Background")
                {
                    backgroundObject = rootGameObjects[i];
                    break;
                }
            }

            if (backgroundObject == null)
                return;

            GameBackgroundVideoPreview preview = backgroundObject.GetComponent<GameBackgroundVideoPreview>();
            if (preview == null)
            {
                preview = backgroundObject.AddComponent<GameBackgroundVideoPreview>();
            }

            preview.InitializeIfNeeded();
        }

        public static void PlayVictoryVideo(SimpleCallback onCompleted)
        {
            if (instance == null || !instance.isPrepared || instance.videoPlayCtrl == null)
            {
                onCompleted?.Invoke();
                return;
            }

            if (instance.victoryPlaybackCoroutine != null)
            {
                instance.StopCoroutine(instance.victoryPlaybackCoroutine);
            }

            instance.victoryPlaybackCoroutine = instance.StartCoroutine(instance.PlayVictoryVideoCoroutine(onCompleted));
        }

        public static void RefreshPlayNowVideo()
        {
            instance.PlayVideo();
        }

        public static void ShowBgVideo()
        {
            instance.ShowBgVideos();
        }

        private void Awake()
        {
            instance = this;
            InitializeIfNeeded();
        }

        private void OnEnable()
        {
            ApplyFullscreenLayout();
        }

        private void LateUpdate()
        {
            ApplyFullscreenLayout();
        }

        private void OnDisable()
        {
            if (victoryPlaybackCoroutine != null)
            {
                StopCoroutine(victoryPlaybackCoroutine);
                victoryPlaybackCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            if (videoPlayCtrl != null)
            {
                videoPlayCtrl.Dispose();
            }
        }

        private void InitializeIfNeeded()
        {
            if (videoPlayCtrl != null)
                return;

            Camera targetCamera = Camera.main;
            if (targetCamera == null)
                return;

            // Setup Canvas for video display
            Transform backgroundCanvasTransform = transform.Find("Video Background Canvas");
            if (backgroundCanvasTransform == null)
            {
                GameObject canvasObject = new GameObject("Video Background Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.layer = LayerMask.NameToLayer("Default");
                canvasObject.transform.SetParent(transform, false);
                backgroundCanvasTransform = canvasObject.transform;
            }

            backgroundCanvas = backgroundCanvasTransform.GetComponent<Canvas>();
            if (backgroundCanvas == null)
                return;

            backgroundCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            backgroundCanvas.worldCamera = targetCamera;
            backgroundCanvas.planeDistance = Mathf.Max(5.0f, targetCamera.farClipPlane - 1.0f);
            backgroundCanvas.overrideSorting = true;
            backgroundCanvas.sortingOrder = BACKGROUND_CANVAS_SORTING_ORDER;

            CanvasScaler canvasScaler = backgroundCanvasTransform.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1080.0f, 1920.0f);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            GraphicRaycaster graphicRaycaster = backgroundCanvasTransform.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = false;
            }

            // Setup RawImage for video display
            Transform videoSurfaceTransform = backgroundCanvasTransform.Find("Video Preview Surface");
            if (videoSurfaceTransform == null)
            {
                GameObject videoSurfaceObject = new GameObject("Video Preview Surface", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                videoSurfaceObject.layer = backgroundCanvasTransform.gameObject.layer;
                videoSurfaceObject.transform.SetParent(backgroundCanvasTransform, false);
                videoSurfaceTransform = videoSurfaceObject.transform;
            }

            videoBackgroundImage_Cover = backgroundCanvasTransform.Find("bgCover").GetComponent<Image>();
            videoImageCover = backgroundCanvasTransform.Find("videoCover").GetComponent<RawImage>();

            videoBackgroundImage = videoSurfaceTransform.GetComponent<RawImage>();
            if (videoBackgroundImage == null)
                return;

            videoBackgroundImage.raycastTarget = false;
            videoBackgroundImage.enabled = false;
            videoBackgroundImage.color = Color.white;

            ApplyFullscreenLayout();

            // Create VideoPlayCtrl instance
            GameObject videoControllerObject = new GameObject("Background Video Controller", typeof(VideoPlayCtrl));
            videoControllerObject.hideFlags = HideFlags.DontSave;
            videoControllerObject.transform.SetParent(transform, false);

            videoPlayCtrl = videoControllerObject.GetComponent<VideoPlayCtrl>();
            if (videoPlayCtrl != null)
            {
                videoPlayCtrl.SetDisplayTarget(videoBackgroundImage);
                // Subscribe to events
                videoPlayCtrl.OnVideoStarted += HandleVideoStarted;
                videoPlayCtrl.OnVideoError += HandleVideoError;
            }

            // Load video when initialized
            // StartCoroutine(LoadVideoCoroutine());
        }

        private void ApplyFullscreenLayout()
        {
            if (backgroundCanvas == null || videoBackgroundImage == null)
                return;

            Camera targetCamera = backgroundCanvas.worldCamera != null ? backgroundCanvas.worldCamera : Camera.main;
            if (targetCamera != null)
            {
                backgroundCanvas.worldCamera = targetCamera;
                backgroundCanvas.planeDistance = Mathf.Max(5.0f, targetCamera.farClipPlane - 1.0f);
            }

            RectTransform rectTransform = videoBackgroundImage.rectTransform;
            if (rectTransform == null)
                return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.SetAsFirstSibling();
        }

        protected void ShowBgVideos()
        {
            StartCoroutine(LoadVideoCoroutine());
        }

        private IEnumerator LoadVideoCoroutine()
        {
            InitializeIfNeeded();

            if (videoPlayCtrl == null || videoBackgroundImage == null)
            {
                yield break;
            }
            /// 这个可以直接先显示封面图，然后再加载视频，避免卡顿
            PlayVideo();
            yield return null;
        }

        private void PlayVideo()
        {
            string levelId = GetCurrentLevelId();
            if (levelId == null) return;

            if (GameController.is_AB_VideoIsB_Local)
            {
                videoImageCover.GetComponent<RawImgLoading>().LoadImage(levelId, () =>
                {
                    videoImageCover.DOFade(1, 0.3f);
                });
            }
            else
            {
                videoImageCover.SetAlpha(0);
            }


            videoPlayCtrl.OnVideoStarted += OnViewStarted;
            videoPlayCtrl.PlayLevelVideo(levelId, videoBackgroundImage);
            videoPlayCtrl.iaAutoPause = true;
        }

        private void OnViewStarted(string idx)
        {
            videoBackgroundImage_Cover.DOFade(0, 0.2f).OnComplete(delegate
            {
                videoPlayCtrl.OnVideoStarted -= OnViewStarted;
            });
        }

        private void HandleVideoStarted(string levelId)
        {
            isPrepared = true;
            if (videoBackgroundImage != null)
            {
                videoBackgroundImage.enabled = true;
            }
        }

        private void HandleVideoError(string levelId, string errorMessage)
        {
            isPrepared = false;
            Debug.LogError($"[GameBackgroundVideoPreview] Video error for level {levelId}: {errorMessage}");
        }

        /// <summary>
        /// Save current video frame region as gallery image
        /// </summary>
        private IEnumerator SaveCurrentVideoFrameAsGalleryImage(string levelId)
        {
            if (videoPlayCtrl == null || videoBackgroundImage == null)
            {
                Debug.LogError("[GameBackgroundVideoPreview] Video player or background image is null");
                yield break;
            }

            // Get the texture from the RawImage (which should have been set by VideoPlayCtrl)
            Texture sourceTexture = videoBackgroundImage.texture;
            if (sourceTexture == null)
            {
                Debug.LogError("[GameBackgroundVideoPreview] Video texture is null");
                yield break;
            }

            // Check if gallery image already exists
            string cacheDirectory = Path.Combine(Application.persistentDataPath, VIDEO_CACHE_FOLDER_NAME);
            string fileId = GetGalleryFileId(levelId);
            string imagePath = Path.Combine(cacheDirectory, fileId + ".png");

            if (File.Exists(imagePath))
            {
                Debug.Log($"[GameBackgroundVideoPreview] Gallery image already exists: {imagePath}");
                yield break;
            }

            // Wait one frame to ensure texture is updated
            yield return null;

            // Prepare textures and render texture
            RenderTexture renderTexture = null;
            Texture2D capturedTexture = null;
            Texture2D croppedTexture = null;

            try
            {
                int sourceWidth = sourceTexture.width;
                int sourceHeight = sourceTexture.height;

                // Ensure valid dimensions
                if (sourceWidth <= 0 || sourceHeight <= 0)
                {
                    sourceWidth = Screen.width;
                    sourceHeight = Screen.height;
                }

                renderTexture = new RenderTexture(sourceWidth, sourceHeight, 0, RenderTextureFormat.ARGB32);
                renderTexture.Create();

                // Copy current video frame to RenderTexture
                Graphics.Blit(sourceTexture, renderTexture);

                // Read pixels from RenderTexture
                capturedTexture = new Texture2D(sourceWidth, sourceHeight, TextureFormat.RGBA32, false);

                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = renderTexture;

                capturedTexture.ReadPixels(new Rect(0, 0, sourceWidth, sourceHeight), 0, 0);
                capturedTexture.Apply();

                RenderTexture.active = previousActive;

                // Calculate crop region (from bottom 0.55 to 0.94)
                // Note: Texture2D coordinate system has origin at bottom-left, Y increases upward
                float cropBottom = 0.55f;  // 55% from bottom
                float cropTop = 0.94f;     // 94% from bottom
                float cropHeightPercent = cropTop - cropBottom; // 0.39 = 39% height

                int cropY = Mathf.FloorToInt(sourceHeight * cropBottom);
                int cropHeight = Mathf.FloorToInt(sourceHeight * cropHeightPercent);

                // Safety checks
                if (cropY < 0) cropY = 0;
                if (cropY + cropHeight > sourceHeight)
                {
                    cropHeight = sourceHeight - cropY;
                }

                int cropWidth = sourceWidth;

                Debug.Log($"[GameBackgroundVideoPreview] Cropping region: Y={cropY}, Height={cropHeight}, Total Height={sourceHeight}");

                // Crop the image
                croppedTexture = new Texture2D(cropWidth, cropHeight, TextureFormat.RGBA32, false);
                Color[] pixels = capturedTexture.GetPixels(0, cropY, cropWidth, cropHeight);
                croppedTexture.SetPixels(pixels);
                croppedTexture.Apply();

                // Save cropped image
                if (!Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                }

                byte[] imageBytes = croppedTexture.EncodeToPNG();
                File.WriteAllBytes(imagePath, imageBytes);

                Debug.Log($"[GameBackgroundVideoPreview] Gallery image saved (cropped {cropBottom * 100}%-{cropTop * 100}%): {imagePath}, Size: {cropWidth}x{cropHeight}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameBackgroundVideoPreview] Failed to save gallery image: {ex.Message}");
            }
            finally
            {
                // Clean up resources
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    Destroy(renderTexture);
                }
                if (capturedTexture != null)
                {
                    Destroy(capturedTexture);
                }
                if (croppedTexture != null)
                {
                    Destroy(croppedTexture);
                }
            }
        }
        private string GetGalleryFileId(string levelId)
        {
            return ServerUtil.GetVideoFileName(levelId);//$"100{Mathf.Max(1, levelId):00}";
        }

        private IEnumerator PlayVictoryVideoCoroutine(SimpleCallback onCompleted)
        {
            // yield return StartCoroutine(SaveCurrentVideoFrameAsGalleryImage(GetCurrentLevelId()));

            GameObject levelObject = GameObject.Find("[LEVEL]");
            UIGame gamePage = UIController.GetPage<UIGame>();
            GameObject gamePageObject = gamePage != null ? gamePage.gameObject : null;

            if (levelObject != null)
            {
                levelObject.SetActive(false);
            }

            if (gamePageObject != null)
            {
                gamePageObject.SetActive(false);
            }

            if (videoPlayCtrl == null)
            {
                victoryPlaybackCoroutine = null;
                onCompleted?.Invoke();
                yield break;
            }

            // bool playbackFinished = false;

            // void HandleVideoComplete()
            // {
            //     playbackFinished = true;
            // }

            // void HandleVideoError(int levelId, string message)
            // {
            //     playbackFinished = true;
            // }

            // // Subscribe to completion events
            // videoPlayCtrl.OnVideoCompleted += (levelId) => HandleVideoComplete();
            // videoPlayCtrl.OnVideoError += HandleVideoError;

            videoBackgroundImage.enabled = true;

            // Play the victory video
            // int currentLevel = GetCurrentLevelId();
            // videoPlayCtrl.PlayLevelVideo(currentLevel, videoBackgroundImage);

            string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic);//"organic");

#if TEST_MODE
            // 如果视频, 就需要等待视频关闭
            // if (!DevPanelEnabler.IsDevForceToVideo)
            if (!DevPanelEnabler.IsDevForceToVideo && !FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#else
            // if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
            //     || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())
            if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#endif
            {
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                videoImageCover.DOFade(0, 0.1f);

                AudioController.PlaySound(AudioController.AudioClips.girls[UnityEngine.Random.Range(0, AudioController.AudioClips.girls.Length - 1)]);
                videoPlayCtrl.SetLooping(false);
                videoPlayCtrl.Resume();
                bool isWaitting = true;
                Action<string> finishAct = (idx) =>
                {
                    isWaitting = false;
                };
                Action<string, string> errorAct = (idx, message) =>
                {
                    isWaitting = false;
                };

                videoPlayCtrl.OnVideoCompleted += finishAct;
                videoPlayCtrl.OnVideoError += errorAct;

                float timeout = Time.realtimeSinceStartup + UnityEngine.Random.Range(3.0f, 10.0f);

                while (isWaitting && Time.realtimeSinceStartup < timeout)
                {
                    yield return null;
                }
                // Wait for playback to finish or timeout
                // float timeout = Time.realtimeSinceStartup + Mathf.Max(3.0f, 10.0f);
                // while (!playbackFinished && Time.realtimeSinceStartup < timeout)
                // {
                //     yield return null;
                // }
                videoPlayCtrl.OnVideoCompleted -= finishAct;
                videoPlayCtrl.OnVideoError -= errorAct;

                // videoPlayCtrl.Stop();
            }

            victoryPlaybackCoroutine = null;
            onCompleted?.Invoke();
        }

        private string GetCurrentLevelId()
        {
            if (!GameController.is_AB_VideoIsB_Local)
            {
                return "A";
            }

            ActiveSession activeSession = ActiveSession.Current;
            if (activeSession.IsPlaySpecialLevel())
            {
                var surpriseId = VideoSerilNumberManager.Instance.surpriseData.GetCurrentPlayingId();
                var specialId = VideoSerilNumberManager.Instance.specialData.GetCurrentPlayingId();

                if (surpriseId.mainId != null)
                {
                    string fullId = VideoSerilNumberManager.FormatSurpriseMainIdFileId(surpriseId.mainId, surpriseId.fileId);
                    return fullId;
                }
                else if (specialId.mainId != null)
                {
                    string fullId = VideoSerilNumberManager.FormatSurpriseMainIdFileId(specialId.mainId, specialId.fileId);
                    return fullId;
                }
                return null;
            }
            else
            {
                // 获取正在进行的视频
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
        }
    }
}
