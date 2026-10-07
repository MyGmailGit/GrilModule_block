using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Watermelon
{
    public static class GameGirlsVideoSaveUtility
    {
        private const string ALBUM_NAME = "BlockJam";

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _SaveVideoToPhotosAlbum(string videoPath);
#endif

        public static bool TrySaveVideoToDevice(string sourceVideoPath, string levelId, string fileId, out string message)
        {
            if (string.IsNullOrEmpty(sourceVideoPath) || !File.Exists(sourceVideoPath))
            {
                message = "Video resource is unavailable.";
                return false;
            }

            byte[] videoBytes;
            try
            {
                videoBytes = File.ReadAllBytes(sourceVideoPath);
                byte[] key = ServerUtil.GetEncryptionKey(levelId);
                videoBytes = Utility.CryptoHelper.XorQuick(videoBytes, key);
            }
            catch (Exception exception)
            {
                message = $"Failed to read video: {exception.Message}";
                return false;
            }

            string fileName = $"{(string.IsNullOrEmpty(fileId) ? "girl" : fileId)}";

#if UNITY_ANDROID && !UNITY_EDITOR
            return TrySaveOnAndroid(videoBytes, fileName, out message);
#elif UNITY_IOS && !UNITY_EDITOR
            return TrySaveOniOS(videoBytes, fileName, out message);
#else
            return TrySaveToAppFolder(videoBytes, fileName, out message);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static bool TrySaveOnAndroid(byte[] videoBytes, string fileName, out string message)
        {
            try
            {
                using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = versionClass.GetStatic<int>("SDK_INT");
                    if (sdkInt >= 29)
                    {
                        if (TrySaveToAndroidMediaStore(videoBytes, fileName))
                        {
                            message = "Video saved successfully.";
                            return true;
                        }
                    }
                }

                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
                    message = "Storage permission is required. Please try again.";
                    return false;
                }

                using (AndroidJavaClass environmentClass = new AndroidJavaClass("android.os.Environment"))
                {
                    string moviesDirectory = environmentClass.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", environmentClass.GetStatic<string>("DIRECTORY_MOVIES"))
                        .Call<string>("getAbsolutePath");

                    string albumDirectory = Path.Combine(moviesDirectory, ALBUM_NAME);
                    if (!Directory.Exists(albumDirectory))
                        Directory.CreateDirectory(albumDirectory);

                    string destinationPath = Path.Combine(albumDirectory, fileName);
                    File.WriteAllBytes(destinationPath, videoBytes);

                    using (AndroidJavaClass mediaScannerClass = new AndroidJavaClass("android.media.MediaScannerConnection"))
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        mediaScannerClass.CallStatic(
                            "scanFile",
                            activity,
                            new string[] { destinationPath },
                            new string[] { "video/mp4" },
                            null);
                    }

                    message = "Video saved successfully.";
                    return true;
                }
            }
            catch (Exception exception)
            {
                message = $"Failed to save video: {exception.Message}";
                return false;
            }
        }

        private static bool TrySaveToAndroidMediaStore(byte[] videoBytes, string fileName)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            using (AndroidJavaClass mediaStoreVideoMedia = new AndroidJavaClass("android.provider.MediaStore$Video$Media"))
            using (AndroidJavaObject contentValues = new AndroidJavaObject("android.content.ContentValues"))
            {
                contentValues.Call("put", "_display_name", fileName);
                contentValues.Call("put", "mime_type", "video/mp4");
                contentValues.Call("put", "relative_path", $"Movies/{ALBUM_NAME}");

                AndroidJavaObject contentUri = mediaStoreVideoMedia.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");
                AndroidJavaObject videoUri = resolver.Call<AndroidJavaObject>("insert", contentUri, contentValues);
                if (videoUri == null)
                    return false;

                using (AndroidJavaObject outputStream = resolver.Call<AndroidJavaObject>("openOutputStream", videoUri))
                {
                    if (outputStream == null)
                        return false;

                    outputStream.Call("write", videoBytes);
                    outputStream.Call("flush");
                }

                return true;
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private static bool TrySaveOniOS(byte[] videoBytes, string fileName, out string message)
        {
            try
            {
                string exportDirectory = Path.Combine(Application.persistentDataPath, "SavedGirls");
                if (!Directory.Exists(exportDirectory))
                    Directory.CreateDirectory(exportDirectory);

                string tempPath = Path.Combine(exportDirectory, fileName);
                File.WriteAllBytes(tempPath, videoBytes);
                _SaveVideoToPhotosAlbum(tempPath);

                message = "Video saved successfully.";
                return true;
            }
            catch (Exception exception)
            {
                message = $"Failed to save video: {exception.Message}";
                return false;
            }
        }
#endif

        private static bool TrySaveToAppFolder(byte[] videoBytes, string fileName, out string message)
        {
            try
            {
                string exportDirectory = Path.Combine(Application.persistentDataPath, "SavedGirls");
                if (!Directory.Exists(exportDirectory))
                    Directory.CreateDirectory(exportDirectory);

                string destinationPath = Path.Combine(exportDirectory, fileName);
                File.WriteAllBytes(destinationPath, videoBytes);

                message = $"Video saved to app storage: {destinationPath}";
                return true;
            }
            catch (Exception exception)
            {
                message = $"Failed to save video: {exception.Message}";
                return false;
            }
        }
    }
}
