using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Watermelon
{
    public class UISettings : UIPage, IPopupWindow//, IPausePopup
    {
        [BoxGroup("References", "References")]
        [SerializeField] Image backgroundImage;
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform panelRectTransform;
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform contentRectTransform;
        public RectTransform ContentRectTransform => contentRectTransform;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;

        private float fadeIntensity;

        public bool IsOpened => canvas.enabled;

        public override void Init()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            backgroundImage.AddEvent(EventTriggerType.PointerDown, OnBackgroundClicked);

            fadeIntensity = backgroundImage.color.a;
        }

        public override void PlayShowAnimation()
        {
            RecalculatePanelSize();

            panelRectTransform.anchoredPosition = Vector2.down * 2000;
            panelRectTransform.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutSine);

            backgroundImage.SetAlpha(0f);
            backgroundImage.DOFade(fadeIntensity, 0.3f).OnComplete(() =>
            {
                UIController.OnPageOpened(this);
            });
        }

        public override void PlayHideAnimation()
        {
            panelRectTransform.DOAnchorPos(Vector2.down * 2000, 0.3f).SetEase(Ease.InSine);

            backgroundImage.DOFade(0f, 0.3f).OnComplete(() =>
            {
                UIController.OnPageClosed(this);
            });
        }

        private void RecalculatePanelSize()
        {
            if (contentRectTransform.GetComponent<LayoutGroup>() == null) return;

            float height = Mathf.Abs(contentRectTransform.sizeDelta.y);

            int childCount = contentRectTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform childTransform = contentRectTransform.GetChild(i);
                if (childTransform != null)
                {
                    SettingsElementsGroup settingsElementsGroup = childTransform.GetComponent<SettingsElementsGroup>();
                    if (settingsElementsGroup != null)
                    {
                        if (settingsElementsGroup.IsGroupActive())
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y;

                            settingsElementsGroup.gameObject.SetActive(true);
                        }
                        else
                        {
                            settingsElementsGroup.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (childTransform.gameObject.activeSelf)
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y;
                        }
                    }
                }
            }

            panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, height);
        }

        public void OnCloseButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UISettings>();
        }

        private void OnBackgroundClicked(PointerEventData data)
        {
            UIController.HidePage<UISettings>();
        }
    }
}
