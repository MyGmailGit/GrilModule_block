using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Watermelon.IAPStore
{
    public class UIStore : IMainPage
    {
        private const int OVERLAY_SORTING_LAYER = 110;
        private const float DEFAULT_STORE_HEIGHT_OFFSET = 300;

        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaTransform;
        [BoxGroup("References")]
        [SerializeField] CurrencyUIPanelSimple coinsUI;
        [BoxGroup("References")]
        [SerializeField] CurrencyUIPanelSimple diamondUI;

        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] VerticalLayoutGroup layout;
        [BoxGroup("Scroll View")]
        [SerializeField] RectTransform content;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;
        [SerializeField] AdsRewardsHolder adsRewardsHolder;

        [SerializeField] SwitchPanelItem buyPanelButton;
        [SerializeField] SwitchPanelItem puPanelButton;

        [SerializeField] ScrollRect currencyScroll;
        [SerializeField] ScrollRect puScroll;


        private IStoreElement[] offersElements;

        private void Awake()
        {
            offersElements = new IStoreElement[content.childCount];
            for (int i = 0; i < offersElements.Length; i++)
            {
                Transform child = content.GetChild(i);

                IStoreElement storeElement = child.GetComponent<IStoreElement>();
                if (storeElement == null)
                {
                    storeElement = new DefaultStoreElement((RectTransform)child);
                }

                storeElement.Init();

                offersElements[i] = storeElement;
            }

            // closeButton.onClick.AddListener(OnCloseButtonClicked);

            buyPanelButton.touchAction = OnSwitchPanelTouch;
            puPanelButton.touchAction = OnSwitchPanelTouch;
        }
        public override void Init()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaTransform);

            coinsUI.Init();
            diamondUI.Init();
        }
        public override void Hide()
        {
            base.Hide();
            PlayHideAnimation();
        }
        public override void Show()
        {
            base.Show();
            PlayShowAnimation();
        }

        public void PlayHideAnimation()
        {
            // UIController.OnPageClosed(this);
            currencyScroll.gameObject.SetActive(true);
            puScroll.gameObject.SetActive(false);
        }

        public void PlayShowAnimation()
        {
            currencyScroll.gameObject.SetActive(true);
            puScroll.gameObject.SetActive(false);
            buyPanelButton.SetState(true);
            puPanelButton.SetState(false);

            float height = layout.padding.top + layout.padding.bottom + DEFAULT_STORE_HEIGHT_OFFSET;

            IStoreElement[] activeOffers = offersElements.Where(x => x.IsActive).ToArray();
            for (int i = 0; i < activeOffers.Length; i++)
            {
                IStoreElement offer = activeOffers[i];

                offer.KillTweenCases();
                offer.PlayAnimation(i);

                height += offer.Height;
            }

            height += activeOffers.Length * layout.spacing;

            // closeButton.transform.localScale = Vector3.zero;
            // closeButton.transform.DOScale(1.0f, 0.3f).SetDelay(0.2f).SetEase(Ease.OutBack);

            content.sizeDelta = new Vector2(0, height);
            content.anchoredPosition = Vector2.zero;

            // UIController.OnPageOpened(this);

            adsRewardsHolder.Refresh();
        }
        /*
                public void Hide()
                {
                    foreach (IStoreElement offer in offersElements)
                    {
                        offer.KillTweenCases();
                    }

                    // UIController.HidePage<UIStore>();
                }
        */

        public void MoveToCoinOrDiamondPanel(int coindiamond = 0)
        {
            if (coindiamond == 0) return;
            if (coindiamond == 1)
            {
                (content.transform as RectTransform).DOAnchorPosY(2000, 0.35f);
                return;
            }
            if (coindiamond == 2)
            {
                (content.transform as RectTransform).DOAnchorPosY(1100, 0.35f);
                return;
            }

        }

        private void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // UIController.HidePage<UIStore>();
        }

        // public void SpawnCurrencyCloud(RectTransform spawnRectTransform, CurrencyType currencyType, int amount, SimpleCallback completeCallback = null)
        // {
        //     FloatingCloud.SpawnCurrency(currencyType.ToString(), spawnRectTransform, coinsUI.RectTransform, amount, null, completeCallback);
        // }

        private void OnSwitchPanelTouch(SwitchPanelItem item)
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            item.SetState(true);
            if (item == buyPanelButton)
            {
                puPanelButton.SetState(false);

                currencyScroll.gameObject.SetActive(true);
                puScroll.gameObject.SetActive(false);
            }
            else
            {
                buyPanelButton.SetState(false);

                currencyScroll.gameObject.SetActive(false);
                puScroll.gameObject.SetActive(true);
            }
        }
    }
}