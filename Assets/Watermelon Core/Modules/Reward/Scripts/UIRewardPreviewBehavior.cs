using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIRewardPreviewBehavior : MonoBehaviour
    {
        [SerializeField] Image image;
        public Image Image => image;

        [SerializeField] TextMeshProUGUI text;
        public TextMeshProUGUI Text => text;

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup => canvasGroup;

        private IRewardPreview rewardPreview;

        public void Init(IRewardPreview rewardPreview, Sprite defaultSprite)
        {
            this.rewardPreview = rewardPreview;

            canvasGroup = GetComponent<CanvasGroup>();

            if (image != null)
            {
                Sprite sprite = rewardPreview.Icon;
                if (sprite == null)
                    sprite = defaultSprite;

                image.sprite = sprite;

                image.SetNativeSize();
                // 无论事宽高哪一个超过280都缩放到280，如果最大的边低于280依然缩放到280

                Vector2 size = image.rectTransform.sizeDelta;
                float maxSide = Mathf.Max(size.x, size.y);

                // 无论最大边是否超过 280，都缩放到 280
                float targetMaxSide = 180f;
                float scale = targetMaxSide / maxSide;

                image.rectTransform.sizeDelta = size * scale;
            }

            if (text != null)
                text.text = rewardPreview.Text;

            OnInitialized();
        }

        protected virtual void OnInitialized() { }
    }
}
