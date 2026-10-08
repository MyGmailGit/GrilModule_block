using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;

namespace Watermelon
{
    public class FirstLevelTutorial : BaseTutorial
    {
        [Space]
        [SerializeField] Color textHighlightColor = Color.red;

        [Space]
        [SerializeField] string firstMessage = "<color={0}>Tap</color> to unscrew!";
        [SerializeField] string secondMessage = "<color={0}>Tap</color> to place screw!";

        public override bool IsActive => saveData.isActive;
        public override bool IsFinished => saveData.isFinished;
        public override int Progress => saveData.progress;

        private TutorialBaseSave saveData;

        private UIGame gameUI;

        private LevelBlockBehavior firstBlock;
        private LevelBlockBehavior secondBlock;

        public override void Init()
        {
            if (isInitialised) return;

            isInitialised = true;

            // Load save file
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER, tutorialId.ToString()));

            gameUI = UIController.GetPage<UIGame>();

            LevelController.InvokeOrWait(OnLevelLoaded);
        }

        private void OnLevelLoaded()
        {
            ActiveSession activeSession = ActiveSession.Current;
            if (activeSession.DisplayLevelIndex == 0 && !activeSession.IsPlaySpecialLevel())
            {
                DOVirtual.DelayedCall(0.1f, () => StartTutorial());
            }
        }

        public override void Unload()
        {
            isInitialised = false;
        }

        public override void FinishTutorial()
        {
            if (saveData.isFinished) return;

            saveData.isFinished = true;
        }

        public override void StartTutorial()
        {
            List<LevelBlockBehavior> activeBlocks = LevelController.LevelRepresentation.ActiveBlocks;

            firstBlock = activeBlocks[0];
            secondBlock = activeBlocks[1];

            firstBlock.BlockCollected += OnFirstBlockCollected;
            secondBlock.BlockCollected += OnSecondBlockCollected;

            EnableFirstBlockPointer();
        }

        private void EnableFirstBlockPointer()
        {
            Bounds bounds = firstBlock.Figure.GetHorizontalCenterBounds();

            gameUI.MessageBox.Activate(string.Format(firstMessage, textHighlightColor.ToHex()));
            gameUI.MessageBox.ActivateTutorial();

            RectTransform messageRectTransform = gameUI.MessageBox.RectTransform;

            TutorialCanvasController.AlignToCorner(messageRectTransform, TutorialCanvasController.UIAnchorCorner.TopCenter, new Vector2(0, -360));

            TutorialCanvasController.ActivatePointer(firstBlock.transform.position + bounds.center, TutorialCanvasController.POINTER_SWIPE_DOWN);
        }

        private void EnableSecondBlockPointer()
        {
            Bounds bounds = secondBlock.Figure.GetHorizontalCenterBounds();

            gameUI.MessageBox.Activate(string.Format(secondMessage, textHighlightColor.ToHex()));
            gameUI.MessageBox.ActivateTutorial();

            RectTransform messageRectTransform = gameUI.MessageBox.RectTransform;

            TutorialCanvasController.AlignToCorner(messageRectTransform, TutorialCanvasController.UIAnchorCorner.TopCenter, new Vector2(0, -360));

            TutorialCanvasController.ActivatePointer(secondBlock.transform.position + bounds.center, TutorialCanvasController.POINTER_SWIPE_UP);
        }

        public void OnFirstBlockCollected()
        {
            gameUI.MessageBox.Disable();
            TutorialCanvasController.ResetPointer();

            if (!secondBlock.IsCollected)
            {
                EnableSecondBlockPointer();
            }
            else
            {
                FinishTutorial();
            }
        }

        public void OnSecondBlockCollected()
        {
            if (firstBlock.IsCollected)
            {
                gameUI.MessageBox.Disable();
                TutorialCanvasController.ResetPointer();

                FinishTutorial();
            }
            else
            {
                gameUI.MessageBox.Activate(string.Format(secondMessage, textHighlightColor.ToHex()));
            }
        }
    }
}
