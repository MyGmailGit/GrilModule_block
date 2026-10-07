#pragma warning disable 0649

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace Watermelon
{
    public class FeatureAnnouncementPopup : UIPage, IPopupWindow//, IPausePopup
    {
        [SerializeField] RectTransform safeAreaTransform;

        [Space]
        [SerializeField] TMP_Text titleText;
        [SerializeField] Transform shinesHolderTransform;
        [SerializeField] Image previewImage;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] TMP_Text actionText;

        [Space(5)]
        [SerializeField] Button closeButton1;
        [SerializeField] Button closeButton2;

        public bool IsOpened => canvas.enabled;

        private List<FeatureAnnouncementPopupData> activeFeatures;
        private int pageIndex = 0;

        private static List<FeatureAnnouncementPopupData> newFeatures = new List<FeatureAnnouncementPopupData>();

        public override void Init()
        {
            closeButton1.onClick.AddListener(ClosePanel);
            closeButton2.onClick.AddListener(ClosePanel);

            NotchSaveArea.RegisterRectTransform(safeAreaTransform);
        }

        public override void PlayShowAnimation()
        {
            if (newFeatures.IsNullOrEmpty())
                return;

            activeFeatures = new List<FeatureAnnouncementPopupData>();
            foreach (FeatureAnnouncementPopupData feature in newFeatures)
            {
                activeFeatures.Add(feature);
            }
            newFeatures.Clear();

            pageIndex = 0;

            PreparePage(pageIndex);

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            titleText.transform.DOScale(0f, 0.25f).SetEase(Ease.InCubic);

            previewImage.transform.DOScale(0f, 0.25f).SetDelay(0.1f).SetEase(Ease.InCubic);

            shinesHolderTransform.transform.DOScale(0f, 0.25f).SetEase(Ease.InCubic);

            descriptionText.DOFade(0f, 0.25f).SetDelay(0.15f).SetEase(Ease.InCubic);

            DOVirtual.DelayedCall(0.4f, () =>
            {
                UIController.OnPageClosed(this);
            });
        }

        private void PreparePage(int index)
        {
            if (!activeFeatures.IsInRange(index))
                return;

            closeButton1.interactable = false;
            closeButton2.interactable = false;

            FeatureAnnouncementPopupData featureData = activeFeatures[index];

            previewImage.sprite = featureData.PreviewSprite;
            descriptionText.text = featureData.Description;

            titleText.transform.localScale = Vector3.zero;
            titleText.transform.DOScale(1.0f, 0.25f).SetDelay(0.4f).SetEase(Ease.OutBack);

            previewImage.transform.localScale = Vector3.zero;
            previewImage.transform.DOScale(1.0f, 0.25f).SetDelay(0.65f).SetEase(Ease.OutBack);

            shinesHolderTransform.transform.localScale = Vector3.zero;
            shinesHolderTransform.transform.DOScale(1.0f, 0.5f).SetDelay(0.8f).SetEase(Ease.OutBack);

            descriptionText.color = descriptionText.color.SetAlpha(0);
            descriptionText.DOFade(1f, 0.5f).SetDelay(0.8f).SetEase(Ease.InSine).OnComplete(() =>
            {
                closeButton1.interactable = true;
                closeButton2.interactable = true;
            });
        }

        public void ClosePanel()
        {
            pageIndex++;

            if (pageIndex >= activeFeatures.Count)
            {
                UIController.HidePage<FeatureAnnouncementPopup>();
            }
            else
            {
                PreparePage(pageIndex);
            }

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public static void ShowAnnouncementIfExists()
        {
            if (newFeatures.IsNullOrEmpty())
                return;

            UIController.ShowPage<FeatureAnnouncementPopup>();
        }

        public static void RegisterFeature(FeatureAnnouncementPopupData feature)
        {
            if (!Application.isPlaying || !feature.ShowAnnouncementPopup)
                return;

            newFeatures.Add(feature);
        }
    }
}
