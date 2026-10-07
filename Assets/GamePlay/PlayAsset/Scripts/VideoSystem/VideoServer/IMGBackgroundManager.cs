using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Drawing.Printing;
using Watermelon;

public class IMGBackgroundManager : MonoBehaviour
{
    [Header("UI组件")]
    public RawImage backgroundImage; // 用于显示背景的UI Image组件

    [Header("图片设置")]
    public const int StartNumber = 1; // 起始图片编号
    public const int EndNumber = 29;   // 结束图片编号

    private int totalImageCount = EndNumber - StartNumber + 1;    // 图片总数
    private string streamingAssetsPath;
    void Awake()
    {
        streamingAssetsPath = Application.streamingAssetsPath;
    }
    void OnDestroy()
    {
        ClearCache();
    }

    /// <summary>
    /// 根据等级显示对应的背景图片（异步加载）
    /// </summary>
    /// <param name="level">当前等级（从1开始）</param>
    /// <param name="onComplete">加载完成后的回调（可选）</param>
    public void SetBackgroundByLevel(RawImage rawImage, System.Action<bool> onComplete = null)
    {
        ActiveSession session = ActiveSession.Current;

        backgroundImage = rawImage;
        // if (backgroundImage == null)
        // {
        //     Debug.LogError("未指定Background Image组件！");
        //     onComplete?.Invoke(false);
        //     return;
        // }

        // 计算循环后的索引（超出范围则循环）
        int imageIndex;
        if (totalImageCount > 0)
        {
            // level从1开始，减1转为0起始索引，然后取模循环
            imageIndex = (session.DisplayLevelIndex) % totalImageCount;//(level - 1) % totalImageCount;
        }
        else
        {
            Debug.LogError("图片数量无效！");
            onComplete?.Invoke(false);
            return;
        }
        // 计算实际的图片编号
        int pictureNumber = StartNumber + imageIndex;


        // 构建平台兼容的路径
        string filePath = GetPlatformPath(pictureNumber);

        // 异步加载图片
        StartCoroutine(LoadImageFromPath(filePath, pictureNumber, onComplete));
    }

    /// <summary>
    /// 获取不同平台的正确路径
    /// </summary>
    private string GetPlatformPath(int pictureNumber)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // 编辑器或PC平台
        return System.IO.Path.Combine(streamingAssetsPath, $"BackgroundImg/{pictureNumber}.png");
#elif UNITY_ANDROID
            // Android平台需要使用jar:file://协议
            return System.IO.Path.Combine(streamingAssetsPath, $"BackgroundImg/{pictureNumber}.png");
#elif UNITY_IOS
            // iOS平台
            return System.IO.Path.Combine(streamingAssetsPath, $"BackgroundImg/{pictureNumber}.png");
#else
        return System.IO.Path.Combine(streamingAssetsPath, $"BackgroundImg/{pictureNumber}.png");
#endif
    }
    /// <summary>
    /// 协程：使用UnityWebRequest从指定路径加载图片
    /// </summary>
    private IEnumerator LoadImageFromPath(string path, int pictureNumber, System.Action<bool> onComplete)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 获取下载的纹理
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                if (texture != null)
                {
                    // Destroy(backgroundImage.texture);
                    // // 显示图片
                    // backgroundImage.texture = texture;
                    EnableImg(texture);

                    Debug.Log($"成功加载图片: {pictureNumber}.jpg");
                    onComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"纹理创建失败: {pictureNumber}.jpg");
                    onComplete?.Invoke(false);
                }
            }
            else
            {
                Debug.LogError($"加载图片失败: {path}\n错误信息: {request.error}");
                onComplete?.Invoke(false);
            }
        }
#else
        yield return null;
        byte[] data = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(data))
        {
            Destroy(texture);
            yield return null;
        }
        else
        {
            EnableImg(texture);
        }
#endif
    }

    private void EnableImg(Texture2D tex)
    {
        Destroy(backgroundImage.texture);
        // 显示图片
        backgroundImage.texture = tex;
        backgroundImage.enabled = true;
    }

    /// <summary>
    /// 清除缓存（释放内存）
    /// </summary>
    public void ClearCache()
    {
        Destroy(backgroundImage.texture);
        Debug.Log("缓存已清除");
    }

    /// <summary>
    /// 获取当前显示的图片编号
    /// </summary>
    public int GetCurrentPictureNumber(int level)
    {
        if (totalImageCount <= 0) return -1;
        int imageIndex = (level - 1) % totalImageCount;
        return StartNumber + imageIndex;
    }
}