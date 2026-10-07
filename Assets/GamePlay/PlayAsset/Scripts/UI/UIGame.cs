using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanel;
        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple diamondPanel;

        [BoxGroup("Top Panel")]
        [SerializeField] TimerVisualiser gameplayTimer;
        public TimerVisualiser GameplayTimer => gameplayTimer;

        [BoxGroup("Top Panel")]
        [SerializeField] LevelPanel levelPanel;

        [BoxGroup("Gameplay")]
        [SerializeField] PUUIController powerUpsUIController;
        public PUUIController PowerUpsUIController => powerUpsUIController;

        [BoxGroup("Message Box")]
        [SerializeField] MessageBox messageBox;
        public MessageBox MessageBox => messageBox;

        [SerializeField] ParticleSystem particleOne;
        [SerializeField] ParticleSystem particleTwo;

        [SerializeField] Button ReplayButton;

        [SerializeField] GamePlayGirlPicProgress gamePlayGirlPicProgress;


        public override void Init()
        {
            coinsPanel.Init();
            diamondPanel.Init();
            messageBox.Init();

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            RefreshPicProgress();

            coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);
            diamondPanel.AddButton.onClick.AddListener(AddDiamondButton);

            ReplayButton.onClick.AddListener(OnReplayButtonTouch);
        }

        public void RefreshPicProgress()
        {
            if (ActiveSession.Current.IsPlaySpecialLevel() || !GameController.is_AB_VideoIsB_Local)
            {
                if (ActiveSession.Current.IsPlaySpecialLevel())
                {
                    levelPanel.Init(ActiveSession.Current.DisplaySpecialLevelIndex, true);
                }
                else
                {
                    levelPanel.Init(ActiveSession.Current.DisplayLevelIndex, false);
                }
                gamePlayGirlPicProgress.gameObject.SetActive(false);
            }
            else
            {
                levelPanel.Init(ActiveSession.Current.DisplayLevelIndex, false);
                gamePlayGirlPicProgress.gameObject.SetActive(true);
                gamePlayGirlPicProgress.SetNowPicProgress();
            }
        }

        public override void PlayHideAnimation()
        {
            gameplayTimer.Hide();

            coinsPanel.Disable();
            diamondPanel.Disable();

            UIController.OnPageClosed(this);

            coinsPanel.AddButton.onClick.RemoveAllListeners();
            diamondPanel.AddButton.onClick.RemoveAllListeners();
        }

        public override void PlayShowAnimation()
        {
            // gameplayTimer.Show(LevelController.GameplayTimer);

            coinsPanel.Activate();
            diamondPanel.Activate();

            UIController.OnPageOpened(this);
            MusicSource.ActiveMusicSource.Activate(0);
        }

        public void AddCoinsButton()
        {
            UIController.ShowPage<IAPStore.UIStoreCoinDiamond>();
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
        public void AddDiamondButton()
        {
            UIController.ShowPage<IAPStore.UIStoreCoinDiamond>();
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
        private void OnReplayButtonTouch()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            if (GameController.is_AB_VideoIsB_Local)
            {

                if (GameController.ShouldCheckMonthlyWinStreak())
                {
                    UIPopResetGame.Show((closeType) =>
                    {
                        if (closeType == UIPopResetGame.CloseType.RvTouch || closeType == UIPopResetGame.CloseType.LostTouch)
                        {
                            GameController.ResetBallPlayPanel(closeType == UIPopResetGame.CloseType.LostTouch);
                        }
                    });
                }
                else
                {
                    GameController.ResetBallPlayPanel(false);
                    SystemMessage.ShowMessage("The current round has been reset.");
                }
            }
            else
            {
                GameController.ResetBallPlayPanel(false);
                SystemMessage.ShowMessage("The current round has been reset.");
            }

            // public void PlayFinishParticle()
            // {
            //     StartCoroutine(PlayFinishParticles());
            // }
            // IEnumerator PlayFinishParticles()
            // {
            //     particleOne.Clear();
            //     particleTwo.Clear();
            //     particleOne.Play();
            //     yield return new WaitForSeconds(0.7f);

            //     particleTwo.Play();
            // }

        }

        public void SetLabelShow()
        {
            UIGameLevelLabel.Show(levelPanel.GetShowLevelStr());
        }
    }
}
