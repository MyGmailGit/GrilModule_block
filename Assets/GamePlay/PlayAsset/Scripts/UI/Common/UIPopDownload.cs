using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UIPopDownload : UIPage, IPopupWindow
{
    public static List<string> downloadFiles;
    public static void Show(List<string> downlaodFileName, bool isShowInQueue = false)
    {
        downloadFiles = downlaodFileName;
        if (isShowInQueue)
        {
            UIPopupQueueManager.Instance.EnqueuePopup<UIPopDownload>();
        }
        else
        {
            UIController.ShowPage<UIPopDownload>();
        }
    }

    [SerializeField] private Button closeBtn;
    [SerializeField] private Image progressImg;
    [SerializeField] private TextMeshProUGUI progressTxt;
    // [SerializeField] private TextMeshProUGUI FailOrCloseTip;

    private Dictionary<string, float> progressData = new Dictionary<string, float>();
    private List<string> downloadFilesUrl = new();
    private Dictionary<string, bool> downloadSuccessMark = new();

    public bool IsOpened => canvas.enabled;

    public override void Init()
    {
        closeBtn.onClick.AddListener(() =>
        {
            SystemMessage.ShowMessage("Images saved! Downloading in background. Check your album.");
            UIController.HidePage(this);

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        });
    }

    public override void PlayHideAnimation()
    {
        foreach (var it in downloadFilesUrl)
        {
            PriorityDownloadManager.Instance.RemoveCallbacks(it, NotifyImageDownloaded, OnDownloadFail, DownloadProgress);
        }
        UIController.OnPageClosed(this);
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);
        progressData.Clear();
        downloadFilesUrl.Clear();
        downloadSuccessMark.Clear();
        progressImg.fillAmount = 0;
        progressTxt.text = "";

        foreach (var it in downloadFiles)
        {
            progressData.Add(it, 0);
            downloadSuccessMark.Add(it, false);

            var Url = VideoImgResourceManager.Instance.MakePngDownloadUrl(it);
            downloadFilesUrl.Add(Url);
            PriorityDownloadManager.Instance.AddDownload(Url, it, NotifyImageDownloaded, OnDownloadFail, DownloadProgress);
        }
    }

    public void DownloadProgress(string fileId, float pro)
    {
        if (progressData.ContainsKey(fileId))
        {
            progressData[fileId] = pro;

            float fullPro = 0;
            foreach (var it in progressData)
            {
                fullPro += it.Value;
            }
            progressTxt.text = $"{fullPro:0.0}/{progressData.Count}";
            progressImg.fillAmount = fullPro / progressData.Count;
        }
    }

    public void NotifyImageDownloaded(string imageId, byte[] textureByte)
    {
        VideoImgResourceManager.Instance.NotifyImageDownloaded(imageId, textureByte);

        if (downloadSuccessMark.ContainsKey(imageId))
        {
            downloadSuccessMark[imageId] = true;
            CheckFinish();
        }
    }

    public void OnDownloadFail(string str)
    {
        UnityEngine.Debug.Log($"{str}:Image downlaod Fail");
    }

    private void CheckFinish()
    {
        bool Allfinish = true;
        foreach (var it in downloadSuccessMark)
        {
            if (it.Value == false)
            {
                Allfinish = false;
                break;
            }
        }

        if (Allfinish)
        {
            UISurpriseSpecialGetPop.Show(downloadFiles);

            UIController.HidePage(this);
        }
    }

}