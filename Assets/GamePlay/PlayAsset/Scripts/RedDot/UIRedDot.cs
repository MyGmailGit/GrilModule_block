// UIRedDot.cs
using UnityEngine;
using UnityEngine.UI;
namespace Game.RedDot
{
    public class UIRedDot : MonoBehaviour
    {
        [SerializeField] private RedDotDefine.Node nodeId;
        [SerializeField] private GameObject redDotObject; // 红点显示对象
        [SerializeField] private Text countText; // 可选：显示数量的文本

        private System.Action<bool, int> redDotListener;

        void Start()
        {
            if (redDotObject == null)
                redDotObject = gameObject;

            redDotListener = OnRedDotChanged;
            RedDotSystem.Instance.RegisterListener(nodeId, redDotListener);
        }

        private void OnRedDotChanged(bool isActive, int count)
        {
            if (redDotObject != null)
                redDotObject.SetActive(isActive);

            if (countText != null)
            {
                countText.gameObject.SetActive(isActive && count > 0);
                if (count > 0)
                    countText.text = count > 99 ? "99+" : count.ToString();
            }
        }

        void OnDestroy()
        {
            if (RedDotSystem.Instance != null)
                RedDotSystem.Instance.UnregisterListener(nodeId, redDotListener);
        }

        // 编辑器工具：自动查找红点对象
        void OnValidate()
        {
            if (redDotObject == null && transform.Find("RedDot") != null)
                redDotObject = transform.Find("RedDot").gameObject;
        }
    }
}