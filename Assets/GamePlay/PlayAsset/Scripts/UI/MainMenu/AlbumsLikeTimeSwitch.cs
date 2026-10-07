using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class AlbumsLikeTimeSwitch : MonoBehaviour
{
    public GameObject firstShow;
    public GameObject secondShow;
    public int index;
    public Button touchBtn;

    public Action<int, AlbumsLikeTimeSwitch> switchTouchCall;

    public bool IsShowFirst = true;

    void Start()
    {
        touchBtn.onClick.AddListener(OnTouch);
        SetShowBool(IsShowFirst);
    }

    private void OnTouch()
    {
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        switchTouchCall?.Invoke(index, this);
    }
    public void RevertShow()
    {
        SetShowBool(!this.IsShowFirst);
    }
    public void SetShowBool(bool isShowFirst)
    {
        if (isShowFirst)
        {
            firstShow.gameObject.SetActive(true);
            secondShow.gameObject.SetActive(false);
        }
        else
        {
            firstShow.gameObject.SetActive(false);
            secondShow.gameObject.SetActive(true);
        }
        this.IsShowFirst = isShowFirst;
    }
}
