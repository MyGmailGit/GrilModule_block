#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelBlockEffectData
    {
        [SerializeField] BlockEffectType type;
        public BlockEffectType Type => type;

        [SerializeField] BlockEffectBehavior behavior;
        public BlockEffectBehavior Behavior => behavior;

        [SerializeField] FeatureAnnouncementPopupData announcementPopupData;
        public FeatureAnnouncementPopupData AnnouncementPopupData => announcementPopupData;

        private LevelBlockEffectSave saveData;
        public LevelBlockEffectSave SaveData => saveData;

        public void Init()
        {
            if(Application.isPlaying)
            {
                saveData = SaveController.GetSaveObject<LevelBlockEffectSave>(string.Format("effect_{0}", type));
            }
            else
            {
                saveData = new LevelBlockEffectSave();
            }
        }
    }
}
