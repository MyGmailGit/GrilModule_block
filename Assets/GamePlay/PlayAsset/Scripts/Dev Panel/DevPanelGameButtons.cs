using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(DevPanel))]
    public class DevPanelGameButtons : MonoBehaviour
    {
        [SerializeField] Button nextLevelButton;
        [SerializeField] Button prevLevelButton;
        [SerializeField] Button failLevelButton;
        [SerializeField] Button completeLevelButton;
        [SerializeField] Button forceToVideoButton;
        [SerializeField] Button AddCoinButton;
        [SerializeField] Button ShowApplovinDebug;

        private DevPanel devPanel;

        private void Awake()
        {
            devPanel = GetComponent<DevPanel>();

            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
            prevLevelButton.onClick.AddListener(OnPrevLevelButtonClicked);
            failLevelButton.onClick.AddListener(OnFailLevelButtonClicked);
            completeLevelButton.onClick.AddListener(OnCompleteLevelButtonClicked);
            forceToVideoButton.onClick.AddListener(OnForceToVideoButton);
            AddCoinButton.onClick.AddListener(OnCoinAddButton);
            ShowApplovinDebug.onClick.AddListener(OnShowApplovinDebug);
        }



        private void OnEnable()
        {
            RefreshForceToVideo();
        }
        private void RefreshForceToVideo()
        {
            var txt = forceToVideoButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            txt.text = DevPanelEnabler.IsDevForceToVideo ? "Force To Video\nforce video" : "Force To Video\norganic";
        }
        private void OnPrevLevelButtonClicked()
        {
            ActiveSession activeSession = ActiveSession.Current;
            activeSession.OnLevelCompleted();

            int prevLevelIndex = Mathf.Clamp(ActiveSession.Current.DisplayLevelIndex - 1, 0, int.MaxValue);
            activeSession.SetLevelIndex(prevLevelIndex);

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });

            devPanel.DisablePanel();
        }

        private void OnNextLevelButtonClicked()
        {
            ActiveSession activeSession = ActiveSession.Current;
            activeSession.OnLevelCompleted();

            activeSession.SetLevelIndex(activeSession.DisplayLevelIndex + 1);

            Overlay.Show(0.3f, () =>
            {
                GameController.Unload(() =>
                {
                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            });

            devPanel.DisablePanel();
        }

        private void OnFailLevelButtonClicked()
        {
            GameController.GameOver(true);

            devPanel.DisablePanel();
        }

        private void OnCompleteLevelButtonClicked()
        {
            GameController.GameComplete();

            devPanel.DisablePanel();
        }

        private void OnForceToVideoButton()
        {
            DevPanelEnabler.SetForceToVideo(!DevPanelEnabler.IsDevForceToVideo);
            RefreshForceToVideo();
            // if (DevPanelEnabler.IsDevForceToVideo)
            {
                GameBackgroundVideoPreview.RefreshPlayNowVideo();
            }
        }
        private void OnCoinAddButton()
        {
            CurrencyController.Set(CurrencyType.Coins, 200000);
        }

        private void OnShowApplovinDebug()
        {
#if MODULE_APPLOVIN
            MaxSdk.ShowMediationDebugger();
#endif
        }
    }
}
