using System;

namespace VideoSystem
{
    /// <summary>
    /// 视频下载开始事件
    /// </summary>
    public class VideoDownloadStartEvent
    {
        public int LevelId;
        public string Url;
        public VideoServerType ServerType;
        public DateTime StartTime;
    }

    /// <summary>
    /// 视频下载进度事件
    /// </summary>
    public class VideoDownloadProgressEvent
    {
        public int LevelId;
        public float Progress; // 0-1
        public long DownloadedBytes;
        public long TotalBytes;
    }

    /// <summary>
    /// 视频下载完成事件
    /// </summary>
    public class VideoDownloadCompleteEvent
    {
        public int LevelId;
        public bool Success;
        public string LocalPath;
        public string ErrorMessage;
        public DateTime CompleteTime;
        public TimeSpan Duration;
    }
    /// <summary>
    /// 所有服务器都下载失败事件（所有重试都失败）
    /// </summary>
    public class VideoDownloadAllServersFailedEvent
    {
        public int LevelId;
        public int TotalAttempts;        // 总尝试次数
        public int TotalServersAttempted; // 尝试了多少个服务器
        public string LastErrorMessage;   // 最后的错误信息
        public DateTime FailedTime;
    }
    /// <summary>
    /// 视频预加载事件
    /// </summary>
    public class VideoPreloadEvent
    {
        public int LevelId;
        public bool IsStart;
    }

    /// <summary>
    /// 缓存清理事件
    /// </summary>
    public class VideoCacheClearedEvent
    {
        public int LevelId; // -1 表示全部清理
        public long FreedBytes;
    }


}