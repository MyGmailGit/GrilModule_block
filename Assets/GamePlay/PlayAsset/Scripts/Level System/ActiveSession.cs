#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [StaticUnload]
    public class ActiveSession
    {
        private enum LevelOrSprcialLevel
        {
            Level,
            SpecialLevel
        }

        private readonly LevelSave levelSave;

        public LevelSave Save => levelSave;

        public int DisplayLevelIndex => levelSave.DisplayLevelIndex;
        public int LevelIndex => levelSave.RealLevelIndex;
        public int MaxReachedLevelIndex => levelSave.MaxReachedLevelIndex;

        public bool FirstStart => levelSave.FirstStart;
        public bool FirstTimeCompletedLevel => levelSave.CompletedLevelIndex < levelSave.DisplayLevelIndex;

        LevelOrSprcialLevel levelOrSprcialLevel = LevelOrSprcialLevel.Level;

        public int DisplaySpecialLevelIndex => levelSave.DisplaySpecialLevelIndex;
        public int SpecialLevelIndex => levelSave.DisplaySpecialLevelIndex;

        /// <summary>
        /// 这个记录当前使用的关卡
        /// </summary>
        /// <returns></returns>
        public int GetNextSpecialLevelIndex()
        {
            int nextIndex = levelSave.RealSpecialLevelIndex + 1;
            levelSave.RealSpecialLevelIndex = nextIndex;
            return nextIndex;
        }

        private LevelData levelData;
        public LevelData LevelData => levelData;

        private static ActiveSession currentSession;
        public static ActiveSession Current
        {
            get
            {
                if (currentSession == null)
                    currentSession = new ActiveSession();

                return currentSession;
            }
        }

        public ActiveSession()
        {
            levelSave = SaveController.GetSaveObject<LevelSave>();
        }

        public void OnLevelStarted(LevelData levelData)
        {
            this.levelData = levelData;

            levelSave.FirstStart = false;
        }

        public void OnLevelCompleted()
        {
            levelSave.IsPlayingRandomLevel = false;
            levelSave.LastPlayerLevelIndex = -1;

            if (FirstTimeCompletedLevel)
            {
                levelSave.CompletedLevelIndex = levelSave.RealLevelIndex;
            }
        }

        public static void SetEditorLevelIndex(int levelIndex)
        {
            if (Application.isPlaying)
            {
                ActiveSession activeSession = Current;
                activeSession.SetLevelIndex(levelIndex);

                return;
            }

#if UNITY_EDITOR
            GlobalSave globalSave = SaveController.GetGlobalSave();

            LevelSave editorLevelSave = globalSave.GetSaveObject<LevelSave>();
            editorLevelSave.DisplayLevelIndex = levelIndex;
            editorLevelSave.FirstStart = true;

            if (levelIndex > editorLevelSave.MaxReachedLevelIndex)
            {
                editorLevelSave.MaxReachedLevelIndex = levelIndex;
            }

            SaveController.SaveCustom(globalSave);
#endif
        }

        public void SetLevelIndex(int levelNumber)
        {
            if (levelSave.DisplayLevelIndex != levelNumber)
                levelSave.IsPlayingRandomLevel = false;

            var disLevel = levelSave.DisplayLevelIndex;

            levelSave.DisplayLevelIndex = levelNumber;
            levelSave.FirstStart = true;

            if (levelNumber > levelSave.MaxReachedLevelIndex)
            {
                levelSave.MaxReachedLevelIndex = levelNumber;
            }

            if (disLevel != levelNumber)
            {
                AnalyticsController.OnGameLevelChange(levelNumber.ToString());
            }

            levelOrSprcialLevel = LevelOrSprcialLevel.Level;
        }

        public int GetLevelIndex(int levelIndex)
        {
            int realLevelIndex;
            if (levelSave.IsPlayingRandomLevel && levelIndex == levelSave.DisplayLevelIndex && levelSave.RealLevelIndex != -1)
            {
                realLevelIndex = levelSave.RealLevelIndex;
            }
            else
            {
                realLevelIndex = LevelController.LevelDatabase.GetRandomLevelIndex(levelIndex, levelSave.LastPlayerLevelIndex, false);

                levelSave.LastPlayerLevelIndex = realLevelIndex;
                levelSave.RealLevelIndex = realLevelIndex;

                if (realLevelIndex != levelIndex)
                {
                    levelSave.IsPlayingRandomLevel = true;
                }
            }

            return realLevelIndex;
        }

        public bool IsPlaySpecialLevel()
        {
            return levelOrSprcialLevel == LevelOrSprcialLevel.SpecialLevel;
        }

        public void SetSpecialLevelIndex(int levelNumber)
        {
            levelSave.DisplaySpecialLevelIndex = levelNumber;
            levelOrSprcialLevel = LevelOrSprcialLevel.SpecialLevel;
        }
        public int GetSpecialLevelIndex()
        {
            return levelSave.DisplaySpecialLevelIndex;
        }


        private static void UnloadStatic()
        {
            currentSession = null;
        }
    }
}