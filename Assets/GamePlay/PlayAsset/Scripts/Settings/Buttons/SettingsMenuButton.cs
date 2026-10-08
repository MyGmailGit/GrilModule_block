using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class SettingsMenuButton : SettingsButtonBase
    {
        public override void Init()
        {
            gameObject.SetActive(SceneUtils.DoesSceneExist(GameConsts.SCENE_MENU));
        }

        public override void OnClick()
        {
            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UILevelQuitPopUp.Show((confirmed) =>
            {
                if (confirmed)
                {
                    if (GameController.is_AB_VideoIsB_Local)
                    {
                        if (GameController.ShouldCheckMonthlyWinStreak())
                        {
                            UIPopResetGame.Show((closeType) =>
                            {
                                if (closeType == UIPopResetGame.CloseType.RvTouch || closeType == UIPopResetGame.CloseType.LostTouch)
                                {
                                    if (closeType == UIPopResetGame.CloseType.LostTouch)
                                        MonthlyCtrl.Instance.OnLevelFail();
                                    LoadMenu();
                                }
                            });
                        }
                        else
                        {
                            LoadMenu();
                        }
                    }
                    else
                    {
                        LoadMenu();
                    }
                }
            });



        }

        private void LoadMenu()
        {
            // Show fullscreen black overlay
            Overlay.Show(0.3f, () =>
            {
                AdsManager.ShowInterstitial((result) =>
                {
                    LivesSystem.UnlockLife(true);

                    // Save the current state of the game
                    SaveController.Save(true);

                    // Unload the current level and all the dependencies
                    GameController.LoadMenu();

                }, AnalyticsStr.inter_back_menu);
            });
        }

        public override void Select()
        {
            IsSelected = true;

            Button.Select();

            EventSystem.current.SetSelectedGameObject(null); //clear any previous selection (best practice)
            EventSystem.current.SetSelectedGameObject(Button.gameObject, new BaseEventData(EventSystem.current));
        }

        public override void Deselect()
        {
            IsSelected = false;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}