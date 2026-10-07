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

        private BottleController firstBlock;
        private BottleController secondBlock;

        private bool is1stTouch = false;
        private bool is2ndTouch = false;

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
                DOVirtual.DelayedCall(0.2f, () => StartTutorial());
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
            var bottles = LevelController.GetFirstAndSecondBottle();

            firstBlock = bottles.firstBottle;
            secondBlock = bottles.secondBottle;
            if (firstBlock == null) return;

            // List<LevelBlockBehavior> activeBlocks = LevelController.LevelRepresentation.ActiveBlocks;

            // firstBlock = activeBlocks[0];
            // secondBlock = activeBlocks[1];

            firstBlock.OnBottleClickedOnly = OnFirstBlockCollected;
            secondBlock.OnBottleClickedOnly = OnSecondBlockCollected;

            EnableFirstBlockPointer();
        }

        private void EnableFirstBlockPointer()
        {
            // Bounds bounds = firstBlock.transform.position;

            secondBlock.SetIsTouchEnableFor(false);

            gameUI.MessageBox.Activate(string.Format(firstMessage, textHighlightColor.ToHex()));
            gameUI.MessageBox.ActivateTutorial();

            RectTransform messageRectTransform = gameUI.MessageBox.RectTransform;

            TutorialCanvasController.AlignToCorner(messageRectTransform, TutorialCanvasController.UIAnchorCorner.TopCenter, new Vector2(0, -360));

            TutorialCanvasController.ActivatePointerWithFirstTipMask(firstBlock.transform.position, TutorialCanvasController.POINTER_CLICK);

        }

        private void EnableSecondBlockPointer()
        {
            // Bounds bounds = secondBlock.Figure.GetHorizontalCenterBounds();

            secondBlock.SetIsTouchEnableFor(true);
            firstBlock.SetIsTouchEnableFor(false);

            gameUI.MessageBox.Activate(string.Format(secondMessage, textHighlightColor.ToHex()));
            gameUI.MessageBox.ActivateTutorial();

            RectTransform messageRectTransform = gameUI.MessageBox.RectTransform;

            TutorialCanvasController.AlignToCorner(messageRectTransform, TutorialCanvasController.UIAnchorCorner.TopCenter, new Vector2(0, -360));

            TutorialCanvasController.ActivatePointerWithFirstTipMask(secondBlock.transform.position, TutorialCanvasController.POINTER_CLICK);
        }

        public void OnFirstBlockCollected(BottleController bottleController)
        {
            gameUI.MessageBox.Disable();
            TutorialCanvasController.ResetPointer();

            firstBlock.OnBottleClickedOnly = null;

            is1stTouch = true;

            if (!is2ndTouch)
            {
                EnableSecondBlockPointer();
            }
            else
            {
                FinishTutorial();
            }
        }

        public void OnSecondBlockCollected(BottleController bottleController)
        {
            is2ndTouch = true;

            firstBlock.OnBottleClickedOnly = null;
            secondBlock.OnBottleClickedOnly = null;

            if (is1stTouch)
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
