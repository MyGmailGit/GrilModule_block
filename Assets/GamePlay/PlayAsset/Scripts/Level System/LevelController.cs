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

        [SerializeField] EnvironmentData environmentData;

        private static LevelController levelController;

        private static LevelSkinData skinData;

        private static LevelDatabase levelDatabase;
        public static LevelDatabase LevelDatabase => levelDatabase;

        public static LevelRepresentation LevelRepresentation { get; private set; }
        public static bool IsLevelLoaded { get; private set; } = false;

        private static BlockMovementManager movementManager;

        public static GameplayTimer GameplayTimer { get; private set; }

        public static event SimpleCallback LevelLoaded;
        public static event LevelScenarioDelegate LevelScenarioChanged;

        public void Init(LevelDatabase levelDatabase)
        {
            levelController = this;

            if (levelDatabase == null)
                Debug.LogError("Level database is not set. Please check the Game Data scriptable object.");

            LevelController.levelDatabase = levelDatabase;

            levelDatabase.Init();

            movementManager = new BlockMovementManager();

            GameplayTimer = new GameplayTimer();
            GameplayTimer.OnTimerFinished += OnGameplayTimerFinished;

            IsLevelLoaded = false;

            skinData = (LevelSkinData)SkinController.Instance.GetSelectedSkin<LevelSkinDatabase>();
            skinData.BlocksVisualsData.Init();

            ActiveSession activeSession = ActiveSession.Current;

            // int displayedLevelIndex = activeSession.DisplayLevelIndex;
            // int levelIndex = activeSession.GetLevelIndex(displayedLevelIndex);

            // LoadLevel(displayedLevelIndex, levelIndex, activeSession.FirstStart);
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

        public void HandleGameEnd()
        {
            RaycastController.Disable();

            OnObjectReleased();

            AudioController.PlaySound(AudioController.AudioClips.blockPick);

            GameplayTimer.Pause();

            List<LevelBlockBehavior> blocks = LevelRepresentation.ActiveBlocks;
            foreach (LevelBlockBehavior block in blocks)
            {
                if (block.HasActiveEffect())
                {
                    List<BlockEffectBehavior> effects = block.Effects;
                    foreach (BlockEffectBehavior effect in effects)
                    {
                        effect.OnGameEnded();
                    }
                }
            }
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
            GameplayTimer.AdjustTime(seconds);
            GameplayTimer.Resume();

            RaycastController.Enable();

            List<LevelBlockBehavior> blocks = LevelRepresentation.ActiveBlocks;
            foreach (LevelBlockBehavior block in blocks)
            {
                if (block.HasActiveEffect())
                {
                    List<BlockEffectBehavior> effects = block.Effects;
                    foreach (BlockEffectBehavior effect in effects)
                    {
                        effect.OnRevived();
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            MovementUpdate();
        }

        public void LoadSpecialLevel(int displayedLevelIndex, int levelIndex)
        {
            if (IsLevelLoaded)
                UnloadLevel();

            var levelDataValue = LevelDatabase.GetSpecialLevel(levelIndex);
            var levelData = levelDataValue.levelData;

            LevelRemoteConfigData overrideData = RemoteConfigController.TryGetConfig<LevelRemoteConfigData>($"level{displayedLevelIndex + 1}");
            if (overrideData != null)
            {
                if (!string.IsNullOrEmpty(overrideData.hash))
                {
                    levelData = levelData.DecompressLevel(overrideData.hash);
                }

                // Apply remove config override
                if (overrideData.duration > 0)
                {
                    levelData.ApplyDurationOverride(overrideData.duration);
                }
            }

            ChainManager.Init();
            RopesManager.Init();

            LevelRepresentation = new LevelRepresentation(levelData, environmentData);

            LevelRepresentation.SpawnEnvironment();
            LevelRepresentation.SpawnInteractiveObjects();
            LevelRepresentation.SpawnBlocks();

            movementManager.SetLevelRepresentation(LevelRepresentation);

            RepositionCamera();

            InitTimer();

            IsLevelLoaded = true;
            LevelLoaded?.Invoke();

            LevelController.InvokeScenario(LevelScenario.LevelStarted);

            ActiveSession activeSession = ActiveSession.Current;
            activeSession.OnLevelStarted(levelDataValue.levelData);

            SavePresets.CreateSave("SpecialLevel " + (displayedLevelIndex + 1).ToString("0000"), "Levels");

            FirebaseAnalyticsModule.Instance.SendLevelEvent(displayedLevelIndex, FirebaseAnalyticsModule.EventLeveType.start);
        }


        public void LoadLevel(int displayedLevelIndex, int levelIndex, bool firstStart)
        {
            if (IsLevelLoaded)
                UnloadLevel();

            LevelData levelData = LevelDatabase.GetLevel(levelIndex);

            LevelRemoteConfigData overrideData = RemoteConfigController.TryGetConfig<LevelRemoteConfigData>($"level{displayedLevelIndex + 1}");
            if (overrideData != null)
            {
                if (!string.IsNullOrEmpty(overrideData.hash))
                {
                    levelData = levelData.DecompressLevel(overrideData.hash);
                }

                // Apply remove config override
                if (overrideData.duration > 0)
                {
                    levelData.ApplyDurationOverride(overrideData.duration);
                }
            }

            ChainManager.Init();
            RopesManager.Init();

            LevelRepresentation = new LevelRepresentation(levelData, environmentData);

            LevelRepresentation.SpawnEnvironment();
            LevelRepresentation.SpawnInteractiveObjects();
            LevelRepresentation.SpawnBlocks();

            movementManager.SetLevelRepresentation(LevelRepresentation);

            RepositionCamera();

            InitTimer();

            IsLevelLoaded = true;
            LevelLoaded?.Invoke();

            LevelController.InvokeScenario(LevelScenario.LevelStarted);

            ActiveSession activeSession = ActiveSession.Current;
            activeSession.OnLevelStarted(levelData);

            SavePresets.CreateSave("Level " + (displayedLevelIndex + 1).ToString("0000"), "Levels");

            FirebaseAnalyticsModule.Instance.SendLevelEvent(displayedLevelIndex, FirebaseAnalyticsModule.EventLeveType.start);
        }

        public void OnLevelFailed()
        {

        }

        public void OnGameActivated()
        {
            GameplayTimer.Start();

            List<LevelBlockBehavior> blocks = LevelRepresentation.ActiveBlocks;
            foreach (LevelBlockBehavior block in blocks)
            {
                if (block.HasActiveEffect())
                {
                    List<BlockEffectBehavior> effects = block.Effects;
                    foreach (BlockEffectBehavior effect in effects)
                    {
                        effect.OnGameStarted();
                    }
                }
            }
        }

        private void RepositionCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                CameraController cameraController = mainCamera.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    Bounds bounds = LevelRepresentation.LevelBounds;

                    cameraController.Reposition(bounds.center, bounds.size);
                }
            }
        }

        public void UnloadLevel()
        {
            if (!IsLevelLoaded)
                return;

            IsLevelLoaded = false;
            LevelLoaded = null;
        }

        #region Movement
        private void MovementUpdate()
        {
            movementManager.FixedUpdate();

            if (movementManager.IsBlockPicked && Time.frameCount % 3 == 0)
            {
                // Check if block can be picked
                LevelBlockBehavior levelBlockBehavior = movementManager.BlockBehavior;

                bool IsMovementAllowed(LevelBlockBehavior levelBlockBehavior)
                {
                    IEnumerable<(GateDirection.Type, GateBehavior)> nearGates = LevelRepresentation.EnvironmentSpawner.NearGates(levelBlockBehavior, movementManager.MovementMatrix);
                    foreach (var gates in nearGates)
                    {
                        GateBehavior gateBehavior = gates.Item2;
                        GateDirection.Type direction = gates.Item1;

                        if (direction != GateDirection.Type.None && gateBehavior != null)
                        {
                            GateDirection gateDirection = GateDirection.DIRECTIONS[(int)direction];

                            bool isAllowedToGoThrough = levelBlockBehavior.OnGateEntered(gateBehavior, gateDirection);
                            if (isAllowedToGoThrough)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                if (IsMovementAllowed(levelBlockBehavior))
                {
                    OnObjectReleased();
                }
                else
                {
                    if (levelBlockBehavior.MoveMultiplyObjects())
                    {
                        List<LevelBlockBehavior> linkedBlocks = levelBlockBehavior.GetLinkedBlocks();
                        if (!linkedBlocks.IsNullOrEmpty())
                        {
                            foreach (LevelBlockBehavior linkedBlock in linkedBlocks)
                            {
                                if (IsMovementAllowed(linkedBlock))
                                {
                                    OnObjectReleased();

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void OnObjectPicked(LevelBlockBehavior levelBlockBehavior)
        {
            if (levelBlockBehavior.IsCollected)
                return;

            movementManager.PickObject(levelBlockBehavior);

            if (levelBlockBehavior.MoveMultiplyObjects())
            {
                movementManager.LinkObjects(levelBlockBehavior.GetLinkedBlocks());
            }

            if (movementManager.IsBlockPicked)
            {
                List<InteractableObjectBehavior> interactableObject = LevelRepresentation.InteractableObjects;
                foreach (InteractableObjectBehavior interactable in interactableObject)
                {
                    interactable.OnBlockPicked(levelBlockBehavior);
                }
            }
        }

        public static void TryToCollectBlock(LevelBlockBehavior levelBlockBehavior, MovementMatrix movementMatrix, SimpleCallback onCollected = null)
        {
            if (levelBlockBehavior != null)
            {
                IEnumerable<(GateDirection.Type, GateBehavior)> nearGates = LevelRepresentation.EnvironmentSpawner.NearGates(levelBlockBehavior, movementMatrix);
                foreach (var gates in nearGates)
                {
                    GateBehavior gateBehavior = gates.Item2;
                    GateDirection.Type direction = gates.Item1;

                    if (direction != GateDirection.Type.None && gateBehavior != null)
                    {
                        GateDirection gateDirection = GateDirection.DIRECTIONS[(int)direction];

                        bool isAllowedToGoThrough = levelBlockBehavior.OnGateEntered(gateBehavior, gateDirection);
                        if (isAllowedToGoThrough)
                        {
                            BlockClip blockClip = new BlockClip(levelBlockBehavior.MeshRenderer, gateBehavior.transform.position + gateDirection.ClipOffset, gateDirection.DirectionNormal);
                            BlockDestructionParticle blockDestructionParticle = new BlockDestructionParticle(levelBlockBehavior, levelBlockBehavior.ColorData.Material, gateBehavior, gateDirection);

                            int elementsSize = gateDirection.GetAlignedSize(levelBlockBehavior.Figure);

                            levelBlockBehavior.OnBlockCollected();
                            levelBlockBehavior.transform.DOMove(levelBlockBehavior.transform.position + gateDirection.GetMoveOffset(levelBlockBehavior.Figure), LevelController.BLOCK_MOVE_DURATION * elementsSize).SetEase(LevelController.BLOCK_MOVE_EASE_TYPE).OnComplete(() =>
                            {
                                // Disable object
                                levelBlockBehavior.gameObject.SetActive(false);
                                levelBlockBehavior.DisableEffects();

                                blockClip.Destroy();
                                blockDestructionParticle.Stop();

                                if (LevelRepresentation.AllBlocksCleared)
                                {
                                    GameController.GameComplete();
                                }

                                onCollected?.Invoke();
                            });


                            OnBlockDestructed(levelBlockBehavior);
                            OnBlockCollected(levelBlockBehavior, gateBehavior);
                        }
                    }
                }
            }
        }

        public static void OnObjectReleased()
        {
            LevelBlockBehavior pickedBlock = movementManager.BlockBehavior;
            if (pickedBlock != null)
            {
                List<BlockMovementManager.LinkedObjectData> linkedObjects = movementManager.LinkedObjects;
                List<InteractableObjectBehavior> interactableObject = LevelRepresentation.InteractableObjects;

                movementManager.SnapToClosestPosition();

                TryToCollectBlock(pickedBlock, movementManager.MovementMatrix);

                if (!linkedObjects.IsNullOrEmpty())
                {
                    foreach (BlockMovementManager.LinkedObjectData linkedObject in linkedObjects)
                    {
                        TryToCollectBlock(linkedObject.Block, movementManager.MovementMatrix);

                        foreach (InteractableObjectBehavior interactable in interactableObject)
                        {
                            interactable.OnBlockReleased(linkedObject.Block);
                        }
                    }
                }

                foreach (InteractableObjectBehavior interactable in interactableObject)
                {
                    interactable.OnBlockReleased(pickedBlock);
                }

                movementManager.ReleaseObject();
                AudioController.PlaySound(AudioController.AudioClips.blockPick);
            }
        }

        public static void OnBlockDestructed(LevelBlockBehavior destructedBlock)
        {
            LevelRepresentation.OnBlockDestructed(destructedBlock);

            List<LevelBlockBehavior> activeBlocks = LevelRepresentation.ActiveBlocks;
            foreach (LevelBlockBehavior block in activeBlocks)
            {
                block.OnBlockDestructed(destructedBlock);
            }

            List<GateBehavior> gates = LevelRepresentation.EnvironmentSpawner.Gates;
            foreach (GateBehavior gate in gates)
            {
                gate.OnBlockDestructed(destructedBlock);
            }
        }

        public static void OnBlockCollected(LevelBlockBehavior collectedBlock, GateBehavior gateBehavior)
        {
            Haptic.Play(Haptic.HAPTIC_HARD);

            List<LevelBlockBehavior> activeBlocks = LevelRepresentation.ActiveBlocks;
            foreach (LevelBlockBehavior block in activeBlocks)
            {
                block.OnBlockEnteredGate(collectedBlock, gateBehavior);
            }

            List<GateBehavior> gates = LevelRepresentation.EnvironmentSpawner.Gates;
            foreach (GateBehavior gate in gates)
            {
                gate.OnBlockEntered(collectedBlock);
            }

            AudioController.PlaySound(AudioController.AudioClips.blockDestroy);
        }
        #endregion

        #region Timer
        private static void InitTimer()
        {
            float time = LevelRepresentation.LevelData.Duration;

            GameplayTimer.SetMaxTime(time);
        }

        private void Update()
        {
            GameplayTimer.Update();
        }

        private static void OnGameplayTimerFinished()
        {
            // This check prevents game over when the level is completed but animation is not finished yet
            if (LevelRepresentation.AllBlocksCleared)
                return;

            GameController.GameOver(true);
        }

        #endregion

        public static LevelBlockEffectData GetEffectData(BlockEffectType effectType)
        {
            return levelDatabase.GetEffectData(effectType);
        }

        public static LevelGateEffectData GetEffectData(GateEffectType effectType)
        {
            return levelDatabase.GetEffectData(effectType);
        }

        public static LevelInteractableObjectData GetInteractableObject(InteractableObjectType objectType)
        {
            return levelDatabase.GetInteractableObjectData(objectType);
        }

        public static BlocksVisualsData GetBlocksVisualsData()
        {
            return skinData.BlocksVisualsData;
        }

        public static BlockColorData GetBlockColorData(BlockColor blockColor)
        {
            return skinData.BlocksVisualsData.GetColorData(blockColor);
        }

        public static BlockData GetBlockData(BlockType blockType)
        {
            return skinData.BlocksVisualsData.GetBlockData(blockType);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            LevelRepresentation?.DrawGizmos();
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
    }
}
