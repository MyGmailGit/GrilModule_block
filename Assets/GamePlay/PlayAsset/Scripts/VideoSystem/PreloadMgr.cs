using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Video;
using GameLogic;
using UnityEngine;
using UnityEngine.Networking;
using Utility;
using VideoSystem;

public class PreloadMgr : Singleton<PreloadMgr>
{
    protected override void OnInit()
    {
        base.OnInit();
    }
    protected override void OnRelease()
    {
        base.OnInit();
    }
    #region 加载角色系列
    /// <summary>
    /// 加载当前这个阶段的所有图和视频，优先下载图片
    /// 如果已经有下载在下载队列，或者正在排队，就不用添加进去
    /// </summary>
    public void PreloadCurrentCharaterStage()
    {
        var startEnd = VideoSerilNumberManager.Instance.GetCurrentStageStartEnd();
        var currentFileName = VideoSerilNumberManager.Instance.GetCurrentFullName();
        if (startEnd == null || currentFileName == null) return;
        for (int i = currentFileName.Value.fileId; i <= startEnd.Value.endIdx; ++i)
        {
            VideoImgResourceManager.Instance.GetImage(VideoSerilNumberManager.FormatMainIdFileId(currentFileName.Value.mainId, i), null);
        }

        for (int i = currentFileName.Value.fileId; i <= startEnd.Value.endIdx; ++i)
        {
            var fileNamefull = VideoSerilNumberManager.FormatMainIdFileId(currentFileName.Value.mainId, i);

            CoroutineTool.Instance.StartACoroutine(DownloadVideo(fileNamefull));
        }

    }
    private IEnumerator DownloadVideo(string filefullName)
    {
        bool isExists = true;
        yield return CheckLocalMp4Exists(filefullName, (isEx) =>
        {
            isExists = isEx;
        });
        yield return null;
        if (isExists) yield break;
        VideoPlayCtrl.DownlaodNetVideo(filefullName);
    }


    private IEnumerator CheckLocalMp4Exists(string filefullName, Action<bool> callback)
    {
        string cachedEncryptedPath = GetCachedEncryptedPath(filefullName);
        if (File.Exists(cachedEncryptedPath))
        {
            callback?.Invoke(true);
            yield break;
        }

        string streamingAssetPath = GetStreamingAssetsPath(ServerUtil.GetVideoFileName(filefullName));
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android需要使用UnityWebRequest读取
        using (UnityWebRequest request = UnityWebRequest.Get(streamingAssetPath))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($" Failed to read source for caching: {request.error}");
                callback?.Invoke(false);
                yield break;
            }
            callback?.Invoke(true);
        }
#else
        if (!File.Exists(streamingAssetPath))
        {
            callback?.Invoke(false);
        }
        else
        {
            callback?.Invoke(true);
        }
#endif

        yield return null;

    }
    private string GetStreamingAssetsPath(string fileName)
    {
        string relativePath = ServerUtil.MENU_VIDEO_UNITY_FOLDER_PATH.Substring("Assets/StreamingAssets".Length).TrimStart('/');
        return Path.Combine(Application.streamingAssetsPath, relativePath, fileName);
    }


    private string GetCachedEncryptedPath(string imageId)
    {
        string fileName = ServerUtil.GetVideoFileName(imageId);
        return Path.Combine(GetCacheDirectory(), fileName);
    }
    private string GetCacheDirectory()
    {
        return Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
    }


    #endregion

    #region 预加载角色选择图
    /// <summary>
    /// 当这个stage已经到第3张图的时候就需要加载后面的选择界面
    /// </summary>
    public void PreloadCharaterChoose()
    {
        var currentFinishProgress = VideoSerilNumberManager.Instance.GetCurrentStageProgress();
        if (currentFinishProgress.totalCount - currentFinishProgress.usedCount == 1)
        {
            var data = VideoSerilNumberManager.Instance.GetCandidatesForPreload();
            if (data != null)
            {
                foreach (var it in data)
                {
                    VideoImgResourceManager.Instance.GetImage(VideoSerilNumberManager.FormatMainIdFileId(it.mainId, it.fileRange[0]), null);
                }
            }

            PreloadSpecialImg();
            PreloadSameGirlChoose();
        }
    }
    #endregion


    public void PreloadSpecialImg()
    {
        // var specialOne = VideoSerilNumberManager.Instance.specialData.GetOneFileToUse();
        // if (specialOne != null)
        // {
        //     var fullFileId = VideoSerilNumberManager.FormatSurpriseMainIdFileId(specialOne.Value.mainId, specialOne.Value.fileId);
        //     VideoImgResourceManager.Instance.GetImage(fullFileId, null);
        // }
    }

    public void PreloadSameGirlChoose()
    {
        var currentFileName = VideoSerilNumberManager.Instance.GetCurrentFullName();
        var NotUseStage = VideoSerilNumberManager.Instance.GetAllUnusedStagesAndImages(currentFileName.Value.mainId);
        if (currentFileName == null || NotUseStage.Count < 1) return;

        for (int i = 0; i < 4; i++)
        {
            if (NotUseStage.ContainsKey(i))
            {
                int fileId = NotUseStage[i][0];
                VideoImgResourceManager.Instance.GetImage(VideoSerilNumberManager.FormatMainIdFileId(currentFileName.Value.mainId, fileId), null);
                break;
            }
        }
    }

}
