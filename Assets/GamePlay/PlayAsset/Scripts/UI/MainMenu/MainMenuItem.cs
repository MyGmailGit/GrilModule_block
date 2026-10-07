using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class MainMenuItem : MonoBehaviour
{
    public enum MainMenuType
    {
        Shop = 0,
        Surprise = 1,
        MainPage = 2,
        Album = 3,
        Monthly = 4,
    }

    [SerializeField] private Image menuIcon;
    [SerializeField] private Image lockImg;
    [SerializeField] private TextMeshProUGUI unlockTxt;
    [SerializeField] private Button menuBtn;
    [SerializeField] private TextMeshProUGUI menutxt;
    [SerializeField] private MainMenuType itemIdx;

    [SerializeField] private Image chooseImage;

    // 选中状态
    [SerializeField] private bool isSelected = false;

    // 回调事件，点击时触发
    private Action<MainMenuType> onClickCallback;

    // 存储原始状态
    private Vector2 originalSize;
    private Vector3 originalIconScale;
    private Vector3 originalIconPosition;
    private Color originalTextColor;

#if UNITY_EDITOR
    private const int SurpriseMenuUnlock = 1;
#else
    private const int SurpriseMenuUnlock = 3;
#endif

    // 常量定义
    private float SELECTED_WIDTH_radio = 1.2962963f;
    private float UNSELECTED_WIDTH_radio = 0.92592593f;
    private float SELECTED_WIDTH = 280f;
    private float UNSELECTED_WIDTH = 200f;


    private const float SELECTED_ICON_SCALE = 1.5f;
    private const float SELECTED_ICON_OFFSET_Y = 30f;
    private readonly Color SELECTED_TEXT_COLOR = Color.white;
    private readonly Color UNSELECTED_TEXT_COLOR = new Color(1f, 1f, 1f, 0.5f); // 白色半透明

    private void Awake()
    {
        // 保存原始状态
        if (menuIcon != null)
        {
            originalIconScale = menuIcon.transform.localScale;
            originalIconPosition = menuIcon.transform.localPosition;
        }

        if (menutxt != null)
        {
            originalTextColor = menutxt.color;
        }

        // 获取原始大小
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalSize = rectTransform.sizeDelta;
        }

        // 绑定按钮点击事件
        if (menuBtn != null)
        {
            menuBtn.onClick.AddListener(OnButtonClick);
        }
    }

    IEnumerator Start()
    {
        yield return null;
        lockImg.gameObject.SetActive(false);
        if (itemIdx == MainMenuType.Surprise)
        {
            ActiveSession activeSession = ActiveSession.Current;
            if (activeSession.DisplayLevelIndex < SurpriseMenuUnlock)
            {
                lockImg.gameObject.SetActive(true);
                unlockTxt.text = $"Unlock\nLevel {SurpriseMenuUnlock}";
            }
        }
        else if (itemIdx == MainMenuType.Monthly)
        {
            if (!MonthlyCtrl.Instance.IsStartMonthlyTask())
            {
                lockImg.gameObject.SetActive(true);
                unlockTxt.text = $"Unlock\nLevel {MonthlyCtrl.StartMonthlyLevel}";
            }
        }
        yield return null;
        var parentRect = transform.parent as RectTransform;
        var width = parentRect.rect.width;
        int count = Enum.GetValues(typeof(MainMenuType)).Length;
        var perWidth = width / count;
        SELECTED_WIDTH = perWidth * SELECTED_WIDTH_radio;
        UNSELECTED_WIDTH = perWidth * UNSELECTED_WIDTH_radio;

        UpdateVisualState();
    }

    private void OnDestroy()
    {
        // 清理事件监听
        if (menuBtn != null)
        {
            menuBtn.onClick.RemoveListener(OnButtonClick);
        }
    }



    /// <summary>
    /// 设置回调函数
    /// </summary>
    /// <param name="callback">回调函数，参数为按钮索引</param>
    public void SetCallback(Action<MainMenuType> callback)
    {
        onClickCallback = callback;
    }

    /// <summary>
    /// 按钮点击处理
    /// </summary>
    private void OnButtonClick()
    {
        if (lockImg.gameObject.activeSelf)
        {
            SystemMessage.ShowMessage("Play Game to unlock");
            return;
        }

        // 触发回调，传递索引
        onClickCallback?.Invoke(itemIdx);

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    /// <param name="selected">是否选中</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }

    /// <summary>
    /// 获取当前选中状态
    /// </summary>
    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// 更新视觉状态
    /// </summary>
    private void UpdateVisualState()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if (isSelected)
        {
            // 选中状态
            // 1. 宽度变为280
            if (rectTransform != null)
            {
                Vector2 size = rectTransform.sizeDelta;
                size.x = SELECTED_WIDTH;
                rectTransform.sizeDelta = size;
            }

            // 2. icon放大1.5倍，上浮30
            if (menuIcon != null)
            {
                menuIcon.transform.localScale = originalIconScale * SELECTED_ICON_SCALE;

                Vector3 position = originalIconPosition;
                position.y += SELECTED_ICON_OFFSET_Y;
                menuIcon.transform.localPosition = position;
            }

            // 3. text变成金黄色
            if (menutxt != null)
            {
                menutxt.color = SELECTED_TEXT_COLOR;
            }

            chooseImage.gameObject.SetActive(true);
        }
        else
        {
            // 未选中状态
            // 1. 宽度变为200
            if (rectTransform != null)
            {
                Vector2 size = rectTransform.sizeDelta;
                size.x = UNSELECTED_WIDTH;
                rectTransform.sizeDelta = size;
            }

            // 2. icon还原
            if (menuIcon != null)
            {
                menuIcon.transform.localScale = originalIconScale;
                menuIcon.transform.localPosition = originalIconPosition;
            }

            // 3. text变成白色半透明
            if (menutxt != null)
            {
                menutxt.color = UNSELECTED_TEXT_COLOR;
            }

            chooseImage.gameObject.SetActive(false);
        }
    }

    // /// <summary>
    // /// 重置为默认状态（可选）
    // /// </summary>
    // public void ResetToDefault()
    // {
    //     RectTransform rectTransform = GetComponent<RectTransform>();

    //     if (rectTransform != null)
    //     {
    //         rectTransform.sizeDelta = originalSize;
    //     }

    //     if (menuIcon != null)
    //     {
    //         menuIcon.transform.localScale = originalIconScale;
    //         menuIcon.transform.localPosition = originalIconPosition;
    //     }

    //     if (menutxt != null)
    //     {
    //         menutxt.color = originalTextColor;
    //     }

    //     isSelected = false;
    // }
}
// 实现MenuButton类的功能：1.如果被选中就把自己的size.x变成280, menuIcon放大1.5上浮30，text变成金黄色。
//     2.没被选中就size.x变成200，menuIcon还原，text变成white半透明。
//     3.设置回调，如果点击就触发回调并且告诉是哪个索引被点击了