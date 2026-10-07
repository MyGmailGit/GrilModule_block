#pragma warning disable 0649

using System.Linq;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/Level/Level Database", fileName = "Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField, LevelEditorSetting] LevelData[] levels;
        public LevelData[] Levels => levels;

        [SerializeField, LevelEditorSetting] LevelData[] specialLevels;
        public LevelData[] SpecialLevels => specialLevels;

        // [Space]
        // [SerializeField, LevelEditorSetting] ElementTypeEditorData[] cells; // used only in level editor
        // [SerializeField, LevelEditorSetting] EditorColorData[] editorColorData; // used only in level editor

        // [Space]
        // [SerializeField] LevelBlockEffectData[] effects;
        // public LevelBlockEffectData[] Effects => effects;

        // [SerializeField] LevelGateEffectData[] gateEffects;
        // public LevelGateEffectData[] GateEffects => gateEffects;

        // [SerializeField] LevelInteractableObjectData[] interactableObjects;
        // public LevelInteractableObjectData[] InteractableObjects => interactableObjects;

        // [Space]
        // [SerializeField] MapLevelData[] mapLevelDatas;
        // public MapLevelData[] MapLevelDatas => mapLevelDatas;

        public int AmountOfLevels => levels.Length;
        public int AmountOfSpecialLevels => specialLevels.Length;

        [Button]
        private void Validate()
        {
            foreach (LevelData level in levels)
            {
                level.Validate();
            }

            RuntimeEditorUtils.SetDirty(this);
        }

        /// <summary>
        /// Is called when LevelController is initialized
        /// </summary>
        public void Init()
        {
            // foreach (LevelBlockEffectData effect in effects)
            // {
            //     effect.Init();
            // }

            // foreach (LevelGateEffectData effect in gateEffects)
            // {
            //     effect.Init();
            // }

            // foreach (LevelInteractableObjectData interactableObject in interactableObjects)
            // {
            //     interactableObject.Init();
            // }
        }

        public int GetRandomLevelIndex(int displayLevelNumber, int lastPlayedLevelNumber, bool replayingLevel)
        {
            if (levels.IsInRange(displayLevelNumber))
            {
                return displayLevelNumber;
            }

            if (replayingLevel)
            {
                return lastPlayedLevelNumber;
            }

            int randomLevelIndex;
            int attempts = 0;

            do
            {
                randomLevelIndex = Random.Range(0, levels.Length);

                attempts++;
                if (attempts > 100)
                    return randomLevelIndex;
            }
            while (!levels[randomLevelIndex].UseInRandomizer && randomLevelIndex != lastPlayedLevelNumber);

            return randomLevelIndex;
        }

        public LevelData GetRandomLevel()
        {
            int randomLevelIndex;

            int attempts = 0;

            do
            {
                randomLevelIndex = Random.Range(0, levels.Length);

                attempts++;
                if (attempts > 100)
                {
                    return levels[randomLevelIndex];
                }
            }
            while (!levels[randomLevelIndex].UseInRandomizer);

            return levels[randomLevelIndex];
        }

        public LevelData GetLevel(int index)
        {
            if (index < AmountOfLevels && index >= 0)
                return levels[index];

            return null;
        }


        #region  special list 
        public (LevelData levelData, int index) GetSpecialLevel(int idx)
        {
            if (idx < AmountOfSpecialLevels && idx >= 0)
                return (specialLevels[idx], idx);

            int index = idx % AmountOfSpecialLevels;
            return (specialLevels[index], index);
        }

        #endregion




        // public LevelBlockEffectData GetEffectData(BlockEffectType effectType)
        // {
        //     foreach (LevelBlockEffectData effect in effects)
        //     {
        //         if (effect.Type == effectType)
        //             return effect;
        //     }

        //     Debug.LogError($"Effect data for {effectType} not found in level database. Please check the LevelDatabase asset.", this);

        //     return null;
        // }

        // public LevelGateEffectData GetEffectData(GateEffectType effectType)
        // {
        //     foreach (LevelGateEffectData effect in gateEffects)
        //     {
        //         if (effect.Type == effectType)
        //             return effect;
        //     }

        //     Debug.LogError($"Effect data for {effectType} not found in level database. Please check the LevelDatabase asset.", this);

        //     return null;
        // }

        // public LevelInteractableObjectData GetInteractableObjectData(InteractableObjectType objectType)
        // {
        //     foreach (LevelInteractableObjectData interactableObject in interactableObjects)
        //     {
        //         if (interactableObject.Type == objectType)
        //             return interactableObject;
        //     }

        //     Debug.LogError($"Interactable object data for {objectType} not found in level database. Please check the LevelDatabase asset.", this);

        //     return null;
        // }

        // public MapLevelData GetMapLevelData(LevelType levelType)
        // {
        //     MapLevelData mapLevelData = mapLevelDatas.First(data => data.LevelType == levelType);
        //     if (mapLevelData == null)
        //     {
        //         Debug.LogError($"Map level data for {levelType} not found in level database. Please check the LevelDatabase asset.", this);
        //     }

        //     return mapLevelData;
        // }
    }
}
