using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Watermelon
{
    public class UIRewardsConfirmation : UIPage, IPopupWindow
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] RectTransform rewardsContainerTransform;
        [SerializeField] GridLayoutGroup rewardsGridLayoutGroup;
        [SerializeField] GameObject rewardUIPrefab;
        [SerializeField] TMP_Text tapToClaimText;

        [Space]

        [Space]
        [SerializeField] Sprite defaultRewardSprite;

        [Space]
        [SerializeField] AudioClip displayAudioClip;

        public bool IsOpened => canvas.enabled;

        private SimpleCallback closeCallback;
        private RewardUIData[] rewards;

        private Tween fadeTweenCase;

        private float rewardItemScale = 1f;

        public override void Init()
        {
            backgroundImage.AddEvent(UnityEngine.EventSystems.EventTriggerType.PointerUp, (data) => OnCloseButtonClicked());
        }

        public override void PlayShowAnimation()
        {
            float appearanceDelay = 0.1f;

            if (!rewards.IsNullOrEmpty())
            {
                UpdateDynamicSize();

                for (int i = 0; i < rewards.Length; i++)
                {
                    RewardUIData reward = rewards[i];

                    UIRewardPreviewBehavior behavior = reward.Behavior;
                    behavior.transform.localScale = Vector3.one * rewardItemScale;

                    behavior.CanvasGroup.alpha = 0.0f;

                    reward.FadeTweenCase = behavior.CanvasGroup.DOFade(1.0f, 0.45f).SetDelay(appearanceDelay * (i + 1));

                    reward.ScaleTweenCase = behavior.Image.transform.DOScale(1.1f, 0.15f).SetDelay(appearanceDelay * (i + 1)).SetEase(Ease.OutSine).OnComplete(() =>
                    {
                        reward.ScaleTweenCase = behavior.Image.transform.DOScale(0.95f, 0.2f).OnComplete(() =>
                        {
                            reward.ScaleTweenCase = behavior.Image.transform.DOScale(1f, 0.1f).SetEase(Ease.OutSine);
                        });
                    });
                }
            }

            float tapTextDelay = rewards.IsNullOrEmpty() ? 0f : appearanceDelay * rewards.Length + 0.5f;

            tapToClaimText.color = tapToClaimText.color.SetAlpha(0f);
            fadeTweenCase = tapToClaimText.DOFade(1f, 0.2f).SetDelay(tapTextDelay).SetEase(Ease.InOutCubic);

            if (displayAudioClip != null)
                AudioController.PlaySound(displayAudioClip);

            UIController.OnPageOpened(this);
        }

        public void UpdateDynamicSize()
        {
            if (rewards.Length < 5)
            {
                rewardsGridLayoutGroup.cellSize = new Vector2(300f, 300f);
                rewardItemScale = 1f;
            }
            else
            {
                rewardsGridLayoutGroup.cellSize = new Vector2(200f, 200f);
                rewardItemScale = 0.8f;
            }
        }

        public override void PlayHideAnimation()
        {
            float animationDuration = 0f;

            if (!rewards.IsNullOrEmpty())
            {
                animationDuration = 0.3f;

                for (int i = 0; i < rewards.Length; i++)
                {
                    RewardUIData reward = rewards[i];

                    UIRewardPreviewBehavior behavior = reward.Behavior;
                    behavior.CanvasGroup.alpha = 1f;

                    reward.FadeTweenCase = behavior.CanvasGroup.DOFade(0f, 0.28f);

                    reward.ScaleTweenCase = behavior.Image.transform.DOScaleY(1.1f, 0.05f).SetDelay(0.1f).SetEase(Ease.InSine).OnComplete(() =>
                    {
                        reward.ScaleTweenCase = behavior.Image.transform.DOScaleY(0f, 0.15f).SetEase(Ease.OutSine);
                    });
                }
            }

            DOVirtual.DelayedCall(animationDuration, () =>
            {
                closeCallback?.Invoke();
                UIController.OnPageClosed(this);
            });
        }

        private void OnDestroy()
        {
            fadeTweenCase.Kill();

            if (!rewards.IsNullOrEmpty())
            {
                foreach (RewardUIData reward in rewards)
                {
                    reward?.Destroy();
                }
            }
        }

        public static bool Display(List<IRewardPreview> previews, SimpleCallback closeCallback = null, bool isShowInQueue = false)
        {
            if (previews.IsNullOrEmpty())
                return false;

            UIRewardsConfirmation rewardsConfirmation = UIController.GetPage<UIRewardsConfirmation>();
            if (rewardsConfirmation != null)
            {
                rewardsConfirmation.closeCallback = closeCallback;

                RewardUIData[] oldRewards = rewardsConfirmation.rewards;
                if (!oldRewards.IsNullOrEmpty())
                {
                    foreach (RewardUIData reward in oldRewards)
                    {
                        reward?.Destroy();
                    }
                }

                previews = previews.OrderBy(x => x.SortingOrder).ToList();

                RewardUIData[] newRewards = new RewardUIData[previews.Count];
                for (int i = 0; i < newRewards.Length; i++)
                {
                    IRewardPreview preview = previews[i];

                    GameObject uiPrefab = preview.GetCustomUIPrefab();
                    if (uiPrefab == null)
                        uiPrefab = rewardsConfirmation.rewardUIPrefab;

                    GameObject uiObject = Instantiate(uiPrefab, rewardsConfirmation.rewardsContainerTransform);
                    uiObject.transform.ResetLocal();

                    UIRewardPreviewBehavior previewBehavior = uiObject.GetComponent<UIRewardPreviewBehavior>();
                    previewBehavior.Init(preview, rewardsConfirmation.defaultRewardSprite);

                    newRewards[i] = new RewardUIData(previewBehavior, preview);
                }

                rewardsConfirmation.rewards = newRewards;

                if (isShowInQueue)
                {
                    UIPopupQueueManager.Instance.EnqueuePopup<UIRewardsConfirmation>();
                }
                else
                {
                    UIController.ShowPage<UIRewardsConfirmation>();
                }

                return true;
            }

            return false;
        }

        private void OnCloseButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage(this);
        }

        public class RewardUIData
        {
            public readonly UIRewardPreviewBehavior Behavior;
            public readonly IRewardPreview Preview;

            public Tween FadeTweenCase;
            public Tween ScaleTweenCase;

            public RewardUIData(UIRewardPreviewBehavior behavior, IRewardPreview preview)
            {
                Behavior = behavior;
                Preview = preview;
            }

            public void Destroy()
            {
                if (Behavior != null && Behavior.gameObject != null)
                    GameObject.Destroy(Behavior.gameObject);

                FadeTweenCase.Kill();
                ScaleTweenCase.Kill();
            }
        }
    }
}
