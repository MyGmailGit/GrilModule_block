using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class LevelPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] Image icon;

        private const string LevelStr = "Level {0}";
        private const string SpeLevelStr = "Special Level {0}";

        public string GetShowLevelStr()
        {
            return levelText.text;
        }
        public void Init(int levelIndex, bool isSpecial)
        {
            GameData gameData = GameData.Data;
            LevelDatabase levelDatabase = gameData.LevelDatabase;
            // LevelRepresentation levelRepresentation = LevelController.LevelRepresentation;

            // LevelData levelData = levelRepresentation.LevelData;
            // MapLevelData mapLevelData = levelDatabase.GetMapLevelData(levelData.Type);

            levelText.text = string.Format(isSpecial ? SpeLevelStr : LevelStr, levelIndex + 1);

            // levelText.color = mapLevelData.TextColor;

            // Sprite customIcon = mapLevelData.Icon;
            // if (customIcon != null)
            // {
            //     icon.sprite = customIcon;
            //     icon.color = mapLevelData.IconColor;
            //     icon.gameObject.SetActive(true);

            //     RectTransform iconRectTransform = icon.rectTransform;
            //     iconRectTransform.sizeDelta *= mapLevelData.IconScale;

            //     RectTransform textRectTransform = levelText.rectTransform;
            //     float textWidth = textRectTransform.sizeDelta.x;
            //     float totalWidth = textWidth + iconRectTransform.sizeDelta.x + Mathf.Abs(iconRectTransform.anchoredPosition.x);

            //     float offset = (totalWidth - textWidth) / 2;

            //     textRectTransform.anchoredPosition = new Vector2(offset, textRectTransform.anchoredPosition.y);
            // }
            // else
            // {
            //     // icon.gameObject.SetActive(false);

            //     levelText.rectTransform.anchoredPosition = Vector2.zero;
            // }
        }
    }
}
