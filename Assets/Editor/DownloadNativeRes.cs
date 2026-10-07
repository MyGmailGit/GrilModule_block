using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utility;
using VideoSystem;

public class DownloadNativeRes : Editor
{
    [MenuItem("Tools/Download Native Res", false, 25)]
    private static void FindReferences()
    {
        string[] imgNames = new string[] { "6000106013", "6000154013", "6000204013", "6000242013", };
        if (Application.isPlaying)
        {
            // download png
            foreach (var it in imgNames)
            {
                var url = MakeDownloadUrl(it, true);
                PriorityDownloadManager.Instance.AddDownload(url, it, NotifyImageDownloaded, (str) =>
                {
                });
            }
            //download mp4
            foreach (var it in imgNames)
            {
                var url = MakeDownloadUrl(it, false);
                PriorityDownloadManager.Instance.AddDownload(url, it, NotifyVideoDownloaded, (str) =>
                {
                });
            }
        }
    }

    private static string MakeDownloadUrl(string fileId, bool isPng)
    {
        string fileName = fileId + (isPng ? ".png" : ".mp4");
        return VideoServerUrl.BASE_URL + fileName;
    }


    private static void NotifyImageDownloaded(string imageId, byte[] textureByte)
    {
        try
        {
            if (textureByte != null)
            {
                var DataTex = Utility.OpenSSLCryptoHelper.DecryptBytes(textureByte, string.Concat(ServerUtil.keyBaseUrl));
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(DataTex);

                CoroutineTool.Instance.StartACoroutine(VideoImgResourceManager.ResizeTexture(texture, imageId, (callTex, imgId) =>
                {

                    string directory = Path.Combine(Application.streamingAssetsPath, ServerUtil.NativePngPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    string filePath = Path.Combine(directory, imageId + ".png");

                    byte[] pngData = callTex.EncodeToPNG();
                    byte[] fileData = OpenSSLCryptoHelper.EncryptBytes(pngData, string.Concat(ServerUtil.keyBaseUrl));
                    File.WriteAllBytes(filePath, fileData);

                    Debug.Log($"下载成功{imageId}.png");
                }));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{imageId}:{e}");
        }
    }

    private static void NotifyVideoDownloaded(string imageId, byte[] textureByte)
    {
        string directory = Path.Combine(Application.streamingAssetsPath, ServerUtil.NativeVideoPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        string filePath = Path.Combine(directory, imageId + ".mp4");
        File.WriteAllBytes(filePath, textureByte);

        Debug.Log($"下载成功{imageId}.mp4");
    }
}
