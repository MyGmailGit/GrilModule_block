using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelInteractableObjectData
    {
        [SerializeField] InteractableObjectType type;
        public InteractableObjectType Type => type;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] FeatureAnnouncementPopupData announcementPopupData;
        public FeatureAnnouncementPopupData AnnouncementPopupData => announcementPopupData;

        private LevelInteractableSave saveData;
        public LevelInteractableSave SaveData => saveData;

        public void Init()
        {
            if (Application.isPlaying)
            {
                saveData = SaveController.GetSaveObject<LevelInteractableSave>(string.Format("interactable_{0}", type));
            }
            else
            {
                saveData = new LevelInteractableSave();
            }
        }
    }
}
