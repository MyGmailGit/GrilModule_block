using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VideoSystem;

public static class ServerUtil
{
    public const string MENU_VIDEO_UNITY_FOLDER_PATH = "Assets/StreamingAssets/MenuBackgroundVideos";
    public const string NativeVideoPath = "MenuBackgroundVideos";
    public const string NativePngPath = "GallaryImg";
    public const string MENU_VIDEO_CACHE_FOLDER_NAME = "MenuBackgroundVideoCache";
    public const string STREAMING_ASSETS_CACHE_FOLDER_NAME = "StreamingAssets";
    public const string MENU_VIDEO_FILE_EXTENSION = ".mp4";
    // public const string DEFAULT_REMOTE_VIDEO_BASE_URL = "https://d38ry694xn4cmr.cloudfront.net/GameBlockJam1/";

    public static string[] keyBaseUrl = { "d", "V", "1", "S", "m", "x", "-", "M", "B", "J", "1", "c", "C", "S", "9", "0", "h", "d", "r", "3", "E", "-", "o", "P", "O", "v", "1", "k", "P", "d", "y", "g", "V", "W", "C", "c", "7", "w", "k", "N", "c", "q", "U" };

    public static string GetVideoFileName(string levelId)
    {
        return $"{levelId}{ServerUtil.MENU_VIDEO_FILE_EXTENSION}";
    }

    public static byte[] GetEncryptionKey(int levelId)
    {
        string keyString = $"{levelId}level{levelId}";
        return System.Text.Encoding.UTF8.GetBytes(keyString);
    }
    public static byte[] GetEncryptionKey(string key)
    {
        string keyString = $"{key}level{key}";
        return System.Text.Encoding.UTF8.GetBytes(keyString);
    }

    public static byte[] EncryptOrDecrypt(byte[] data, string keyArg)
    {
        byte[] key = GetEncryptionKey(keyArg);
        return Utility.CryptoHelper.XorQuick(data, key);
    }

    public static string MakePngDownloadUrl(string filefullName)
    {
        string fileName = filefullName + ".png";
        return VideoServerUrl.BASE_URL + fileName;
    }
    public static string MakeMP4DownloadUrl(string filefullName)
    {
        string fileName = filefullName + ".mp4";
        return VideoServerUrl.BASE_URL + fileName;
    }


    public static bool IsVideoFile(byte[] data)
    {
        if (data == null || data.Length < 12) return false;

        // 常见视频格式的文件头（Magic Number）
        // MP4/MOV: 00 00 00 xx 66 74 79 70 (ftyp)
        if (data[4] == 0x66 && data[5] == 0x74 && data[6] == 0x79 && data[7] == 0x70)
            return true;

        // // AVI: RIFFxxxxAVI
        // if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 && // RIFF
        //     data[8] == 0x41 && data[9] == 0x56 && data[10] == 0x49) // AVI
        //     return true;

        // // WebM: 1A 45 DF A3 (EBML头)
        // if (data[0] == 0x1A && data[1] == 0x45 && data[2] == 0xDF && data[3] == 0xA3)
        //     return true;

        // // FLV: FLV头
        // if (data[0] == 0x46 && data[1] == 0x4C && data[2] == 0x56)
        //     return true;

        // // MKV: 1A 45 DF A3 (和WebM一样，都是EBML)
        // if (data[0] == 0x1A && data[1] == 0x45 && data[2] == 0xDF && data[3] == 0xA3)
        //     return true;

        // // WMV: 30 26 B2 75 8E 66 CF 11 A6 D9 00 AA 00 62 CE 6C
        // if (data[0] == 0x30 && data[1] == 0x26 && data[2] == 0xB2 && data[3] == 0x75)
        //     return true;

        // // 3GP: 与MP4类似但可能有不同的ftype
        // if (data[4] == 0x66 && data[5] == 0x74 && data[6] == 0x79 && data[7] == 0x70)
        // {
        //     // 检查具体的ftype类型
        //     if (data[8] == 0x6D && data[9] == 0x70 && data[10] == 0x34 && data[11] == 0x31 || // mp41
        //         data[8] == 0x69 && data[9] == 0x73 && data[10] == 0x6F && data[11] == 0x6D) // isom
        //         return true;
        // }

        return false;
    }

    // internal const int QuickEncryptLength = 2048;
    // /// <summary>
    // /// 快速XOR加密（返回新数组）
    // /// </summary>
    // public static byte[] XorQuick(byte[] data, byte[] key)
    // {
    //     return XorRange(data, 0, QuickEncryptLength, key);
    // }
    // /// <summary>
    // /// XOR加密整个数组（返回新数组）
    // /// </summary>
    // public static byte[] XorAll(byte[] data, byte[] key)
    // {
    //     if (data == null) return null;
    //     return XorRange(data, 0, data.Length, key);
    // }

    // /// <summary>
    // /// XOR加密指定范围（返回新数组）
    // /// </summary>
    // public static byte[] XorRange(byte[] data, int startIndex, int length, byte[] key)
    // {
    //     if (data == null) return null;

    //     int dataLength = data.Length;
    //     byte[] result = new byte[dataLength];
    //     Array.Copy(data, 0, result, 0, dataLength);
    //     XorRangeInPlace(result, startIndex, length, key);
    //     return result;
    // }

    // /// <summary>
    // /// XOR加密指定范围（直接修改原数组）
    // /// </summary>
    // public static void XorRangeInPlace(byte[] data, int startIndex, int length, byte[] key)
    // {
    //     if (data == null) return;
    //     if (key == null) throw new Exception("XOR key cannot be null.");

    //     int keyLength = key.Length;
    //     if (keyLength <= 0) throw new Exception("XOR key length must be greater than 0.");

    //     if (startIndex < 0 || length < 0 || startIndex + length > data.Length)
    //         throw new Exception("Invalid start index or length.");

    //     int keyIndex = startIndex % keyLength;
    //     for (int i = startIndex; i < startIndex + length; i++)
    //     {
    //         data[i] ^= key[keyIndex++];
    //         keyIndex %= keyLength;
    //     }
    // }

}
