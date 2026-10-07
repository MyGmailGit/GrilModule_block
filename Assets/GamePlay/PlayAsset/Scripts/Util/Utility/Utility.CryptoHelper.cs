using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace Utility
{
    public static class CryptoHelper
    {
        // ========== XOR 加密相关方法 ==========


        internal const int QuickEncryptLength = 2048;
        /// <summary>
        /// 快速XOR加密（返回新数组）
        /// </summary>
        public static byte[] XorQuick(byte[] data, byte[] key)
        {
            return XorRange(data, 0, QuickEncryptLength, key);
        }
        /// <summary>
        /// XOR加密整个数组（返回新数组）
        /// </summary>
        public static byte[] XorAll(byte[] data, byte[] key)
        {
            if (data == null) return null;
            return XorRange(data, 0, data.Length, key);
        }

        /// <summary>
        /// XOR加密指定范围（返回新数组）
        /// </summary>
        public static byte[] XorRange(byte[] data, int startIndex, int length, byte[] key)
        {
            if (data == null) return null;

            int dataLength = data.Length;
            byte[] result = new byte[dataLength];
            Array.Copy(data, 0, result, 0, dataLength);
            XorRangeInPlace(result, startIndex, length, key);
            return result;
        }

        /// <summary>
        /// XOR加密指定范围（直接修改原数组）
        /// </summary>
        public static void XorRangeInPlace(byte[] data, int startIndex, int length, byte[] key)
        {
            if (data == null) return;
            if (key == null) throw new Exception("XOR key cannot be null.");

            int keyLength = key.Length;
            if (keyLength <= 0) throw new Exception("XOR key length must be greater than 0.");

            if (startIndex < 0 || length < 0 || startIndex + length > data.Length)
                throw new Exception("Invalid start index or length.");

            int keyIndex = startIndex % keyLength;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                data[i] ^= key[keyIndex++];
                keyIndex %= keyLength;
            }
        }


        // ========== AES 加密相关方法（带随机Salt） ==========

        private const int BitUnit = 8;
        private const int KeySizeBytes = 16;      // AES-128使用16字节密钥
        private const int IvSizeBytes = 16;        // AES的IV也是16字节
        private const int Iterations = 10000;      // PBKDF2迭代次数
        private const int SaltSizeBytes = 16;      // Salt大小16字节

        /// <summary>
        /// 加密字符串（返回Base64，包含随机Salt）
        /// </summary>
        public static string EncryptString(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(password))
                return string.Empty;

            try
            {
                // 生成随机Salt
                byte[] salt = GenerateRandomSalt();

                using (var aes = CreateAesWithSalt(password, salt))
                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    // 组合：Salt(16字节) + 加密数据
                    byte[] result = new byte[SaltSizeBytes + encryptedBytes.Length];
                    Array.Copy(salt, 0, result, 0, SaltSizeBytes);
                    Array.Copy(encryptedBytes, 0, result, SaltSizeBytes, encryptedBytes.Length);

                    return Convert.ToBase64String(result);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"EncryptString failed: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 解密字符串（从Base64，自动提取Salt）
        /// </summary>
        public static string DecryptString(string encryptedText, string password)
        {
            if (string.IsNullOrEmpty(encryptedText) || string.IsNullOrEmpty(password))
                return string.Empty;

            try
            {
                byte[] encryptedData = Convert.FromBase64String(encryptedText);

                // 提取Salt（前16字节）
                if (encryptedData.Length < SaltSizeBytes)
                    throw new Exception("Encrypted data too short");

                byte[] salt = new byte[SaltSizeBytes];
                Array.Copy(encryptedData, 0, salt, 0, SaltSizeBytes);

                // 提取真正的加密数据
                byte[] cipherData = new byte[encryptedData.Length - SaltSizeBytes];
                Array.Copy(encryptedData, SaltSizeBytes, cipherData, 0, cipherData.Length);

                using (var aes = CreateAesWithSalt(password, salt))
                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"DecryptString failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加密大字符串（使用流式处理，适合长文本，包含随机Salt）
        /// </summary>
        public static string EncryptStringStream(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(password))
                return string.Empty;

            try
            {
                byte[] salt = GenerateRandomSalt();

                using (var ms = new MemoryStream())
                {
                    // 先写入Salt
                    ms.Write(salt, 0, salt.Length);

                    using (var aes = CreateAesWithSalt(password, salt))
                    using (var encryptor = aes.CreateEncryptor())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"EncryptStringStream failed: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 解密大字符串（使用流式处理，自动提取Salt）
        /// </summary>
        public static string DecryptStringStream(string encryptedText, string password)
        {
            if (string.IsNullOrEmpty(encryptedText) || string.IsNullOrEmpty(password))
                return string.Empty;

            try
            {
                byte[] encryptedData = Convert.FromBase64String(encryptedText);

                if (encryptedData.Length < SaltSizeBytes)
                    throw new Exception("Encrypted data too short");

                // 提取Salt
                byte[] salt = new byte[SaltSizeBytes];
                Array.Copy(encryptedData, 0, salt, 0, SaltSizeBytes);

                using (var ms = new MemoryStream(encryptedData, SaltSizeBytes, encryptedData.Length - SaltSizeBytes))
                using (var aes = CreateAesWithSalt(password, salt))
                using (var decryptor = aes.CreateDecryptor())
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"DecryptStringStream failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加密字节数组（包含随机Salt）
        /// </summary>
        public static byte[] EncryptBytes(byte[] plainData, string password)
        {
            if (plainData == null || plainData.Length == 0 || string.IsNullOrEmpty(password))
                return plainData;

            try
            {
                byte[] salt = GenerateRandomSalt();

                using (var aes = CreateAesWithSalt(password, salt))
                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainData, 0, plainData.Length);

                    // 组合：Salt + 加密数据
                    byte[] result = new byte[SaltSizeBytes + encryptedBytes.Length];
                    Array.Copy(salt, 0, result, 0, SaltSizeBytes);
                    Array.Copy(encryptedBytes, 0, result, SaltSizeBytes, encryptedBytes.Length);

                    return result;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"EncryptBytes failed: {e.Message}");
                return plainData;
            }
        }

        /// <summary>
        /// 解密字节数组（自动提取Salt）
        /// </summary>
        public static byte[] DecryptBytes(byte[] encryptedData, string password)
        {
            if (encryptedData == null || encryptedData.Length == 0 || string.IsNullOrEmpty(password))
                return encryptedData;

            try
            {
                if (encryptedData.Length < SaltSizeBytes)
                    throw new Exception("Encrypted data too short");

                // 提取Salt
                byte[] salt = new byte[SaltSizeBytes];
                Array.Copy(encryptedData, 0, salt, 0, SaltSizeBytes);

                // 提取真正的加密数据
                byte[] cipherData = new byte[encryptedData.Length - SaltSizeBytes];
                Array.Copy(encryptedData, SaltSizeBytes, cipherData, 0, cipherData.Length);

                using (var aes = CreateAesWithSalt(password, salt))
                using (var decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"DecryptBytes failed: {e.Message}");
                return encryptedData;
            }
        }

        /// <summary>
        /// 生成随机Salt
        /// </summary>
        private static byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[SaltSizeBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// 使用指定的Salt创建AES实例
        /// </summary>
        private static AesManaged CreateAesWithSalt(string password, byte[] salt)
        {
            if (string.IsNullOrEmpty(password)) return null;
            if (salt == null || salt.Length == 0) throw new Exception("Salt cannot be null or empty");

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                AesManaged aes = new AesManaged();

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.BlockSize = 128;

                aes.Key = deriveBytes.GetBytes(KeySizeBytes);
                aes.IV = deriveBytes.GetBytes(IvSizeBytes);

                return aes;
            }
        }
    }
}
