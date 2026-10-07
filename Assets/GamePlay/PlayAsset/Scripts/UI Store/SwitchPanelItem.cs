using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class SwitchPanelItem : MonoBehaviour
{
    [SerializeField]
    Button button;
    [SerializeField]
    TextMeshProUGUI textMeshProUGUI;
    [SerializeField]
    Color[] colors;
    [SerializeField]
    Sprite[] sprites;
    [SerializeField]
    bool defaultActive = true;
    public Action<SwitchPanelItem> touchAction;

    void Start()
    {
        button.onClick.AddListener(OnbuttonClick);

        SetState(defaultActive);
    }

    private void OnbuttonClick()
    {
        touchAction?.Invoke(this);

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    public void SetState(bool isActive)
    {
        button.image.sprite = isActive ? sprites[0] : sprites[1];

        textMeshProUGUI.color = isActive ? colors[0] : colors[1];
    }
}
