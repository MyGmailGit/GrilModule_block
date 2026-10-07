using FirebaseRemote;
using GameUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;

namespace Watermelon
{
    public class StaticMapPanel : MonoBehaviour
    {
        private const string BUTTON_TEXT = "Play {0}";//"LEVEL {0}";

        [SerializeField] RectTransform[] mapElementPositions;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] TextMeshProUGUI playButtonText;
        [SerializeField] TextMeshProUGUI levelTypeText;
        [SerializeField] Image playButtonImg;
        // [SerializeField] Sprite[] playButtonSprite;

        public void Init()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);

            GameData gameData = GameData.Data;
            LevelDatabase levelDatabase = gameData.LevelDatabase;
            ActiveSession session = ActiveSession.Current;

            int currentIndex = session.Save.DisplayLevelIndex;
            // int levelIndex = currentIndex;
            // for (int i = 0; i < mapElementPositions.Length; i++)
            // {
            //     LevelData levelData = levelDatabase.GetLevel(levelIndex);
            //     if (levelData == null)
            //         levelData = levelDatabase.GetRandomLevel();

            //     MapLevelData mapData = levelDatabase.GetMapLevelData(levelData.Type);

            //     RectTransform targetTransform = mapElementPositions[i];

            //     GameObject mapObject = Instantiate(mapData.Prefab, targetTransform);
            //     mapObject.transform.localPosition = Vector3.zero;

            //     MapLevelBehavior mapLevelBehavior = mapObject.GetComponent<MapLevelBehavior>();
            //     mapLevelBehavior.Init(levelIndex, currentIndex);

            //     levelIndex++;
            // }

            LevelData currentLevelData = levelDatabase.GetLevel(session.Save.DisplayLevelIndex);
            if (currentLevelData == null)
                currentLevelData = levelDatabase.GetRandomLevel();

            // MapLevelData currentMapData = levelDatabase.GetMapLevelData(currentLevelData.Type);

            // playButtonText.text = string.Format(BUTTON_TEXT, session.Save.DisplayLevelIndex + 1);

            levelTypeText.text = string.Format(BUTTON_TEXT, session.Save.DisplayLevelIndex + 1);

            // levelTypeText.text = currentMapData.Lable;

            // playButtonImg.sprite = currentLevelData.Type == LevelType.Normal ? playButtonSprite[0] : playButtonSprite[1];
        }

        private void OnPlayButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            if (UIMainMenu.is_AB_VideoIsB_Local)
            {
                if (VideoSerilNumberManager.Instance.GetCurrentProgress() == null)
                {
                    UIPlaySameGirl.Show(() =>
                    {
                        if (VideoSerilNumberManager.Instance.GetCurrentProgress() == null)
                        {
                            UICharacterChoose.ShowPage((i) =>
                            {
                                if (i < 0)
                                {
                                    return;
                                }
                                EnterToPlay();
                            });
                        }
                        else
                        {
                            EnterToPlay();
                        }
                    });
                }
                else
                {
                    EnterToPlay();
                }
            }
            else
            {
                EnterToPlay();
            }
        }

        private void EnterToPlay()
        {
            MenuController.OnPlayButtonClicked();

            FirebaseWrapper.CheckNoticfacation();
        }
    }
}
