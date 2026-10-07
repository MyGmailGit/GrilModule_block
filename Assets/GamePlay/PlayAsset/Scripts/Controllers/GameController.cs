using GameUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VideoSystem;
using DG.Tweening;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] UIController uiController;

        private static ParticlesController particlesController;
        private static FloatingTextController floatingTextController;
        private static LevelController levelController;
        // private static SkinController skinController;
        // private static RaycastController raycastController;
        private static PUController puController;
        private static TutorialController tutorialController;

        private static bool isGameActivated;
        public static bool IsGameActivated => isGameActivated;

        private static bool isGameFinished;
        private static bool isRevived;

        private static Tween boostersTweenCase;
        private static Coroutine waitTweenCase;

        public static bool is_AB_VideoIsB_Local { get; private set; } = false;

        private void Awake()
        {
            GameData gameData = GameData.Data;
            if (gameData == null)
                Debug.LogError("GameData is null. Please add the Game Settings component to the Project Init Settings and link Game Data scriptable object.");


#if TEST_MODE
            // 如果设置强制走视频 ，就要修改哪些按钮不可见
            if (DevPanelEnabler.IsDevForceToVideo || FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#else
            if (FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
#endif
            {
                is_AB_VideoIsB_Local = true;
            }
            else
            {
                is_AB_VideoIsB_Local = false;
            }


            // Cache components
            gameObject.CacheComponent(out particlesController);
            gameObject.CacheComponent(out floatingTextController);
            gameObject.CacheComponent(out levelController);
            // gameObject.CacheComponent(out skinController);
            // gameObject.CacheComponent(out raycastController);
            gameObject.CacheComponent(out puController);
            gameObject.CacheComponent(out tutorialController);

            // Initialize UI Controller to let other classes use UIController.GetPage method
            uiController.Init();

            GameBackgroundVideoPreview.EnsureInitialized();

            // Initialize other controlles
            particlesController.Init();
            floatingTextController.Init();
            // skinController.Init();
            // raycastController.Init();

            puController.Init();
            puController.InitBehaviors();

            levelController.Init(gameData == null ? null : gameData.LevelDatabase);
            // levelController.Init();
            tutorialController.Init();

            // Initialize currency cloud and pages
            uiController.InitPages();

            // Mark game as inactive
            isGameActivated = false;
            isGameFinished = false;
            isRevived = false;

            AdsManager.EnableBanner();
        }

        private void OnEnable()
        {
            // RaycastController.OnObjectTouched += OnObjectTouched;
        }

        private void OnDisable()
        {
            // RaycastController.OnObjectTouched -= OnObjectTouched;
        }

        private void Start()
        {
            int levelIndex = ActiveSession.Current.DisplayLevelIndex;

            // DisplayLevelIndex is zero-based, while the video file id is one-based.
            // Preload the next level's video, so we need to advance both the index and the file-id space.
            // StartCoroutine(NextLevelVideoPreloader.PreloadNextLevelVideoCoroutine(this, levelIndex + 2));

            // Display default page
            UIController.ShowPage<UIGame>();

            PUController.OnLevelLoaded(levelIndex);

            waitTweenCase = UIController.Instance.WaitForPopupsClose(() =>
            {
                // Enable tutorials
                TutorialController.ActivateTutorial(TutorialController.GetTutorial(TutorialID.PUTools));
                // TutorialController.ActivateTutorial(TutorialController.GetTutorial(TutorialID.PUMagnet));
                // TutorialController.ActivateTutorial(TutorialController.GetTutorial(TutorialID.PUHammer));
                waitTweenCase = null;
            });

            CheckShowCharactorChoose();
        }

        private void OnDestroy()
        {
            boostersTweenCase.Kill();
            // waitTweenCase.Kill();
            if (waitTweenCase != null)
                StopCoroutine(waitTweenCase);

            // Reset time scale
            Time.timeScale = 1.0f;
        }

        private void CheckShowCharactorChoose()
        {
            ActiveSession activeSession = ActiveSession.Current;
            bool isSpecialPlay = activeSession.IsPlaySpecialLevel();

            if (is_AB_VideoIsB_Local)
            {
                if (!isSpecialPlay && VideoSerilNumberManager.Instance.GetCurrentProgress() == null)
                {
                    UICharacterChoose.ShowPage((idx) =>
                    {
                        if (idx >= 0)
                        {
                            // 开始加载下一个图片
                            GameBackgroundVideoPreview.ShowBgVideo();
                            UIController.GetPage<UIGame>().RefreshPicProgress();

                            PreloadMgr.Instance.PreloadCurrentCharaterStage();
                            // PreloadMgr.Instance.PreloadCharaterChoose();

                            UIController.GetPage<UIGame>().SetLabelShow();
                        }
                        else
                        {
                            // 如果不选就回到main menu
                            LoadMenu();
                        }
                    });
                }
                else
                {
                    GameBackgroundVideoPreview.ShowBgVideo();

                    if (!isSpecialPlay)
                    {
                        PreloadMgr.Instance.PreloadCurrentCharaterStage();
                        PreloadMgr.Instance.PreloadCharaterChoose();
                    }

                    UIController.GetPage<UIGame>().SetLabelShow();
                }
            }
            else
            {
                GameBackgroundVideoPreview.ShowBgVideo();

                UIController.GetPage<UIGame>().SetLabelShow();
            }
        }

        private static void FinishPop(VideoSerilNumberManager.CurrentFinishPicData finishPicData)//string mainId, int fileId, bool isSpecial)
        {
            if (finishPicData.isSpecial || !is_AB_VideoIsB_Local)
            {
                UIComplete.ShowPage(true);
            }
            else
            {
                MonthlyCtrl.Instance.ToCheckShouldInitMonthly();

                UIWinCountProgress.Show(() =>
                {
                    if (finishPicData.currentStageIsFinish)
                    {
                        UIGetSpecialPic.shouldShowSpecialGet = true;

                        UIPlayCompletePicStage.Show(finishPicData.mainId, finishPicData.startIdx, finishPicData.endIdx, () =>
                        {
                            UIComplete.ShowPage(true);
                        });
                    }
                    else
                    {
                        // 如果已经完成一个阶段，那么需要返回menu界面，然后展示specialImg
                        UIFinishGetImgShow.Show(finishPicData.mainId, VideoSerilNumberManager.FormatMainIdFileId(finishPicData.mainId, finishPicData.fileId), () =>
                        {
                            UIComplete.ShowPage(false);
                        });
                    }
                });
            }
        }

        public static void GameComplete()
        {
            if (isGameFinished)
                return;

            isGameFinished = true;

            levelController.HandleGameEnd();
            var currentIdFinishData = levelController.OnGameCompleted();

            PUController.OnLevelEnded();

            AudioController.PlaySound(AudioController.AudioClips.win);

            OnLevelCompleted();

            levelController.FinishScaleToHideBalls(() =>
            {
                GameBackgroundVideoPreview.PlayVictoryVideo(() =>
                {

                    UIController.ShowPage<UIFinishParticle>();

                    FinishPop(currentIdFinishData);//currentIdFinishData.mainId, currentIdFinishData.fileId, currentIdFinishData.isSpecial);

                    // CollectNewFeatures();

                    // FeatureAnnouncementPopup.ShowAnnouncementIfExists();
                });
            });
        }
        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="failWinStreak">是否设置当前连胜失败</param>
        public static void ResetBallPlayPanel(bool failWinStreak)
        {
            levelController.ResetBottlePlayPanel();
            if (failWinStreak)
            {
                MonthlyCtrl.Instance.OnLevelFail();
            }
        }
        public static bool ShouldCheckMonthlyWinStreak()
        {
            if (MonthlyCtrl.Instance.IsStartMonthlyTask())
            {
                ActiveSession activeSession = ActiveSession.Current;
                if (!activeSession.IsPlaySpecialLevel())
                {
                    return true;
                }
            }
            return false;
        }

        public static void GameOver(bool allowRevive, float uiDelay = 0.0f)
        {
            if (isGameFinished)
                return;

            if (isRevived)
                allowRevive = false;

            isGameFinished = true;

            levelController.OnLevelFailed();
            levelController.HandleGameEnd();

            PUController.OnLevelEnded();

            AudioController.PlaySound(AudioController.AudioClips.lose);

            if (uiDelay > 0)
            {

                DOVirtual.DelayedCall(uiDelay, () =>
                {
                    UIGameOver.Show(allowRevive);
                });
            }
            else
            {
                UIGameOver.Show(allowRevive);
            }
        }

        public static void Revive(int seconds)
        {
            if (!isGameFinished)
                return;

            isGameFinished = false;
            isRevived = true;

            levelController.OnRevived(seconds);

            AudioController.PlaySound(AudioController.AudioClips.revive);

            UIController.HidePage<UIGameOver>();
        }

        public static void OnLevelCompleted()
        {
            ActiveSession currentSession = ActiveSession.Current;
            // 游戏完成 设置下一关数据
            // 如果不是特殊关卡就直接设置下一关
            if (!currentSession.IsPlaySpecialLevel())
            {
                int levelIndex = currentSession.DisplayLevelIndex;
                currentSession.SetLevelIndex(levelIndex + 1);

                FirebaseAnalyticsModule.Instance.SendLevelEvent(currentSession.DisplayLevelIndex, FirebaseAnalyticsModule.EventLeveType.complete);
            }

            LevelController.InvokeScenario(LevelScenario.LevelCompleted);
        }

        public static void OnLevelFailed()
        {
            // LivesSystem.UnlockLife(true);

            ActiveSession currentSession = ActiveSession.Current;

            int levelIndex = currentSession.DisplayLevelIndex;

            LevelController.InvokeScenario(LevelScenario.LevelFailed);
        }

        public static void OnCompleteRewardRecieved()
        {
            Overlay.Show(0.3f, () =>
            {
                Unload(() =>
                {
                    levelController.ResetBallSortModule();

                    SceneManager.LoadScene(GameConsts.SCENE_MENU);
                });
            });
        }

        public static void LoadMenu(SimpleCallback unloadCallback = null)
        {
            Unload(() =>
            {
                levelController.ResetBallSortModule();
                SceneManager.LoadScene(GameConsts.SCENE_MENU);

                unloadCallback?.Invoke();
            });
        }

        public static void Unload(SimpleCallback onUnloaded)
        {
            GameData gameData = GameData.Data;

            PUController.OnLevelEnded();

            SaveController.Save();

            levelController.UnloadLevel();

            AdsManager.ShowInterstitial((result) =>
            {
                onUnloaded?.Invoke();
            }, AnalyticsStr.inter_level_finish);//"Complete");
        }

        private void OnObjectTouched()
        {
            ActivateGame();

            RaycastController.OnObjectTouched -= OnObjectTouched;
        }

        public static void ActivateGame()
        {
            if (isGameActivated)
                return;

            isGameActivated = true;

            levelController.OnGameActivated();
        }
    }
}
