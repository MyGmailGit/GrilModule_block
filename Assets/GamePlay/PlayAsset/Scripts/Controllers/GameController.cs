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
        private static SkinController skinController;
        private static RaycastController raycastController;
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
            gameObject.CacheComponent(out skinController);
            gameObject.CacheComponent(out raycastController);
            gameObject.CacheComponent(out puController);
            gameObject.CacheComponent(out tutorialController);

            // Initialize UI Controller to let other classes use UIController.GetPage method
            uiController.Init();

            GameBackgroundVideoPreview.EnsureInitialized();

            // Initialize other controlles
            particlesController.Init();
            floatingTextController.Init();
            skinController.Init();
            raycastController.Init();

            puController.Init();
            puController.InitBehaviors();

            levelController.Init(gameData.LevelDatabase);

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
            RaycastController.OnObjectTouched += OnObjectTouched;
        }

        private void OnDisable()
        {
            RaycastController.OnObjectTouched -= OnObjectTouched;
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

            // levelController.FinishScaleToHideBalls(() =>
            // {
            GameBackgroundVideoPreview.PlayVictoryVideo(() =>
            {

                UIController.ShowPage<UIFinishParticle>();

                FinishPop(currentIdFinishData);//currentIdFinishData.mainId, currentIdFinishData.fileId, currentIdFinishData.isSpecial);

                CollectNewFeatures();

                FeatureAnnouncementPopup.ShowAnnouncementIfExists();
            });
            // });
        }

        private static void CollectNewFeatures()
        {
            ActiveSession activeSession = ActiveSession.Current;
            int nextLevelIndex = activeSession.DisplayLevelIndex;

            // Collect PUs
            PUBehavior[] powerUps = PUController.ActivePowerUps;
            foreach (PUBehavior pu in powerUps)
            {
                PUSettings settings = pu.Settings;
                if (settings.RequiredLevel != 0 && !settings.IsUnlocked && (settings.RequiredLevel - 1) == nextLevelIndex)
                {
                    FeatureAnnouncementPopup.RegisterFeature(settings.AnnouncementPopupData);
                }
            }

            // Collect effects
            LevelData levelData = LevelController.LevelDatabase.GetLevel(nextLevelIndex);
            if (levelData != null)
            {
                LevelRemoteConfigData overrideData = RemoteConfigController.TryGetConfig<LevelRemoteConfigData>($"level{nextLevelIndex + 1}");
                if (overrideData != null)
                {
                    if (!string.IsNullOrEmpty(overrideData.hash))
                    {
                        levelData = levelData.DecompressLevel(overrideData.hash);
                    }
                }

                LevelElementData[] levelElements = levelData.LevelElements;
                foreach (LevelElementData levelElement in levelElements)
                {
                    if (levelElement.Type == ElementType.Block)
                    {
                        BlockEffectData[] blockEffects = levelElement.BlockEffects;
                        if (!blockEffects.IsNullOrEmpty())
                        {
                            foreach (BlockEffectData blockEffect in blockEffects)
                            {
                                LevelBlockEffectData effectData = LevelController.GetEffectData(blockEffect.Type);
                                if (effectData != null)
                                {
                                    if (!effectData.SaveData.IsAnnounced && effectData.AnnouncementPopupData.ShowAnnouncementPopup)
                                    {
                                        effectData.SaveData.IsAnnounced = true;

                                        FeatureAnnouncementPopup.RegisterFeature(effectData.AnnouncementPopupData);
                                    }
                                }
                            }
                        }
                    }
                    else if (levelElement.Type == ElementType.Gate)
                    {
                        GateEffectData[] gateEffects = levelElement.GateEffects;
                        if (!gateEffects.IsNullOrEmpty())
                        {
                            foreach (GateEffectData gateEffect in gateEffects)
                            {
                                LevelGateEffectData effectData = LevelController.GetEffectData(gateEffect.Type);
                                if (effectData != null)
                                {
                                    if (!effectData.SaveData.IsAnnounced && effectData.AnnouncementPopupData.ShowAnnouncementPopup)
                                    {
                                        effectData.SaveData.IsAnnounced = true;

                                        FeatureAnnouncementPopup.RegisterFeature(effectData.AnnouncementPopupData);
                                    }
                                }
                            }
                        }
                    }
                    else if (levelElement.Type == ElementType.InteractableObject)
                    {
                        if (levelElement.InteractableObjectData.Type != InteractableObjectType.None)
                        {
                            LevelInteractableObjectData interactableObjectData = LevelController.GetInteractableObject(levelElement.InteractableObjectData.Type);
                            if (interactableObjectData != null)
                            {
                                if (!interactableObjectData.SaveData.IsAnnounced && interactableObjectData.AnnouncementPopupData.ShowAnnouncementPopup)
                                {
                                    interactableObjectData.SaveData.IsAnnounced = true;

                                    FeatureAnnouncementPopup.RegisterFeature(interactableObjectData.AnnouncementPopupData);
                                }
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="failWinStreak">是否设置当前连胜失败</param>
        public static void ResetBallPlayPanel(bool failWinStreak)
        {
            // levelController.ResetBottlePlayPanel();
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
            LivesSystem.UnlockLife(true);

            ActiveSession currentSession = ActiveSession.Current;

            int levelIndex = currentSession.DisplayLevelIndex;

            LevelController.InvokeScenario(LevelScenario.LevelFailed);
        }

        public static void Replay(SimpleCallback onReplayCallback = null)
        {
            if (LivesSystem.Lives > 0 || LivesSystem.InfiniteMode)
            {
                onReplayCallback?.Invoke();

                LivesSystem.LockLife();

                Overlay.Show(0.3f, () =>
                {
                    Unload(() =>
                    {
                        LivesSystem.LockLife();

                        SceneManager.LoadScene(GameConsts.SCENE_GAME);
                    });
                });
            }
            else
            {
                UIAddLivesPanel.Show((lifeRecieved) =>
                {
                    if (lifeRecieved)
                    {
                        onReplayCallback?.Invoke();

                        Overlay.Show(0.3f, () =>
                        {
                            Unload(() =>
                            {
                                LivesSystem.LockLife();

                                SceneManager.LoadScene(GameConsts.SCENE_GAME);
                            });
                        });
                    }
                    else
                    {
                        MusicSource musicSource = MusicSource.ActiveMusicSource;
                        if (musicSource != null)
                            musicSource.Fade(0.2f, 0.3f);

                        Overlay.Show(0.3f, () =>
                        {
                            LoadMenu(() =>
                            {
                                if (musicSource != null)
                                    musicSource.Fade(1, 0.3f);
                            });
                        });
                    }
                });
            }
        }

        public static void OnCompleteRewardRecieved()
        {
            LivesSystem.UnlockLife(true);

            Overlay.Show(0.3f, () =>
            {
                Unload(() =>
                {
                    SceneManager.LoadScene(GameConsts.SCENE_MENU);
                });
            });
        }

        public static void LoadMenu(SimpleCallback unloadCallback = null)
        {
            LivesSystem.UnlockLife(true);

            Unload(() =>
            {
                // levelController.ResetBallSortModule();
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
