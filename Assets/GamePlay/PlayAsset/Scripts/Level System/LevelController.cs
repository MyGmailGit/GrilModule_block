using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using VideoSystem;

namespace Watermelon
{
    [StaticUnload]
    public class LevelController : MonoBehaviour
    {
        public static readonly Ease BLOCK_MOVE_EASE_TYPE = Ease.InQuad;
        public static readonly float BLOCK_MOVE_DURATION = 0.25f;

        [SerializeField] BottlesController bottlesCtrlPrefab;

        static BottlesController bottlesController = null;

        // [SerializeField] EnvironmentData environmentData;

        private static LevelController levelController;

        // private static LevelSkinData skinData;

        private static LevelDatabase levelDatabase;
        public static LevelDatabase LevelDatabase => levelDatabase;

        public static bool BallCoverEnabled { get; private set; } = false;

        private static bool IsBallCoverLevelType(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Hard:
                    return true;
                case LevelType.Normal:
                default:
                    return false;
            }
        }

        // public static LevelRepresentation LevelRepresentation { get; private set; }
        public static bool IsLevelLoaded { get; private set; } = false;

        // private static BlockMovementManager movementManager;

        // public static GameplayTimer GameplayTimer { get; private set; }

        public static event SimpleCallback LevelLoaded;
        public static event LevelScenarioDelegate LevelScenarioChanged;


        private struct MoveStep
        {
            public BottleController from;
            public BottleController to;
            public int ballNum;
        }
        private Stack<MoveStep> moveSteps = new Stack<MoveStep>();

        private LevelData currentLevelData;


        public void Init(LevelDatabase levelDatabase = null)
        {
            levelController = this;

            if (bottlesController == null)
            {
                bottlesController = Instantiate(bottlesCtrlPrefab, Vector3.zero, Quaternion.identity);
                // bottlesController = bottle;
            }

            if (levelDatabase == null)
                Debug.LogError("Level database is not set. Please check the Game Data scriptable object.");

            LevelController.levelDatabase = levelDatabase;

            levelDatabase.Init();

            // movementManager = new BlockMovementManager();

            // GameplayTimer = new GameplayTimer();
            // GameplayTimer.OnTimerFinished += OnGameplayTimerFinished;

            IsLevelLoaded = false;

            // skinData = (LevelSkinData)SkinController.Instance.GetSelectedSkin<LevelSkinDatabase>();
            // skinData.BlocksVisualsData.Init();

            ActiveSession activeSession = ActiveSession.Current;

            if (activeSession.IsPlaySpecialLevel())
            {
                int levelIndex = activeSession.GetSpecialLevelIndex();
                LoadSpecialLevel(levelIndex, levelIndex);
            }
            else
            {
                int displayedLevelIndex = activeSession.DisplayLevelIndex;
                int levelIndex = activeSession.GetLevelIndex(displayedLevelIndex);

                LoadLevel(displayedLevelIndex, levelIndex, activeSession.FirstStart);
            }
        }

        public void RecordMoveStep(BottleController fromArg, BottleController toArg, int ballNumArg)
        {
            moveSteps.Push(new LevelController.MoveStep() { from = fromArg, to = toArg, ballNum = ballNumArg });
        }

        public bool PullBackAStep()
        {
            if (moveSteps.Count > 0 && !bottlesController.IsPlayingMove())
            {
                var oneStep = moveSteps.Pop();

                bottlesController.PullBackOneStep(oneStep.from, oneStep.to, oneStep.ballNum);

                return true;
            }
            return false;
        }

        public bool AddABottle()
        {
            return bottlesController.AddABottle();
        }

        public bool SuggestOneStep()
        {
            return bottlesController.SuggestOneStep();
        }


        public void HandleGameEnd()
        {
            // RaycastController.Disable();

            // OnObjectReleased();

            // AudioController.PlaySound(AudioController.AudioClips.blockPick);

            // GameplayTimer.Pause();

            // List<LevelBlockBehavior> blocks = LevelRepresentation.ActiveBlocks;
            // foreach (LevelBlockBehavior block in blocks)
            // {
            //     if (block.HasActiveEffect())
            //     {
            //         List<BlockEffectBehavior> effects = block.Effects;
            //         foreach (BlockEffectBehavior effect in effects)
            //         {
            //             effect.OnGameEnded();
            //         }
            //     }
            // }
        }
        /// <summary>
        /// 主id， 当前文件id， 是否是surprise或者special，当前stage开始的id和结束的id
        /// </summary>
        /// <returns></returns>
        public VideoSerilNumberManager.CurrentFinishPicData OnGameCompleted()
        {
            if (!GameController.is_AB_VideoIsB_Local)
            {
                return new VideoSerilNumberManager.CurrentFinishPicData();
            }

            ActiveSession activeSession = ActiveSession.Current;
            activeSession.OnLevelCompleted();

            SaveController.MarkAsSaveIsRequired();

            if (activeSession.IsPlaySpecialLevel())
            {
                var surpriseid = VideoSerilNumberManager.Instance.surpriseData.GetCurrentPlayingId();
                var specialid = VideoSerilNumberManager.Instance.specialData.GetCurrentPlayingId();

                VideoSerilNumberManager.Instance.surpriseData.CompleteCurrentPlaying();
                VideoSerilNumberManager.Instance.specialData.CompleteCurrentPlaying();

                if (surpriseid.mainId != null)
                {
                    return new VideoSerilNumberManager.CurrentFinishPicData() { mainId = surpriseid.mainId, fileId = surpriseid.fileId, isSpecial = true, startIdx = -1, endIdx = -1, currentStageIsFinish = false };
                }
                else// if (specialid.mainId != null)
                {
                    return new VideoSerilNumberManager.CurrentFinishPicData() { mainId = specialid.mainId, fileId = specialid.fileId, isSpecial = true, startIdx = -1, endIdx = -1, currentStageIsFinish = false };
                    // return (specialid.mainId, specialid.fileId, true, -1, -1);
                }
            }
            else
            {
                MonthlyCtrl.Instance.OnLevelWin();

                var curpro = VideoSerilNumberManager.Instance.GetCurrentFullName();

                var stageStartEnd = VideoSerilNumberManager.Instance.GetCurrentStageStartEnd();

                VideoSerilNumberManager.Instance.CompleteCurrentImageAndGetNext();

                var currentStageFinishData = VideoSerilNumberManager.Instance.GetCurrentStageProgress();

                bool isFinish = currentStageFinishData.usedCount >= currentStageFinishData.totalCount;

                return new VideoSerilNumberManager.CurrentFinishPicData()
                {
                    mainId = curpro.Value.mainId,
                    fileId = curpro.Value.fileId,
                    isSpecial = false,
                    startIdx = stageStartEnd.Value.startIdx,
                    endIdx = stageStartEnd.Value.endIdx,
                    currentStageIsFinish = isFinish,
                };
            }
        }

        public void OnRevived(int seconds)
        {
            // GameplayTimer.AdjustTime(seconds);
            // GameplayTimer.Resume();

            // RaycastController.Enable();

            // List<LevelBlockBehavior> blocks = LevelRepresentation.ActiveBlocks;
            // foreach (LevelBlockBehavior block in blocks)
            // {
            //     if (block.HasActiveEffect())
            //     {
            //         List<BlockEffectBehavior> effects = block.Effects;
            //         foreach (BlockEffectBehavior effect in effects)
            //         {
            //             effect.OnRevived();
            //         }
            //     }
            // }
        }

        // private void FixedUpdate()
        // {
        //     MovementUpdate();
        // }

        public void LoadLevel(int displayedLevelIndex, int levelIndex, bool firstStart)
        {
            if (IsLevelLoaded)
                UnloadLevel();

            LevelData levelData = LevelDatabase.GetLevel(levelIndex);
            currentLevelData = levelData;
            BallCoverEnabled = IsBallCoverLevelType(LevelType.Normal);//levelData.Type);
            LevelBotSet(levelData);

            // LevelRemoteConfigData overrideData = RemoteConfigController.TryGetConfig<LevelRemoteConfigData>($"level{displayedLevelIndex + 1}");
            // if (overrideData != null)
            // {
            //     if (!string.IsNullOrEmpty(overrideData.hash))
            //     {
            //         levelData = levelData.DecompressLevel(overrideData.hash);
            //     }

            //     // Apply remove config override
            //     if (overrideData.duration > 0)
            //     {
            //         levelData.ApplyDurationOverride(overrideData.duration);
            //     }
            // }

            // ChainManager.Init();
            // RopesManager.Init();

            // LevelRepresentation = new LevelRepresentation(levelData, environmentData);

            // LevelRepresentation.SpawnEnvironment();
            // LevelRepresentation.SpawnInteractiveObjects();
            // LevelRepresentation.SpawnBlocks();

            // movementManager.SetLevelRepresentation(LevelRepresentation);

            // RepositionCamera();

            // InitTimer();

            IsLevelLoaded = true;
            LevelLoaded?.Invoke();

            LevelController.InvokeScenario(LevelScenario.LevelStarted);

            ActiveSession activeSession = ActiveSession.Current;
            activeSession.OnLevelStarted(levelData);

            SavePresets.CreateSave("Level " + (displayedLevelIndex + 1).ToString("0000"), "Levels");

            FirebaseAnalyticsModule.Instance.SendLevelEvent(activeSession.DisplayLevelIndex, FirebaseAnalyticsModule.EventLeveType.complete);
        }

        public void LoadSpecialLevel(int displayedLevelIndex, int levelIndex)
        {
            if (IsLevelLoaded)
                UnloadLevel();

            var levelData = LevelDatabase.GetSpecialLevel(levelIndex);
            currentLevelData = levelData.levelData;
            BallCoverEnabled = IsBallCoverLevelType(LevelType.Hard);//levelData.levelData.Type);
            LevelBotSet(levelData.levelData);

            IsLevelLoaded = true;
            LevelLoaded?.Invoke();

            LevelController.InvokeScenario(LevelScenario.LevelStarted);

            ActiveSession activeSession = ActiveSession.Current;
            activeSession.OnLevelStarted(levelData.levelData);

            SavePresets.CreateSave("SpecialLevel " + (displayedLevelIndex + 1).ToString("0000"), "Levels");
        }


        private void LevelBotSet(LevelData levelData, bool keepCurrentBottleCount = false)
        {
            var data = levelData.GetPuzzleConfig();
            int targetBottleCount = keepCurrentBottleCount
                ? Mathf.Max(levelData.TubeCount, bottlesController.GetBottlesAmount())
                : levelData.TubeCount;

            bottlesController.SetBottlesAmount(targetBottleCount);

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].x == 0)
                {
                    bottlesController.SpawnBottle();
                }
                else if (data[i].y == 0)
                {
                    bottlesController.SpawnBottle(data[i].x);
                }
                else if (data[i].z == 0)
                {
                    bottlesController.SpawnBottle(data[i].x, data[i].y);
                }
                else if (data[i].w == 0)
                {
                    bottlesController.SpawnBottle(data[i].x, data[i].y, data[i].z);
                }
                else
                {
                    bottlesController.SpawnBottle(data[i].x, data[i].y, data[i].z, data[i].w);
                }
            }

            int baseBottleCount = data.Count + levelData.EmptyTubeCount;
            // int extraBottleCount = Mathf.Max(0, targetBottleCount - baseBottleCount);

            for (int i = 0; i < levelData.EmptyTubeCount; i++)
            {
                bottlesController.SpawnBottle();
            }

            // for (int i = 0; i < extraBottleCount; i++)
            // {
            //     bottlesController.SpawnBottle();
            // }
        }
        /// <summary>
        /// 重玩，设置当前的球回到初始状态
        /// </summary>
        public void ResetBottlePlayPanel()
        {
            if (currentLevelData == null) return;

            moveSteps.Clear();
            LevelBotSet(currentLevelData, keepCurrentBottleCount: true);
        }

        public void ResetBallSortModule()
        {
            bottlesController.ResetBallSortModule();
        }
        public void FinishScaleToHideBalls(Action callback)
        {
            bottlesController.FinishScaleToHide(callback);
        }

        public void OnLevelFailed()
        {

        }

        public void OnGameActivated()
        {
            // GameplayTimer.Start();

            // List<LevelBlockBehavior> blocks = LevelRepresentation.ActiveBlocks;
            // foreach (LevelBlockBehavior block in blocks)
            // {
            //     if (block.HasActiveEffect())
            //     {
            //         List<BlockEffectBehavior> effects = block.Effects;
            //         foreach (BlockEffectBehavior effect in effects)
            //         {
            //             effect.OnGameStarted();
            //         }
            //     }
            // }
        }

        // private void RepositionCamera()
        // {
        //     Camera mainCamera = Camera.main;
        //     if (mainCamera != null)
        //     {
        //         CameraController cameraController = mainCamera.GetComponent<CameraController>();
        //         if (cameraController != null)
        //         {
        //             Bounds bounds = LevelRepresentation.LevelBounds;

        //             cameraController.Reposition(bounds.center, bounds.size);
        //         }
        //     }
        // }

        public void UnloadLevel()
        {
            if (!IsLevelLoaded)
                return;

            IsLevelLoaded = false;
            LevelLoaded = null;
        }



        public static (BottleController firstBottle, BottleController secondBottle) GetFirstAndSecondBottle()
        {
            return bottlesController.GetFirstAndSecondBottle();
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            // LevelRepresentation?.DrawGizmos();
        }

        public static void InvokeOrWait(SimpleCallback loadCallback)
        {
            if (IsLevelLoaded)
            {
                loadCallback?.Invoke();
            }
            else
            {
                LevelLoaded += loadCallback;
            }
        }

        public static void InvokeScenario(LevelScenario scenario)
        {
            LevelScenarioChanged?.Invoke(scenario);
        }

        private static void UnloadStatic()
        {
            LevelLoaded = null;
            LevelScenarioChanged = null;
        }

        public delegate void LevelScenarioDelegate(LevelScenario type);


        public static void SRecordMoveStep(BottleController fromArg, BottleController toArg, int ballNumArg)
        {
            levelController.RecordMoveStep(fromArg, toArg, ballNumArg);
        }

        public static bool PullBackOneStep()
        {
            return levelController.PullBackAStep();
        }

        public static bool SAddABottle()
        {
            return levelController.AddABottle();
        }
        public static bool SuggestAStep()
        {
            return levelController.SuggestOneStep();
        }
    }
}
