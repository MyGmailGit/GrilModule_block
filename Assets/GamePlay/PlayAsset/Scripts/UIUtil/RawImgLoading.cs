using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;

public class RawImgLoading : MonoBehaviour
{
    [SerializeField] RawImage rawIcon = null;
    [SerializeField] Transform laodingImg;

    private int _currentRequestId = -1;
    private string _currentImageId;

    private Action onLoadCallback = null;

    public void LoadImage(string imgId, Action callback = null)
    {
        if (rawIcon == null) rawIcon = GetComponent<RawImage>();

        if (_currentImageId != imgId)
        {
            onLoadCallback = callback;
            // Context.SelectedIndex = Index;
            rawIcon.texture = null;

            VideoImgResourceManager.Instance.UnPin(_currentImageId);

            VideoImgResourceManager.Instance.Pin(imgId);

            if (_currentRequestId != -1 && !string.IsNullOrEmpty(_currentImageId))
            {
                VideoImgResourceManager.Instance.CancelLoad(_currentImageId, _currentRequestId);
            }

            _currentImageId = imgId;

            // 发起新的加载请求
            _currentRequestId = VideoImgResourceManager.Instance.GetImage(_currentImageId, OnIconLoad);

            // laodingImg?.gameObject.SetActive(true);
        }
    }
    private void OnIconLoad(Texture2D tex)
    {
        // laodingImg?.gameObject.SetActive(false);
        rawIcon.texture = tex;
        onLoadCallback?.Invoke();
        onLoadCallback = null;
    }

    public void Release()
    {
        VideoImgResourceManager.Instance.UnPin(_currentImageId);
        _currentImageId = null;
    }
    public void ODestroy()
    {
        Release();
    }



}
