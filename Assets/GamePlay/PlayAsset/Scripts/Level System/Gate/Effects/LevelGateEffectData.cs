using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelGateEffectData
    {
        [SerializeField] GateEffectType type;
        public GateEffectType Type => type;

        [SerializeField] GateEffectBehavior behavior;
        public GateEffectBehavior Behavior => behavior;

        [SerializeField] FeatureAnnouncementPopupData announcementPopupData;
        public FeatureAnnouncementPopupData AnnouncementPopupData => announcementPopupData;

        private LevelGateEffectSave saveData;
        public LevelGateEffectSave SaveData => saveData;

        public void Init()
        {
            if (Application.isPlaying)
            {
                saveData = SaveController.GetSaveObject<LevelGateEffectSave>(string.Format("gate_effect_{0}", type));
            }
            else
            {
                saveData = new LevelGateEffectSave();
            }
        }
    }
}